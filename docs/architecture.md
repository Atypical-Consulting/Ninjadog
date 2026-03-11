---
title: Architecture
layout: default
nav_order: 3
---

# Architecture
{: .no_toc }

How Ninjadog's CLI turns a simple configuration file into a full REST API project.
{: .fs-6 .fw-300 }

<details open markdown="block">
  <summary>Table of contents</summary>
  {: .text-delta }
1. TOC
{:toc}
</details>

---

## How It Works

When you run the Ninjadog CLI, it reads your `ninjadog.json` configuration file, discovers all defined entities and their properties, and uses template-based code generation to emit C# source files for every layer of the API stack.

```mermaid
flowchart TB
    A["ninjadog.json<br/>Entity Definitions"] --> B["Ninjadog CLI<br/>Template Engine"]
    B --> C["Contracts<br/>Requests, Responses"]
    B --> D["Data Layer<br/>DTOs, Mappers"]
    B --> E["Clients<br/>C#, TypeScript"]
    C --> F["Endpoints<br/>CRUD"]
    D --> G["Repos<br/>Services"]
    E --> H["Validators<br/>OpenAPI"]
    F --> I["Generated .NET Project<br/>sln, csproj, Source Files"]
    G --> I
    H --> I
```

The key insight: **everything is generated before you build**. The CLI creates a complete .NET project -- solution file, project files, NuGet references, and all source files -- written to disk via `DiskOutputProcessor`. The generated code is plain C# that compiles like any hand-written code.

## Key Design Decisions

| Decision | Rationale |
|---|---|
| **CLI-based generation** | No runtime reflection, no startup penalty, full control over output |
| **Template engine (NinjadogTemplate subclasses)** | Uses `IndentedStringBuilder` for clean, readable generated code -- no Roslyn APIs needed |
| **Per-entity isolation** | Each entity gets independent files; no cross-entity coupling or shared state |
| **Convention over configuration** | Sensible defaults for routes, validation, and database schema -- zero config needed |
| **Type-aware output** | Route constraints, SQL column types, and validation rules adapt automatically to property types |

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

## Project Structure

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
├── docs/                                    # GitHub Pages documentation
├── doc/                                     # Generator reference docs
├── Ninjadog.sln                             # Solution file
└── global.json                              # .NET SDK version config
```

## NuGet Packages

| Package | Description |
|---|---|
| `Ninjadog.Engine` | Main code generation engine |
| `Ninjadog.Engine.Core` | Core generator abstractions |
| `Ninjadog.Engine.Infrastructure` | Infrastructure utilities |
| `Ninjadog.Helpers` | Shared helper functions |
| `Ninjadog.Settings` | Generator configuration |
| `Ninjadog.Settings.Extensions` | Settings extension methods |
| `Ninjadog.Templates.CrudWebApi` | CRUD Web API template |
| `Ninjadog.CLI` | Command-line tool (build from source) |

---

## Next Steps

- [Getting Started](/Ninjadog/getting-started) -- Build your first API
- [Generators](/Ninjadog/generators) -- Deep dive into all 30 generators
- [Generated Examples](/Ninjadog/examples) -- See real output code
