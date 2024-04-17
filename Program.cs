using System.Collections;
using System.Data;
using Micropartions;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

var connString = $"Host={builder.Configuration["Host:Ip"]}:{builder.Configuration["Host:Port"]};Username={builder.Configuration["DatabaseCredentials:Username"]};Password={builder.Configuration["DatabaseCredentials:Password"]};Database={builder.Configuration["DatabaseName"]}";

List<Micropartion> allMicroPartions = new List<Micropartion>();

app.MapGet("/micro", () =>
{
    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
    var dataSource = dataSourceBuilder.Build();

    var conn =  dataSource.OpenConnection();

    using var cmd = new NpgsqlCommand("SELECT * FROM micropartions", conn);
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
        allMicroPartions.Add(new Micropartion()
        {
            Micropartionguid = reader.GetString(0),
            Boxserial =reader.GetString(1),
            Skuserial = reader.GetString(2),
            Operationguid = reader.GetString(3),
            Operationnumber = reader.GetInt32(4)
        });
    Console.WriteLine("Finish");
});

app.Run();
