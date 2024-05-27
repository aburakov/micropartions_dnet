using System.Text;
using System.Text.Json;
using Micropartions;
using Micropartions.Entity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigin",
        builderLocal => builderLocal
            .SetIsOriginAllowed(x => _ = true)
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();
app.UseCors("AllowAllOrigin");

app.MapGet("/micro", () => MicropartionsManager.GetJsonMicropartionsToFront(builder));

app.MapPost("/micro", (HttpRequest request) => MicropartionsManager.SaveMicropartionsFromFrontToDb(request,builder));


app.Run();