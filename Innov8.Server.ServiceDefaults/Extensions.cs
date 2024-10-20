#region

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

#endregion

namespace Innov8.Server.ServiceDefaults;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
  public static string MyAllowSpecificOrigins { get; } = "_devViteCorsOrigins";

  public static IHostApplicationBuilder AddServiceDefaults(
    this IHostApplicationBuilder builder)
  {
    builder.ConfigureOpenTelemetry();

    builder.AddDefaultHealthChecks();

    builder.Services.AddServiceDiscovery();

    builder.Services.ConfigureHttpClientDefaults(
      static http =>
      {
        // Turn on resilience by default
        http.AddStandardResilienceHandler();

        // Turn on service discovery by default
        http.AddServiceDiscovery();
      });

    builder.CorsConfiguration();

    return builder;
  }

  public static IHostApplicationBuilder CorsConfiguration(
    this IHostApplicationBuilder builder)
  {
    builder.Services.AddCors(
      static options =>
      {
        options.AddPolicy(
          MyAllowSpecificOrigins,
          static policy =>
          {
            policy
              .WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "http://localhost:3300",
                "https://localhost:3300")
              .AllowAnyHeader()
              .AllowAnyMethod();
            ;
          });
      });
    return builder;
  }

  public static IHostApplicationBuilder ConfigureOpenTelemetry(
    this IHostApplicationBuilder builder)
  {
    builder.Logging.AddOpenTelemetry(
      static logging =>
      {
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
      });

    builder.Services.AddOpenTelemetry()
      .WithMetrics(
        static metrics =>
        {
          metrics.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation();
        })
      .WithTracing(
        static tracing =>
        {
          tracing.AddAspNetCoreInstrumentation()
            // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
            //.AddGrpcClientInstrumentation()
            .AddHttpClientInstrumentation();
        });

    builder.AddOpenTelemetryExporters();

    return builder;
  }

  private static IHostApplicationBuilder AddOpenTelemetryExporters(
    this IHostApplicationBuilder builder)
  {
    var useOtlpExporter = !string.IsNullOrWhiteSpace(
      builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

    if (useOtlpExporter)
    {
      builder.Services.AddOpenTelemetry().UseOtlpExporter();
    }

    // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
    //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
    //{
    //    builder.Services.AddOpenTelemetry()
    //       .UseAzureMonitor();
    //}

    return builder;
  }

  public static IHostApplicationBuilder AddDefaultHealthChecks(
    this IHostApplicationBuilder builder)
  {
    builder.Services.AddHealthChecks()
      // Add a default liveness check to ensure app is responsive
      .AddCheck("self", static () => HealthCheckResult.Healthy(), ["live"]);

    return builder;
  }

  public static WebApplication MapDefaultEndpoints(this WebApplication app)
  {
    // Adding health checks endpoints to applications in non-development environments has security implications.
    // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
    if (app.Environment.IsDevelopment())
    {
      // All health checks must pass for app to be considered ready to accept traffic after starting
      app.MapHealthChecks("/health");

      // Only health checks tagged with the "live" tag must pass for app to be considered alive
      app.MapHealthChecks(
        "/alive",
        new HealthCheckOptions
        {
          Predicate = static r => r.Tags.Contains("live")
        });
    }

    return app;
  }
}