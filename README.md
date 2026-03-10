# Ninjadog

> **Stop writing boilerplate REST API code — let source generators do it at compile time.**

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

## Table of Contents

- [The Problem](#the-problem)
- [The Solution](#the-solution)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Usage](#usage)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [List of Generators](#list-of-generators)
- [Roadmap](#roadmap)
- [Stats](#stats)
- [Contributing](#contributing)
- [License](#license)

## The Problem

Writing boilerplate C# code for REST APIs is repetitive and error-prone — DTOs, mappers, validators, repositories, endpoints, API clients. Developers waste hours on mechanical plumbing code that follows predictable patterns, and every layer must stay in sync whenever the domain model changes.

## The Solution

**Ninjadog** uses C# Source Generators to eliminate REST API boilerplate at compile time. Define your domain entities with DDD patterns, add an attribute, and the entire API stack is generated — zero runtime cost, zero reflection, always in sync with your source.

```csharp
// 1. Define your domain entity
[Ninjadog]
public class Movie
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public int Year { get; set; }
}

// 2. Compile — Ninjadog generates:
//    - CRUD endpoints (Create, ReadAll, ReadOne, Update, Delete)
//    - DTOs, request/response contracts
//    - Repository + interface
//    - Service + interface
//    - Mappers (Domain <-> DTO <-> API Contract)
//    - Validators (Create, Update)
//    - OpenAPI summaries
//    - C# and TypeScript API clients
```

## Features

- [x] Generates full CRUD REST endpoints based on a DDD approach
  - Create, Read All, Read One, Update, Delete
- [x] Generates API clients in C# and TypeScript
  - C# client accessible via `/cs-client`
  - TypeScript client accessible via `/ts-client`
- [x] Generates DTOs, request/response contracts, and validators
- [x] Generates repositories, services, and mapper layers
- [x] Generates OpenAPI summaries for each endpoint
- [x] Database initializer and connection factory generation
- [x] CLI tooling for project scaffolding
- [x] SaaS web application frontend

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 10 |
| Language | C# 13 |
| Code Generation | Roslyn Source Generators |
| Architecture | Domain-Driven Design (DDD) |
| SaaS Frontend | Blazor |
| Build | Cake Frosting |

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later

### Installation

**Option 1 — NuGet Package** *(recommended)*

```bash
dotnet add package Ninjadog
```

**Option 2 — From Source**

```bash
git clone https://github.com/Atypical-Consulting/ninjadog.git
cd ninjadog
dotnet build
```

## Usage

### Basic Example

1. Create a new .NET 10 Web API project:

```bash
dotnet new web -n MyApi
cd MyApi
dotnet add package Ninjadog
```

2. Define your domain entity with the `[Ninjadog]` attribute:

```csharp
using Ninjadog;

[Ninjadog]
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

3. Build the project — all REST endpoints, contracts, repositories, services, mappers, and validators are generated automatically:

```bash
dotnet build
dotnet run
```

Your API is now live with full CRUD endpoints for `Product`.

## Architecture

```
┌─────────────────────────────────────────────────────┐
│                   Your C# Source                     │
│              [Ninjadog] Domain Entities               │
└──────────────────────┬──────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────┐
│                 Roslyn Compiler                       │
│            Ninjadog Source Generators                 │
└──────────────────────┬──────────────────────────────┘
                       │
          ┌────────────┼────────────┐
          ▼            ▼            ▼
   ┌────────────┐ ┌─────────┐ ┌──────────┐
   │ Contracts  │ │  Data   │ │ Clients  │
   │ Requests   │ │  DTOs   │ │ C# / TS  │
   │ Responses  │ │ Mappers │ │          │
   └────────────┘ └─────────┘ └──────────┘
          │            │            │
          ▼            ▼            ▼
   ┌────────────┐ ┌─────────┐ ┌──────────┐
   │ Endpoints  │ │  Repos  │ │Validators│
   │ CRUD       │ │Services │ │Summaries │
   └────────────┘ └─────────┘ └──────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────┐
│                  Compiled Assembly                    │
│           Full REST API — Zero Boilerplate            │
└─────────────────────────────────────────────────────┘
```

### Project Structure

```
ninjadog/
├── src/
│   ├── library/                        # Core generator libraries
│   │   ├── Ninjadog.Engine/            # Main source generator engine
│   │   ├── Ninjadog.Engine.Core/       # Core generator abstractions
│   │   ├── Ninjadog.Engine.Infrastructure/ # Infrastructure utilities
│   │   ├── Ninjadog.Helpers/           # Shared helper functions
│   │   ├── Ninjadog.Settings/          # Generator configuration
│   │   └── Ninjadog.Settings.Extensions/ # Settings extension methods
│   ├── tools/
│   │   └── Ninjadog.CLI/              # Command-line interface
│   ├── templates/
│   │   └── Ninjadog.Templates.CrudWebApi/ # CRUD Web API template
│   ├── saas/
│   │   ├── Ninjadog.SaaS/            # SaaS backend
│   │   ├── Ninjadog.SaaS.WebApp/     # SaaS Blazor frontend
│   │   └── Ninjadog.SaaS.ServiceDefaults/ # Service defaults
│   ├── tests/
│   │   └── Ninjadog.Tests/           # Unit tests
│   └── build/
│       └── Ninjadog.Build/           # Cake Frosting build scripts
├── doc/                               # Generator documentation
├── Ninjadog.sln                       # Solution file
└── global.json                        # .NET SDK version config
```

## List of Generators

### Ninjadog
- [x] [Ninjadog](./doc/generators/NinjadogGenerator.md) | Single File

### Ninjadog.Contracts.Data
- [x] [DtoGenerator](./doc/generators/DtoGenerator.md) | By Model

### Ninjadog.Contracts.Requests
- [x] [CreateRequestGenerator](./doc/generators/CreateRequestGenerator.md) | By Model
- [x] [DeleteRequestGenerator](./doc/generators/DeleteRequestGenerator.md) | By Model
- [x] [GetRequestGenerator](./doc/generators/GetRequestGenerator.md) | By Model
- [x] [UpdateRequestGenerator](./doc/generators/UpdateRequestGenerator.md) | By Model

### Ninjadog.Contracts.Responses
- [x] [GetAllResponseGenerator](./doc/generators/GetAllResponseGenerator.md) | By Model
- [x] [ResponseGenerator](./doc/generators/ResponseGenerator.md) | By Model

### Ninjadog.Database
- [x] [DatabaseInitializerGenerator](./doc/generators/DatabaseInitializerGenerator.md) | Single File
- [x] [DbConnectionFactoryGenerator](./doc/generators/DbConnectionFactoryGenerator.md) | Single File

### Ninjadog.Endpoints
- [x] [CreateEndpointGenerator](./doc/generators/CreateEndpointGenerator.md) | By Model
- [x] [DeleteEndpointGenerator](./doc/generators/DeleteEndpointGenerator.md) | By Model
- [x] [GetAllEndpointGenerator](./doc/generators/GetAllEndpointGenerator.md) | By Model
- [x] [GetEndpointGenerator](./doc/generators/GetEndpointGenerator.md) | By Model
- [x] [UpdateEndpointGenerator](./doc/generators/UpdateEndpointGenerator.md) | By Model

### Ninjadog.Mapping
- [x] [ApiContractToDomainMapperGenerator](./doc/generators/ApiContractToDomainMapperGenerator.md) | Single File
- [x] [DomainToApiContractMapperGenerator](./doc/generators/DomainToApiContractMapperGenerator.md) | Single File
- [x] [DomainToDtoMapperGenerator](./doc/generators/DomainToDtoMapperGenerator.md) | Single File
- [x] [DtoToDomainMapperGenerator](./doc/generators/DtoToDomainMapperGenerator.md) | Single File

### Ninjadog.Repositories
- [x] [RepositoryGenerator](./doc/generators/RepositoryGenerator.md) | By Model
- [x] [RepositoryInterfaceGenerator](./doc/generators/RepositoryInterfaceGenerator.md) | By Model

### Ninjadog.Services
- [x] [ServiceGenerator](./doc/generators/ServiceGenerator.md) | By Model
- [x] [ServiceInterfaceGenerator](./doc/generators/ServiceInterfaceGenerator.md) | By Model

### Ninjadog.Summaries
- [x] [CreateSummaryGenerator](./doc/generators/CreateSummaryGenerator.md) | By Model
- [x] [DeleteSummaryGenerator](./doc/generators/DeleteSummaryGenerator.md) | By Model
- [x] [GetAllSummaryGenerator](./doc/generators/GetAllSummaryGenerator.md) | By Model
- [x] [GetSummaryGenerator](./doc/generators/GetSummaryGenerator.md) | By Model
- [x] [UpdateSummaryGenerator](./doc/generators/UpdateSummaryGenerator.md) | By Model

### Ninjadog.Validation
- [x] [CreateRequestValidatorGenerator](./doc/generators/CreateRequestValidatorGenerator.md) | By Model
- [x] [UpdateRequestValidatorGenerator](./doc/generators/UpdateRequestValidatorGenerator.md) | By Model

## Roadmap

- [x] Solution that compiles
- [x] Branding — Name
- [ ] Branding — Logo
- [ ] Branding — TagLine
- [ ] Benefits of the solution
- [ ] Target audience definition
- [ ] Write documentation
- [ ] A client demo
- [ ] NuGet package publishing
- [x] CI/CD pipeline

> Want to contribute? Pick any roadmap item and open a PR!

## Stats

<!-- Get your hash from https://repobeats.axiom.co -->
![Alt](https://repobeats.axiom.co/api/embed/placeholder.svg "Repobeats analytics image")

## Contributing

Contributions are welcome! Here's how to get started:

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit using [conventional commits](https://www.conventionalcommits.org/) (`git commit -m 'feat: add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the Apache License 2.0 — see the [LICENSE](LICENSE) file for details.

---

Built with care by [Atypical Consulting](https://atypical.garry-ai.cloud) — opinionated, production-grade open source.

[![Contributors](https://contrib.rocks/image?repo=Atypical-Consulting/ninjadog)](https://github.com/Atypical-Consulting/ninjadog/graphs/contributors)
