using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Micropartions.DAO;
using Micropartions.Entity;
using Npgsql;

namespace Micropartions;

public class MicropartionsManager
{
    private static Dictionary<string, FrontEndBox> frontEndMicropartions;
    [JsonPropertyName("MicropartionGuid")] public string Micropartionguid { get; set; } = null!;
    [JsonPropertyName("BoxSerial")] public string? Boxserial { get; set; }
    [JsonPropertyName("SkuSerial")] public string Skuserial { get; set; } = null!;
    [JsonPropertyName("OperationGuid")] public string Operationguid { get; set; } = null!;
    [JsonPropertyName("OperationNumber")] public long Operationnumber { get; set; }

    /// <summary>
    /// Получаем из базы данных все микропартии, записываем их в List DbMicropartion
    /// </summary>
    /// <param name="connectionString">Строка параметров для подключения</param>
    /// <returns></returns>
    private static List<MicropartionsManager> GetAllMicropartionsFromDatabase(string connectionString, WebApplicationBuilder builder)
    {
        Stopwatch stopwatch = new Stopwatch();
        List<MicropartionsManager> allMicroPartions = new List<MicropartionsManager>();

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        var dataSource = dataSourceBuilder.Build();


        using var conn = dataSource.OpenConnection();

        string sqlRequest = "SELECT * FROM micropartions";
        using var cmd = new NpgsqlCommand(sqlRequest, conn);

        stopwatch.Start();
        using var sqlReader = cmd.ExecuteReader();
        while (sqlReader.Read())
            allMicroPartions.Add(new MicropartionsManager()
            {
                Micropartionguid = sqlReader.GetString(0),
                Boxserial = sqlReader.GetString(1),
                Skuserial = sqlReader.GetString(2),
                Operationguid = sqlReader.GetString(3),
                Operationnumber = sqlReader.GetInt32(4)
            });
        stopwatch.Stop();
        if (builder.Configuration["BtmcLogging:ShowTimers"]!.Equals("True"))
        {
            Console.WriteLine($"DBMS select request took: {stopwatch.Elapsed}");
        }
        
        return allMicroPartions;
    }

    /// <summary>
    /// Формируем JSON для фронта на основе полученных из БД данных
    /// </summary>
    /// <param name="builder">WebApplicationBuilder - для доступа к appsettings.json</param>
    /// <returns></returns>
    public static string GetJsonMicropartionsToFront(WebApplicationBuilder builder)
    {
        frontEndMicropartions = new Dictionary<string, FrontEndBox>();
        FrontEndOperation tempOperation = new FrontEndOperation();
        FrontEndSku tempFrontEndSku = new FrontEndSku();
        FrontEndBox tempFrontEndBox = new FrontEndBox();

        var allMicropartions = GetAllMicropartionsFromDatabase(DbConnector.GetConnectionString(false, builder), builder);


        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        foreach (var micropartion in allMicropartions)
        {
            // новая микропартия (значит и новый бокс - новый бокс не анализируем)
            if (!frontEndMicropartions.ContainsKey(micropartion.Micropartionguid))
            {
                tempOperation.OperationGuid = micropartion.Operationguid;
                tempOperation.OpertionNumber = micropartion.Operationnumber;

                tempFrontEndSku.SkusSerial = micropartion.Skuserial;
                tempFrontEndSku.PartionOperations = new List<FrontEndOperation>();
                tempFrontEndSku.PartionOperations.Add(tempOperation);

                tempFrontEndBox.BoxSerial = micropartion.Boxserial;
                tempFrontEndBox.Skus = new List<FrontEndSku>();
                tempFrontEndBox.Skus.Add(tempFrontEndSku);

                frontEndMicropartions.Add(micropartion.Micropartionguid, tempFrontEndBox);

                //TODO: нужно замерить просто обнуление значений или вовсе убрать обнуление
                tempOperation = new FrontEndOperation();
                tempFrontEndSku = new FrontEndSku();
                tempFrontEndBox = new FrontEndBox();
            }
            // микропартия есть, значит и бокс. анализируем сначала номеклатуру, если нет - создаем нову и новую операцию. если есть анализируем опеарции
            else
            {
                bool newSkuForFront = true;
                foreach (var frontEndSku in frontEndMicropartions[micropartion.Micropartionguid].Skus)
                {
                    if (frontEndSku.SkusSerial.Equals(micropartion.Skuserial))
                    {
                        newSkuForFront = false;

                        bool newFrontEndOperation = true;
                        foreach (var frontEndOperation in frontEndSku.PartionOperations)
                        {
                            if (frontEndOperation.OperationGuid.Equals(micropartion.Operationguid) && frontEndOperation.OpertionNumber == micropartion.Operationnumber)
                            {
                                newFrontEndOperation = false;
                                break;
                            }
                        }

                        if (newFrontEndOperation)
                        {
                            tempOperation.OperationGuid = micropartion.Operationguid;
                            tempOperation.OpertionNumber = micropartion.Operationnumber;

                            frontEndSku.PartionOperations.Add(tempOperation);
                            tempOperation = new FrontEndOperation();
                        }

                        break;
                    }
                }

                // новая номенклатура
                if (newSkuForFront)
                {
                    tempOperation.OperationGuid = micropartion.Operationguid;
                    tempOperation.OpertionNumber = micropartion.Operationnumber;

                    tempFrontEndSku.SkusSerial = micropartion.Skuserial;
                    tempFrontEndSku.PartionOperations = new List<FrontEndOperation>();
                    tempFrontEndSku.PartionOperations.Add(tempOperation);

                    frontEndMicropartions[micropartion.Micropartionguid].Skus.Add(tempFrontEndSku);

                    tempOperation = new FrontEndOperation();
                    tempFrontEndSku = new FrontEndSku();
                }
            }
        }

        stopwatch.Stop();
        if (builder.Configuration["BtmcLogging:ShowTimers"]!.Equals("True"))
        {
            Console.WriteLine($"JSON creating took: {stopwatch.Elapsed}");
        }

        return JsonSerializer.Serialize(frontEndMicropartions);
    }

