<p align="center">
  <img src="https://raw.githubusercontent.com/Atypical-Consulting/Ninjadog/dev/logo.png" alt="Ninjadog logo" width="256" />
</p>

# Ninjadog

> **One config. Full REST API. Zero boilerplate.**

<!-- Badges: Row 1 — Identity -->
[![Atypical-Consulting - ninjadog](https://img.shields.io/static/v1?label=Atypical-Consulting&message=ninjadog&color=blue&logo=github)](https://github.com/Atypical-Consulting/ninjadog)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple?logo=dotnet)](https://dotnet.microsoft.com/)
[![stars - ninjadog](https://img.shields.io/github/stars/Atypical-Consulting/ninjadog?style=social)](https://github.com/Atypical-Consulting/ninjadog)
[![forks - ninjadog](https://img.shields.io/github/forks/Atypical-Consulting/ninjadog?style=social)](https://github.com/Atypical-Consulting/ninjadog)

<!-- Badges: Row 2 — Activity -->
[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![GitHub tag](https://img.shields.io/github/tag/Atypical-Consulting/ninjadog?include_prereleases=&sort=semver&color=blue)](https://github.com/Atypical-Consulting/ninjadog/releases/)
[![issues - ninjadog](https://img.shields.io/github/issues/Atypical-Consulting/ninjadog)](https://github.com/Atypical-Consulting/ninjadog/issues)
[![GitHub pull requests](https://img.shields.io/github/issues-pr/Atypical-Consulting/ninjadog)](https://github.com/Atypical-Consulting/ninjadog/pulls)
[![GitHub last commit](https://img.shields.io/github/last-commit/Atypical-Consulting/ninjadog)](https://github.com/Atypical-Consulting/ninjadog/commits/dev)

---

## Quick Start

```bash
dotnet tool install -g Ninjadog
mkdir MyApi && cd MyApi
ninjadog init
```

Edit `ninjadog.json` to define your entities:

```json
{
  "config": {
    "name": "MyApi",
    "version": "1.0.0",
    "description": "My API",
    "rootNamespace": "MyApi",
    "outputPath": "src/applications/MyApi",
    "saveGeneratedFiles": true
  },
  "entities": {
    "Product": {
      "properties": {
        "Id": { "type": "Guid", "isKey": true },
        "Name": { "type": "string" },
        "Price": { "type": "decimal" }
      }
    }
  }
}
```

```bash
ninjadog build
cd src/applications/MyApi
dotnet run
```

That's it. You now have a full CRUD API with endpoints, DTOs, validation, repositories, services, mappers, and OpenAPI docs — all generated from a single JSON config.

---

## Table of Contents

- [The Problem](#the-problem)
- [Why Ninjadog?](#why-ninjadog)
- [What Gets Generated](#what-gets-generated)
- [Generated Output Examples](#generated-output-examples)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Usage](#usage)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Generators](#generators)
- [CLI](#cli)
- [Roadmap](#roadmap)
- [Contributing](#contributing)
- [License](#license)

## The Problem

Writing boilerplate C# code for REST APIs is repetitive and error-prone — DTOs, mappers, validators, repositories, endpoints, API clients. Developers waste hours on mechanical plumbing code that follows predictable patterns, and every layer must stay in sync whenever the domain model changes.

## Why Ninjadog?

| | Without Ninjadog | With Ninjadog |
|---|---|---|
| **Code you write** | ~500+ lines per entity | ~10 lines of JSON per entity |
| **Files to maintain** | 20+ per entity | 1 config file |
| **Layers in sync** | Manual | Automatic |
| **Runtime cost** | Depends on approach | Zero (compile-time) |
| **Reflection** | Often required | None |
| **Time to full CRUD** | Hours | Seconds |

Ninjadog uses **template-based code generation** via its CLI to produce your entire API stack before you build. No runtime reflection, no files to keep in sync. Change your config, regenerate, done.

## What Gets Generated

For **each** entity defined in `ninjadog.json`, the generator produces:

| Category | Generated Files | Description |
|---|---|---|
| **Endpoints** | 5 | Create, GetAll (paginated), GetOne, Update, Delete |
| **Contracts** | 7 | DTOs, request objects, response objects |
| **Data Layer** | 4 | Repository + interface, service + interface |
| **Mapping** | 4 | Domain-to-DTO, DTO-to-Domain, Domain-to-Contract, Contract-to-Domain |
| **Validation** | 2 | Create + Update request validators |
| **OpenAPI** | 5 | Summaries for each endpoint |
| **Database** | 2 | Initializer + connection factory |
| **Clients** | 2 | C# and TypeScript API clients |
| **Total** | **~30 files** | **From a single annotated class** |

### Generated HTTP Endpoints

```
POST   /products              Create a new product
GET    /products              List all products (paginated: ?page=1&pageSize=10)
GET    /products/{id:guid}    Get a single product
PUT    /products/{id:guid}    Update a product
DELETE /products/{id:guid}    Delete a product
```

Route constraints are dynamic — `:guid`, `:int`, or untyped — based on your entity's key type.

## Generated Output Examples

All examples below are **real generated code** from Ninjadog's verified snapshot tests.

<details>
<summary><strong>Endpoint — GetAll with pagination</strong></summary>

```csharp
public partial class GetAllTodoItemsEndpoint(ITodoItemService todoItemService)
    : EndpointWithoutRequest<GetAllTodoItemsResponse>
{
    public override void Configure()
    {
        Get("/todo-items");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var page = int.TryParse(HttpContext.Request.Query["page"], out var p) && p > 0 ? p : 1;
        var pageSize = int.TryParse(HttpContext.Request.Query["pageSize"], out var ps) && ps > 0 ? ps : 10;

        var (todoItems, totalCount) = await todoItemService.GetAllAsync(page, pageSize);
        var todoItemsResponse = todoItems.ToTodoItemsResponse(page, pageSize, totalCount);
        await SendOkAsync(todoItemsResponse, ct);
    }
}
```
</details>

<details>
<summary><strong>Endpoint — GetOne with route constraint</strong></summary>

```csharp
public partial class GetTodoItemEndpoint(ITodoItemService todoItemService)
    : Endpoint<GetTodoItemRequest, TodoItemResponse>
{
    public override void Configure()
    {
        Get("/todo-items/{id:guid}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetTodoItemRequest req, CancellationToken ct)
    {
        var todoItem = await todoItemService.GetAsync(req.Id);

        if (todoItem is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var todoItemResponse = todoItem.ToTodoItemResponse();
        await SendOkAsync(todoItemResponse, ct);
    }
}
```
</details>

<details>
<summary><strong>Validator — type-aware (skips value types)</strong></summary>

```csharp
public partial class CreateTodoItemRequestValidator : Validator<CreateTodoItemRequest>
{
    public CreateTodoItemRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required!");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required!");

        RuleFor(x => x.DueDate)
            .NotEmpty()
            .WithMessage("DueDate is required!");

        // IsCompleted (bool), Priority (int), Cost (decimal) — skipped, value types always have defaults
    }
}
```
</details>

<details>
<summary><strong>Database — SQLite schema with type-aware columns</strong></summary>

```csharp
public partial class DatabaseInitializer(IDbConnectionFactory connectionFactory)
{
    public async Task InitializeAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();

        await connection.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS TodoItems (
            Id CHAR(36) PRIMARY KEY,
            Title TEXT NOT NULL,
            Description TEXT NOT NULL,
            IsCompleted INTEGER NOT NULL,
            DueDate TEXT NOT NULL,
            Priority INTEGER NOT NULL,
            Cost REAL NOT NULL)");
    }
}
```
</details>

## Features

- **Full CRUD generation** — Create, Read All (paginated), Read One, Update, Delete
- **API clients** — C# client via `/cs-client`, TypeScript client via `/ts-client`
- **Type-aware validation** — Value types (`int`, `bool`, `decimal`) skip `.NotEmpty()` rules automatically
- **Dynamic entity keys** — Supports `Guid`, `int`, `string` keys with any property name (not hardcoded to `Id`)
- **Type-aware database schema** — SQLite columns map correctly (`INTEGER`, `REAL`, `TEXT`, `CHAR(36)`)
- **Dynamic route constraints** — Routes use `:guid`, `:int`, etc. based on key type
- **Pagination** — `?page=1&pageSize=10` with `TotalCount` metadata in responses
- **OpenAPI summaries** — Each endpoint gets Swagger documentation
- **Database initializer** — Schema creation and connection factory generation
- **CLI tooling** — Project scaffolding and code generation commands
- **Snapshot tested** — 14 Verify snapshot tests cover template output correctness

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10, C# 13 |
| Code Generation | Template-based Code Generation |
| API Framework | FastEndpoints |
| Database | SQLite + Dapper |
| Validation | FluentValidation |
| OpenAPI | FastEndpoints.Swagger |
| Client Generation | FastEndpoints.ClientGen |
| Architecture | Domain-Driven Design (DDD) |
| CLI | Spectre.Console |

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later

### Installation

**Option 1 — Global CLI tool** *(recommended)*

```bash
dotnet tool install -g Ninjadog
```

**Option 2 — From Source**

```bash
git clone https://github.com/Atypical-Consulting/ninjadog.git
cd ninjadog
dotnet build
```

## Usage

### Basic Example

1. Initialize a new project:

```bash
mkdir MyApi && cd MyApi
ninjadog init
```

2. Edit `ninjadog.json` to define your entities:

```json
{
  "config": {
    "name": "MyApi",
    "version": "1.0.0",
    "description": "My API",
    "rootNamespace": "MyApi",
    "outputPath": "src/applications/MyApi",
    "saveGeneratedFiles": true
  },
  "entities": {
    "Product": {
      "properties": {
        "Id": { "type": "Guid", "isKey": true },
        "Name": { "type": "string" },
        "Price": { "type": "decimal" }
      }
    }
  }
}
```

3. Generate and run — all REST endpoints, contracts, repositories, services, mappers, and validators are produced automatically:

```bash
ninjadog build
cd src/applications/MyApi
dotnet run
```

Your API is now live with full CRUD endpoints for `Product`.

### Multiple Entities

Each entity gets its own isolated set of generated files. Add as many entities as you need in `ninjadog.json`:

```json
{
  "entities": {
    "Movie": {
      "properties": {
        "Id": { "type": "Guid", "isKey": true },
        "Title": { "type": "string" },
        "Year": { "type": "int" }
      }
    },
    "Order": {
      "properties": {
        "OrderId": { "type": "int", "isKey": true },
        "CustomerName": { "type": "string" },
        "Total": { "type": "decimal" }
      }
    }
  }
}
```

## Architecture

```
┌───────────────────────────────────────────────┐
│            ninjadog.json                       │
│         Entity Definitions                    │
└──────────────────┬────────────────────────────┘
                   │
                   ▼
┌───────────────────────────────────────────────┐
│            Ninjadog CLI                       │
│       Template-based Code Generation          │
└──────────────────┬────────────────────────────┘
                   │
     ┌─────────────┼─────────────┐
     ▼             ▼             ▼
┌──────────┐ ┌──────────┐ ┌──────────┐
│Contracts │ │   Data   │ │ Clients  │
│ Requests │ │   DTOs   │ │  C# /TS  │
│Responses │ │  Mappers │ │          │
└──────────┘ └──────────┘ └──────────┘
     │             │             │
     ▼             ▼             ▼
┌──────────┐ ┌──────────┐ ┌──────────┐
│Endpoints │ │  Repos   │ │Validators│
│  CRUD    │ │ Services │ │ OpenAPI  │
└──────────┘ └──────────┘ └──────────┘
                   │
                   ▼
┌───────────────────────────────────────────────┐
│        Generated .NET Project                 │
│      Full REST API — Zero Boilerplate         │
└───────────────────────────────────────────────┘
```

### Project Structure

```
ninjadog/
├── src/
│   ├── library/                             # Core generator libraries
│   │   ├── Ninjadog.Engine/                 # Main code generation engine
│   │   ├── Ninjadog.Engine.Core/            # Core generator abstractions
│   │   ├── Ninjadog.Engine.Infrastructure/  # Infrastructure utilities
│   │   ├── Ninjadog.Helpers/                # Shared helper functions
│   │   ├── Ninjadog.Settings/               # Generator configuration
│   │   └── Ninjadog.Settings.Extensions/    # Settings extension methods
│   ├── tools/
│   │   └── Ninjadog.CLI/                    # Command-line interface
│   ├── templates/
│   │   └── Ninjadog.Templates.CrudWebApi/   # CRUD Web API template
│   └── tests/
│       └── Ninjadog.Tests/                  # Snapshot + unit tests
├── doc/                                     # Generator documentation
├── Ninjadog.sln                             # Solution file
└── global.json                              # .NET SDK version config
```

## Generators

Ninjadog includes **30 generators** organized into 11 categories. Each generator produces either a single shared file or a per-entity file.

| Category | Generators | Scope |
|---|---|---|
| **Core** | NinjadogGenerator | Single file |
| **Contracts — Data** | DtoGenerator | Per entity |
| **Contracts — Requests** | Create, Delete, Get, Update | Per entity |
| **Contracts — Responses** | GetAllResponse, Response | Per entity |
| **Database** | DatabaseInitializer, DbConnectionFactory | Single file |
| **Endpoints** | Create, Delete, GetAll, Get, Update | Per entity |
| **Mapping** | ApiContract-to-Domain, Domain-to-ApiContract, Domain-to-Dto, Dto-to-Domain | Single file |
| **Repositories** | Repository, RepositoryInterface | Per entity |
| **Services** | Service, ServiceInterface | Per entity |
| **Summaries** | Create, Delete, GetAll, Get, Update | Per entity |
| **Validation** | CreateRequestValidator, UpdateRequestValidator | Per entity |

Full documentation for each generator is available in [`doc/generators/`](./doc/generators/).

## CLI

Install the CLI as a global dotnet tool:

```bash
dotnet tool install -g Ninjadog
```

Available commands:

```bash
ninjadog init              # Initialize a new Ninjadog project
ninjadog build             # Build and run the generator engine
ninjadog ninjadog          # Generate a new Ninjadog project
```

## Roadmap

- [x] Solution that compiles
- [x] Branding — Name
- [x] Type-aware code generation
- [x] Dynamic entity key support
- [x] Pagination support
- [x] CLI build command
- [x] Template snapshot tests
- [x] CI/CD pipeline
- [ ] Branding — Logo
- [ ] Branding — Tagline
- [ ] Benefits of the solution
- [ ] Target audience definition
- [ ] Write documentation
- [ ] A client demo
- [x] NuGet package publishing

> Want to contribute? Pick any roadmap item and open a PR!

## Contributing

Contributions are welcome! Here's how to get started:

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Run the tests to make sure everything works:
   ```bash
   dotnet test
   ```
4. Commit using [conventional commits](https://www.conventionalcommits.org/) (`git commit -m 'feat: add amazing feature'`)
5. Push to the branch (`git push origin feature/amazing-feature`)
6. Open a Pull Request

### Development Setup

```bash
git clone https://github.com/Atypical-Consulting/ninjadog.git
cd ninjadog
dotnet build
dotnet test
```

### Where to Look

- **Generator templates** — `src/templates/Ninjadog.Templates.CrudWebApi/`
- **Snapshot tests** — `src/tests/Ninjadog.Tests/Templates/`
- **CLI commands** — `src/tools/Ninjadog.CLI/`
- **Generator docs** — `doc/generators/`

## License

This project is licensed under the Apache License 2.0 — see the [LICENSE](LICENSE) file for details.

---

Built with care by [Atypical Consulting](https://atypical.garry-ai.cloud) — opinionated, production-grade open source.

[![Contributors](https://contrib.rocks/image?repo=Atypical-Consulting/ninjadog)](https://github.com/Atypical-Consulting/ninjadog/graphs/contributors)
