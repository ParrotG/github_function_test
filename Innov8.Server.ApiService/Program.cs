#region

using System.Security.Claims;
using Asp.Versioning;
using Innov8.Server.ApiService;
using Innov8.Server.DbManager.InnovDb;
using Innov8.Server.ServiceDefaults;
using Keycloak.AuthServices.Authentication;

#endregion

#region builder

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

builder.AddServiceDefaults();
services.AddApplicationOpenApi(configuration);

builder.AddNpgsqlDbContext<InnovDbContext>("postgresdb");

services.AddControllers();
services.AddProblemDetails();
services
  .AddApiVersioning(
    static options =>
    {
      options.AssumeDefaultVersionWhenUnspecified = true;
      options.ReportApiVersions = true;
      options.DefaultApiVersion = new ApiVersion(1, 0); // 默认版本为 1.0
      options.ReportApiVersions = true; // 返回响应头中报告可用的 API 版本
    })
  .AddApiExplorer(
    static options =>
    {
      // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
      // note: the specified format code will format the version as "'v'major[.minor][-status]"
      options.GroupNameFormat = "'v'VVV";

      // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
      // can also be used to control the format of the API version in route templates
      options.SubstituteApiVersionInUrl = true;
    })
  .EnableApiVersionBinding();

services
  .AddKeycloakWebApiAuthentication(
    configuration,
    static options =>
    {
      // options.Audience = "workspaces-client";
      options.Audience = "account";
      options.RequireHttpsMetadata = false;
    }
  );
services.AddAuthorization();

#endregion


#region app

var app = builder.Build();

app.UseApplicationOpenApi();
app.UseCors(Extensions.MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet(
    "/hello",
    (ClaimsPrincipal user)
      =>
    {
      app.Logger.LogInformation("{@User}", user.Identity!.Name);
      return new HelloMessage
      {
        Message =
          $"Hello World! {
            string.Join(",",
              user.Identities
                .First()
                .Claims
                .Where(static e => e.Type == "preferred_username"))}"
      };
    })
  .RequireAuthorization();


var summaries = new[]
{
  "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot",
  "Sweltering", "Scorching"
};
app.MapGet(
  "/weatherforecast",
  () =>
  {
    var forecast = Enumerable.Range(1, 5).Select(
        index =>
          new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
          ))
      .ToArray();
    return forecast;
  });

#endregion

app.Run();

#region other

namespace Innov8.Server.ApiService
{
  internal class HelloMessage
  {
    public string Message { get; set; } = "Hello World!";
  }

  internal record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string? Summary)
  {
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
  }
}

#endregion