    public static async void SaveMicropartionsFromFrontToDb(HttpRequest request, WebApplicationBuilder builder)
    {
        string micropartionguid, boxserial, skuserial, operationguid;
        long operationnumber;

        using var reader = new StreamReader(request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false);
        var bodyString = reader.ReadToEndAsync();
        try
        {
            List<MicropartionChangesFromFront>? fullOperationList = JsonSerializer.Deserialize<List<MicropartionChangesFromFront>>(bodyString.Result);

            Stopwatch stopwatch = new Stopwatch();

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(DbConnector.GetConnectionString(false, builder));
            var dataSource = dataSourceBuilder.Build();

            var conn = dataSource.OpenConnection();
            stopwatch.Start();

            string sqlRequest = "insert into public.micropartions (micropartionguid, boxserial, skuserial, operationguid, operationnumber) values ";
            // парсим объект в строку
            {
                for (var x = 0; x < fullOperationList.Count; x++)
                {
                    micropartionguid = fullOperationList[x].MicropartionGuid;
                    boxserial = fullOperationList[x].Box.BoxSerial;
                    for (var y = 0; y < fullOperationList[x].Box.Skus.Length; y++)
                    {
                        skuserial = fullOperationList[x].Box.Skus[y].SkuSerial;
                        operationguid = fullOperationList[x].Box.Skus[y].PartionOperations.OperationGuid;
                        operationnumber = fullOperationList[x].Box.Skus[y].PartionOperations.OperationNumber;

                        var requestAddon = $"('{micropartionguid}','{boxserial}', '{skuserial}', '{operationguid}', {operationnumber})";
                        sqlRequest += requestAddon;
                        if (y < fullOperationList[x].Box.Skus.Length - 1)
                        {
                            sqlRequest += ",";
                        }
                        else if (y == fullOperationList[x].Box.Skus.Length - 1 && x < fullOperationList.Count - 1)
                        {
                            sqlRequest += ",";
                        }
                        else if (x == fullOperationList.Count - 1 && y == fullOperationList[x].Box.Skus.Length - 1)
                        {
                            sqlRequest += "ON CONFLICT (micropartionguid,skuserial,operationguid,operationnumber) DO NOTHING";
                        }
                    }
                }
            }
            await using var cmd = new NpgsqlCommand(sqlRequest, conn);
            await cmd.ExecuteNonQueryAsync();
            stopwatch.Stop();

            if (builder.Configuration["BtmcLogging:ShowTimers"]!.Equals("True"))
            {
                Console.WriteLine($"DBMS insert request took: {stopwatch.Elapsed}");
            }
        }
        // прописать детальнее эксепшены
        catch (Exception e)
        {
            Console.WriteLine($"{DateTime.Now} : Can't deserialize front-end's data from JSON");
            throw;
        }
    }
}