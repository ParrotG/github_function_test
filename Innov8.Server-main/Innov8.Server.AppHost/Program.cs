#region

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Projects;

#endregion

var builder = DistributedApplication.CreateBuilder(args);

var exeDirectory = AppContext.BaseDirectory;

builder.Services
  .AddDataProtection()
  .PersistKeysToFileSystem(
    new DirectoryInfo(
      $"{exeDirectory}/server/share/directory"
        .Replace('/', Path.DirectorySeparatorChar)
    )
  );

var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin(
      static c =>
        c.WithHostPort(5050)
    )
  ;


var postgresDb =
  postgres.AddDatabase("postgresdb");

var keycloak = builder
    .AddKeycloakContainer("keycloak")
    .WithDataVolume()
    .WithImport("./KeycloakConfiguration/Test-realm.json")
    .WithImport("./KeycloakConfiguration/Test-users-0.json")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    .WithArgs("--hostname-strict=false")
    .WithArgs("--hostname-strict-https=false")
    .WithArgs("--http-enabled=true")
    .WithArgs("--proxy=edge")
  // .WithArgs("--https=false")
  ;

var realm = keycloak.AddRealm("Test");

var apiService = builder
    .AddProject<Innov8_Server_ApiService>("apiservice")
    .WithReference(keycloak)
    .WithReference(realm)
    .WithReference(postgresDb)
  ;

builder
  .AddProject<Innov8_Server_DbManager>("dbmanager")
  .WithReference(postgresDb);

builder.Build().Run();
