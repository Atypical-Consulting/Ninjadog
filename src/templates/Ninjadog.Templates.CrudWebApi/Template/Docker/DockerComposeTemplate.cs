namespace Ninjadog.Templates.CrudWebAPI.Template.Docker;

/// <summary>
/// This template generates a docker-compose.yml file for the Web API project.
/// </summary>
public class DockerComposeTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "DockerCompose";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var name = ninjadogSettings.Config.Name;
        var lowerName = name.ToLowerInvariant();
        var provider = ninjadogSettings.Config.DatabaseProvider;

        var content = provider switch
        {
            "postgresql" => GenerateWithPostgres(lowerName),
            "sqlserver" => GenerateWithSqlServer(lowerName),
            _ => GenerateWithSqlite(lowerName, name),
        };

        return CreateNinjadogContentFile("docker-compose.yml", content, false);
    }

    private static string GenerateWithSqlite(string serviceName, string name)
    {
        return $$"""
                 services:
                   {{serviceName}}:
                     build:
                       context: .
                       dockerfile: Dockerfile
                     ports:
                       - "8080:8080"
                     volumes:
                       - app-data:/app/data
                     environment:
                       - ASPNETCORE_ENVIRONMENT=Production
                       - Database__ConnectionString=Data Source=/app/data/{{name}}.db

                 volumes:
                   app-data:
                 """;
    }

    private static string GenerateWithPostgres(string lowerName)
    {
        return $$"""
                 services:
                   {{lowerName}}:
                     build:
                       context: .
                       dockerfile: Dockerfile
                     ports:
                       - "8080:8080"
                     depends_on:
                       db:
                         condition: service_healthy
                     environment:
                       - ASPNETCORE_ENVIRONMENT=Production
                       - Database__ConnectionString=Host=db;Port=5432;Database={{lowerName}};Username=postgres;Password=postgres

                   db:
                     image: postgres:17
                     ports:
                       - "5432:5432"
                     volumes:
                       - db-data:/var/lib/postgresql/data
                     environment:
                       - POSTGRES_DB={{lowerName}}
                       - POSTGRES_USER=postgres
                       - POSTGRES_PASSWORD=postgres
                     healthcheck:
                       test: ["CMD-SHELL", "pg_isready -U postgres"]
                       interval: 5s
                       timeout: 5s
                       retries: 5

                 volumes:
                   db-data:
                 """;
    }

    private static string GenerateWithSqlServer(string lowerName)
    {
        return $$"""
                 services:
                   {{lowerName}}:
                     build:
                       context: .
                       dockerfile: Dockerfile
                     ports:
                       - "8080:8080"
                     depends_on:
                       db:
                         condition: service_healthy
                     environment:
                       - ASPNETCORE_ENVIRONMENT=Production
                       - Database__ConnectionString=Server=db;Database={{lowerName}};User Id=sa;Password=Your_Strong_Password123;TrustServerCertificate=True

                   db:
                     image: mcr.microsoft.com/mssql/server:2022-latest
                     ports:
                       - "1433:1433"
                     volumes:
                       - db-data:/var/opt/mssql/data
                     environment:
                       - ACCEPT_EULA=Y
                       - MSSQL_SA_PASSWORD=Your_Strong_Password123
                     healthcheck:
                       test: /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Your_Strong_Password123" -C -Q "SELECT 1" -b
                       interval: 10s
                       timeout: 5s
                       retries: 5

                 volumes:
                   db-data:
                 """;
    }
}
