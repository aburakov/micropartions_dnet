namespace Micropartions.DAO;

public static class DbConnector
{
    /// <summary>
    /// Получаем строку параметров для подключения к СУБД
    /// Меняем параметры подключения в зависимости от среды исполнения
    /// </summary>
    /// <param name="isProd">Продакш сервер?</param>
    /// <param name="builder">Ссылка на WebApplicationBuilder</param>
    /// <returns></returns>
    public static string GetConnectionString(bool isProd, WebApplicationBuilder builder)
    {
        string connString = "";
        if (isProd)
        {
            connString =$"Host={builder.Configuration["ProdHost:Ip"]}:{builder.Configuration["ProdHost:Port"]};Username={builder.Configuration["ProdDatabaseCredentials:Username"]};Password={builder.Configuration["ProdDatabaseCredentials:Password"]};Database={builder.Configuration["DatabaseName"]}";
        }
        else
        {
            connString =$"Host={builder.Configuration["DevHost:Ip"]}:{builder.Configuration["DevHost:Port"]};Username={builder.Configuration["DevDatabaseCredentials:Username"]};Password={builder.Configuration["DevDatabaseCredentials:Password"]};Database={builder.Configuration["DatabaseName"]}";
        }

        return connString;
    }
}