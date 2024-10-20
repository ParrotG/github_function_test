#region

using Innov8.Server.DbManager;
using Innov8.Server.DbManager.InnovDb;
using Innov8.Server.ServiceDefaults;
using Microsoft.EntityFrameworkCore;

#endregion

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder
  .AddNpgsqlDbContext<InnovDbContext>(
    "postgresdb",
    null,
    static optionsBuilder =>
      optionsBuilder.UseNpgsql(
        static npgsqlBuilder =>
          npgsqlBuilder.MigrationsAssembly(
            typeof(Program).Assembly.GetName().Name)
      )
  );


// builder.Services.AddDbContext<InnovDbContext>(options =>
//   options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
//     b => b.MigrationsAssembly("Innov8.Server.DbManager")));

builder.Services.AddOpenTelemetry()
  .WithTracing(
    static tracing
      => tracing.AddSource(InnovDbInitializer.ActivitySourceName));

builder.Services.AddSingleton<InnovDbInitializer>();
builder.Services.AddHostedService(
  static sp => sp.GetRequiredService<InnovDbInitializer>());
builder.Services.AddHealthChecks()
  .AddCheck<InnovDbInitializerHealthCheck>("DbInitializer");

var app = builder.Build();

app.MapDefaultEndpoints();

await app.RunAsync();
