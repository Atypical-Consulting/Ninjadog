---
title: Docker Generators
description: "Ninjadog Docker generators: Dockerfile with multi-stage build, docker-compose.yml with provider-aware database services, and .dockerignore."
layout: default
parent: Generators
nav_order: 8
---

# Docker Generators

Ninjadog generates Docker deployment files tailored to your project configuration. The generated files use multi-stage builds for optimized images and adapt the `docker-compose.yml` based on your configured database provider.

## DockerfileGenerator

| Scope | Single File |
|---|---|

Generates a multi-stage `Dockerfile` optimized for .NET 10 ASP.NET Core applications:

- **Build stage** -- Uses the .NET SDK image to restore, build, and publish
- **Runtime stage** -- Uses the lightweight ASP.NET runtime image

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["MyApp.csproj", "."]
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MyApp.Api.dll"]
```

{: .tip }
> The Dockerfile uses the project name from `config.name` for the `.csproj` reference and the root namespace for the entrypoint DLL.

## DockerComposeGenerator

| Scope | Single File |
|---|---|

Generates a `docker-compose.yml` file that varies based on the configured database provider. Each variant includes health checks, volume persistence, and environment-based configuration.

### SQLite (default)

A single-service setup with a mounted volume for the database file:

```yaml
services:
  myapp:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "8080:8080"
    volumes:
      - app-data:/app/data
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - Database__ConnectionString=Data Source=/app/data/MyApp.db

volumes:
  app-data:
```

### PostgreSQL

Adds a PostgreSQL 17 service with health checks and `depends_on`:

```yaml
services:
  myapp:
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
      - Database__ConnectionString=Host=db;Port=5432;Database=myapp;Username=postgres;Password=postgres

  db:
    image: postgres:17
    ports:
      - "5432:5432"
    volumes:
      - db-data:/var/lib/postgresql/data
    environment:
      - POSTGRES_DB=myapp
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 5s
      timeout: 5s
      retries: 5

volumes:
  db-data:
```

### SQL Server

Adds a SQL Server 2022 service with health checks:

```yaml
services:
  myapp:
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
      - Database__ConnectionString=Server=db;Database=myapp;User Id=sa;Password=Your_Strong_Password123;TrustServerCertificate=True

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
```

{: .warning }
> The generated `docker-compose.yml` uses default credentials for development convenience. **Always change passwords before deploying to production.**

## DockerIgnoreGenerator

| Scope | Single File |
|---|---|

Generates a `.dockerignore` file to exclude build artifacts, IDE files, and version control directories from the Docker build context. This reduces image size and speeds up builds.

---

## Next Steps

- [Data Layer Generators](/Ninjadog/generators/data-layer) -- Database provider configuration that drives docker-compose generation
- [Project Setup](/Ninjadog/generators/project-setup) -- Program.cs and appsettings.json generators
- [CLI Reference](/Ninjadog/cli) -- Run `ninjadog build` to generate all files including Docker
