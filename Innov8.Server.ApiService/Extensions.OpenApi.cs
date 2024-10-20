#region

using Asp.Versioning.ApiExplorer;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Common;
using Microsoft.OpenApi.Models;

#endregion

namespace Innov8.Server.ApiService;

public static class ExtensionsOpenApi
{
  public static IServiceCollection AddApplicationOpenApi(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(
      c =>
      {
        #region Keycloak

        var keycloakOptions =
          configuration.GetKeycloakOptions<KeycloakAuthenticationOptions>()!;

        c.AddSecurityDefinition(
          "oidc",
          new OpenApiSecurityScheme
          {
            Name = "OIDC",
            Type = SecuritySchemeType.OpenIdConnect,
            OpenIdConnectUrl = new Uri(keycloakOptions.OpenIdConnectUrl!)
          }
        );

        c.AddSecurityRequirement(
          new OpenApiSecurityRequirement
          {
            {
              new OpenApiSecurityScheme
              {
                Reference = new OpenApiReference
                {
                  Type = ReferenceType.SecurityScheme,
                  Id = "oidc"
                }
              },
              Array.Empty<string>()
            }
          }
        );

        #endregion

        var versionProvider = services.BuildServiceProvider()
          .GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (
          var versionDescription in versionProvider.ApiVersionDescriptions)
        {
          c.SwaggerDoc(
            versionDescription.GroupName,
            new OpenApiInfo
            {
              Title = $"Innov8 API {versionDescription.ApiVersion}",
              Version = versionDescription.ApiVersion.ToString(),
              Description = $"API version {versionDescription.ApiVersion}",
              Contact = new OpenApiContact
              {
                Name = "Innov8 Group",
                Email = "innov8@example.com"
              }
            });
        }
      });
    return services;
  }

  public static IApplicationBuilder UseApplicationOpenApi(
    this IApplicationBuilder app)
  {
    app.UseSwagger();
    app.UseSwaggerUI(
      options =>
      {
        var versionProvider = app.ApplicationServices
          .GetRequiredService<IApiVersionDescriptionProvider>()
          .ApiVersionDescriptions;

        // build a swagger endpoint for each discovered API version
        foreach (var description in versionProvider)
        {
          var url = $"/swagger/{description.GroupName}/swagger.json";
          var name = description.GroupName.ToUpperInvariant();
          options.SwaggerEndpoint(url, name);
        }

        options.RoutePrefix = "swagger";
      });

    return app;
  }
}
