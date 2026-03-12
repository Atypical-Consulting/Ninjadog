---
title: Configuration Reference
description: "Complete ninjadog.json configuration reference: project settings, CORS, features, database providers, entities, enums, relationships, seed data, and validation rules."
layout: default
nav_order: 4
---

# Configuration Reference
{: .no_toc }

Everything Ninjadog generates is driven by a single `ninjadog.json` file in your project root.
{: .fs-6 .fw-300 }

<details open markdown="block">
  <summary>Table of contents</summary>
  {: .text-delta }
1. TOC
{:toc}
</details>

---

## Quick Reference

The following skeleton shows **every** configuration option at a glance. Required fields are marked with a comment. All other fields are optional.

```json
{
  "$schema": "./ninjadog.schema.json",
  "config": {
    "name": "MyApi",                          // required
    "version": "1.0.0",                       // required
    "description": "My REST API",             // required
    "rootNamespace": "MyApi",                 // required
    "outputPath": "src/applications/MyApi",   // default: "."
    "saveGeneratedFiles": true,               // default: true
    "cors": {
      "origins": ["https://example.com"],     // required if cors is set
      "methods": ["GET", "POST"],             // optional
      "headers": ["Content-Type"]             // optional
    },
    "features": {
      "softDelete": false,                    // default: false
      "auditing": false,                      // default: false
      "aot": false                            // default: false
    },
    "database": {
      "provider": "sqlite"                    // "sqlite" | "postgresql" | "sqlserver"
    },
    "auth": {
      "provider": "jwt",                      // only "jwt" supported
      "issuer": "https://myapp.com",          // default: "https://localhost"
      "audience": "myapp-api",                // default: "api"
      "tokenExpirationMinutes": 60,           // default: 60
      "roles": ["Admin", "User"],             // optional
      "generateLoginEndpoint": true,          // default: true
      "generateRegisterEndpoint": true        // default: true
    },
    "rateLimit": {
      "permitLimit": 100,                     // default: 100
      "windowSeconds": 60,                    // default: 60
      "segmentsPerWindow": 6                  // default: 6
    }
  },
  "enums": {
    "Priority": ["Low", "Medium", "High"]
  },
  "entities": {
    "Product": {
      "properties": {
        "Id": { "type": "Guid", "isKey": true },
        "Name": { "type": "string", "required": true, "minLength": 1, "maxLength": 200 },
        "Price": { "type": "decimal", "min": 0, "max": 9999 },
        "Email": { "type": "string", "pattern": "^[^@]+@[^@]+\\.[^@]+$" },
        "Status": { "type": "Priority" }
      },
      "relationships": {
        "Orders": { "relatedEntity": "Order", "type": "OneToMany" }
      },
      "seedData": [
        { "Id": "...", "Name": "Sample", "Price": 9.99, "Email": "a@b.com", "Status": 0 }
      ]
    }
  }
}
```

{: .tip }
> New to Ninjadog? Start with the [Getting Started](/Ninjadog/getting-started) tutorial -- it walks you through creating your first API from a minimal config. Come back here when you need the full reference.

## Overview

The `ninjadog.json` file is the central configuration for your project. It defines the project metadata, feature flags, entity schemas, enums, and seed data. Every `ninjadog build` invocation reads this file and produces the corresponding .NET source code.

## JSON Schema

Ninjadog ships a JSON Schema file alongside your configuration. Adding a `$schema` property to your `ninjadog.json` enables autocomplete, inline documentation, and validation in editors that support JSON Schema (VS Code, JetBrains IDEs, and others).

```json
{
  "$schema": "./ninjadog.schema.json",
  "config": { ... },
  "entities": { ... }
}
```

{: .tip }
> Run `ninjadog init` to generate both `ninjadog.json` and `ninjadog.schema.json` automatically. If you already have a project, run `ninjadog validate` to check your configuration against the schema.

## Config Section

The `config` object contains project-level settings.

```json
{
  "config": {
    "name": "MyApi",
    "version": "1.0.0",
    "description": "My REST API",
    "rootNamespace": "MyApi",
    "outputPath": "src/applications/MyApi",
    "saveGeneratedFiles": true,
    "cors": { ... },
    "features": { ... },
    "database": { ... },
    "auth": { ... },
    "rateLimit": { ... }
  }
}
```

### Core Fields

| Field | Type | Required | Default | Description |
|---|---|---|---|---|
| `name` | `string` | Yes | -- | Project name used in the generated `.sln` and `.csproj` files. |
| `version` | `string` | Yes | -- | Semantic version of your API project. |
| `description` | `string` | Yes | -- | Short description included in the generated project metadata. |
| `rootNamespace` | `string` | Yes | -- | Root C# namespace for all generated code. |
| `outputPath` | `string` | No | `"."` | Directory where generated files are written, relative to `ninjadog.json`. |
| `saveGeneratedFiles` | `boolean` | No | `true` | When `true`, generated files are saved to disk. |

### CORS

The optional `cors` object configures the CORS policy applied to the generated API.

| Field | Type | Required | Description |
|---|---|---|---|
| `origins` | `string[]` | Yes | Allowed origins for cross-origin requests. |
| `methods` | `string[]` | No | Allowed HTTP methods. When omitted, all methods are allowed. |
| `headers` | `string[]` | No | Allowed request headers. When omitted, all headers are allowed. |

{: .note }
> If you omit the `cors` block entirely, Ninjadog defaults to allowing `https://localhost:7270` as the only origin.

### Features

The optional `features` object enables cross-cutting concerns.

| Field | Type | Default | Description |
|---|---|---|---|
| `softDelete` | `boolean` | `false` | Adds `IsDeleted` and `DeletedAt` columns; replaces DELETE with UPDATE. |
| `auditing` | `boolean` | `false` | Adds `CreatedAt` and `UpdatedAt` columns managed automatically by the repository. |
| `aot` | `boolean` | `false` | Enables Native AOT publishing support. See [AOT Support](#native-aot-support) below. |

### Database

The optional `database` object controls the target database provider.

| Field | Type | Default | Description |
|---|---|---|---|
| `provider` | `string` | `"sqlite"` | Database provider. Supported values: `sqlite`, `postgresql`, `sqlserver`. |

{: .note }
> The provider affects SQL dialect (e.g., `LIMIT` vs `OFFSET/FETCH`), column type mappings, timestamp functions, NuGet dependencies, and the generated connection factory class.

### Authentication

The optional `auth` object enables JWT authentication and authorization for the generated API.

```json
{
  "config": {
    "auth": {
      "provider": "jwt",
      "issuer": "https://myapp.com",
      "audience": "myapp-api",
      "tokenExpirationMinutes": 120,
      "roles": ["Admin", "User"],
      "generateLoginEndpoint": true,
      "generateRegisterEndpoint": true
    }
  }
}
```

| Field | Type | Default | Description |
|---|---|---|---|
| `provider` | `string` | `"jwt"` | Authentication provider. Currently only `jwt` is supported. |
| `issuer` | `string` | `"https://localhost"` | JWT token issuer URL. |
| `audience` | `string` | `"api"` | JWT token audience identifier. |
| `tokenExpirationMinutes` | `integer` | `60` | Token lifetime in minutes. |
| `roles` | `string[]` | -- | Role names for authorization policies. When set, generates named policies. |
| `generateLoginEndpoint` | `boolean` | `true` | Generates a `POST /api/auth/login` endpoint. |
| `generateRegisterEndpoint` | `boolean` | `true` | Generates a `POST /api/auth/register` endpoint. |

When auth is enabled:
- **GET/GetAll endpoints** remain `AllowAnonymous()` (public reads)
- **Create/Update/Delete endpoints** require a valid JWT token
- **Login/Register endpoints** are `AllowAnonymous()`
- A `Users` table is created automatically at startup
- Passwords are hashed with BCrypt
- An `appsettings.json` `Jwt` section is generated with a placeholder secret key

{: .warning }
> Replace the `Jwt:Secret` value in `appsettings.json` with a strong secret key (minimum 32 characters) before deploying to production. Never commit real secrets to source control.

{: .note }
> When the `auth` block is absent, all endpoints use `AllowAnonymous()` and no auth infrastructure is generated. This is fully backward-compatible.

### Rate Limiting

The optional `rateLimit` object enables ASP.NET Core's built-in sliding window rate limiter for the generated API.

```json
{
  "config": {
    "rateLimit": {
      "permitLimit": 100,
      "windowSeconds": 60,
      "segmentsPerWindow": 6
    }
  }
}
```

| Field | Type | Default | Description |
|---|---|---|---|
| `permitLimit` | `integer` | `100` | Maximum number of requests allowed within the time window. |
| `windowSeconds` | `integer` | `60` | Duration of the time window in seconds. |
| `segmentsPerWindow` | `integer` | `6` | Number of segments the window is divided into for smoother rate limiting. |

When rate limiting is enabled:
- Requests exceeding the limit receive a `429 Too Many Requests` response
- The rate limiter middleware is added after CORS and before authentication in the pipeline
- A named policy `"sliding"` is registered and applied globally

{: .note }
> When the `rateLimit` block is absent, no rate limiting infrastructure is generated. This is fully backward-compatible.

## Entities

The `entities` object is a dictionary where each key is an entity name (PascalCase) and each value describes its properties and optional relationships and seed data.

```json
{
  "entities": {
    "Product": {
      "properties": { ... },
      "relationships": { ... },
      "seedData": [ ... ]
    }
  }
}
```

### Properties

Each property is defined by a name (PascalCase) mapped to an object with these fields:

| Field | Type | Required | Description |
|---|---|---|---|
| `type` | `string` | Yes | The C# type of the property. |
| `isKey` | `boolean` | No | Marks this property as the primary key. Exactly one property per entity should be a key. |
| `required` | `boolean` | No | Generates a `NotEmpty()` validation rule. |
| `maxLength` | `integer` | No | Generates a `MaximumLength()` validation rule. |
| `minLength` | `integer` | No | Generates a `MinimumLength()` validation rule. |
| `min` | `integer` | No | Generates a `GreaterThanOrEqualTo()` validation rule. |
| `max` | `integer` | No | Generates a `LessThanOrEqualTo()` validation rule. |
| `pattern` | `string` | No | Generates a `Matches()` validation rule with the given regex. |

### Supported Types

| Type | Aliases | SQLite Mapping |
|---|---|---|
| `Guid` | -- | `CHAR(36)` |
| `string` | `String` | `TEXT` |
| `int` | `Int32` | `INTEGER` |
| `long` | `Int64` | `INTEGER` |
| `float` | `Single` | `REAL` |
| `double` | `Double` | `REAL` |
| `decimal` | `Decimal` | `REAL` |
| `bool` | `Boolean` | `INTEGER` |
| `DateTime` | -- | `TEXT` |
| `DateTimeOffset` | -- | `TEXT` |
| `DateOnly` | -- | `TEXT` |
| `TimeOnly` | -- | `TEXT` |
| `byte[]` | -- | `BLOB` |

{: .tip }
> Type names are case-insensitive. `string` and `String` are equivalent, as are `int` and `Int32`.

## Relationships

The optional `relationships` object on an entity defines associations with other entities.

```json
{
  "Author": {
    "properties": {
      "Id": { "type": "Guid", "isKey": true },
      "Name": { "type": "string" }
    },
    "relationships": {
      "Posts": { "relatedEntity": "Post", "type": "OneToMany" }
    }
  }
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `relatedEntity` | `string` | Yes | Name of the related entity (must exist in `entities`). |
| `type` | `string` | Yes | Relationship type: `OneToOne`, `OneToMany`, `ManyToMany`, `OneWay`, `ManyWay`. |

{: .note }
> `OneToMany` relationships generate a nested GET endpoint (e.g., `/authors/{authorId}/posts`). See [Generated Examples](/Ninjadog/examples#endpoint----nested-relationship) for sample output.

## Enums

The optional top-level `enums` object defines C# enum types. Each key is the enum name, and the value is an array of string member names.

```json
{
  "enums": {
    "Priority": ["Low", "Medium", "High", "Critical"],
    "Status": ["Draft", "Active", "Archived"]
  }
}
```

Each enum produces a separate `.cs` file under the `Domain` namespace. Enum-typed columns are mapped to `INTEGER` in the database.

{: .tip }
> You can use enum names as property types in your entities. For example, `"type": "Priority"` on a property will generate the correct C# type and an `INTEGER` database column.

## Seed Data

Entities can include an optional `seedData` array to populate tables with initial rows at startup.

```json
{
  "Category": {
    "properties": {
      "Id": { "type": "Guid", "isKey": true },
      "Name": { "type": "string" },
      "IsActive": { "type": "bool" }
    },
    "seedData": [
      { "Id": "550e8400-e29b-41d4-a716-446655440001", "Name": "Electronics", "IsActive": true },
      { "Id": "550e8400-e29b-41d4-a716-446655440002", "Name": "Books", "IsActive": true }
    ]
  }
}
```

Each object in the array must include the key property and any non-nullable properties. The generated `DatabaseSeeder` class runs INSERT statements at startup, immediately after `DatabaseInitializer.InitializeAsync()`.

{: .note }
> The `DatabaseSeeder` is only generated when at least one entity defines `seedData`. See [Seed Data Generator](/Ninjadog/generators/seed-data) for details on the generated code.

## Native AOT Support

When `features.aot` is set to `true`, Ninjadog generates code optimized for [Native AOT publishing](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/):

```json
{
  "config": {
    "features": {
      "aot": true
    }
  }
}
```

### What changes with AOT enabled

| Area | Standard | AOT |
|---|---|---|
| **App builder** | `WebApplication.CreateBuilder()` | `WebApplication.CreateSlimBuilder()` |
| **JSON serialization** | Reflection-based | Source-generated `AppJsonSerializerContext` |
| **FastEndpoints** | Default config | Configured with `SerializerContext` |
| **Swagger / Client Gen** | Included | Removed (incompatible with AOT) |
| **Dapper** | Standard | `[DapperAot]` attribute on repositories |
| **NuGet packages** | Standard set | Adds `Dapper.AOT` |

### Generated files

When AOT is enabled, an additional `AppJsonSerializerContext.cs` file is generated at the project root. It contains `[JsonSerializable]` attributes for every Request, Response, DTO, and Domain type, enabling System.Text.Json source generation.

{: .warning }
> Swagger UI and client code generation endpoints are not available when AOT is enabled, as they rely on runtime reflection. Use the standard (non-AOT) mode during development if you need these features.

{: .tip }
> To publish your AOT-enabled project, add `<PublishAot>true</PublishAot>` to your `.csproj` file and run `dotnet publish -c Release`.

## Complete Example

A full `ninjadog.json` demonstrating all features:

```json
{
  "$schema": "./ninjadog.schema.json",
  "config": {
    "name": "BookStore",
    "version": "1.0.0",
    "description": "Bookstore REST API",
    "rootNamespace": "BookStore.Api",
    "outputPath": "src/applications/BookStore",
    "saveGeneratedFiles": true,
    "cors": {
      "origins": ["https://bookstore.example.com"],
      "methods": ["GET", "POST", "PUT", "DELETE"],
      "headers": ["Content-Type", "Authorization"]
    },
    "features": {
      "softDelete": true,
      "auditing": true,
      "aot": false
    },
    "database": {
      "provider": "postgresql"
    },
    "auth": {
      "provider": "jwt",
      "issuer": "https://bookstore.example.com",
      "audience": "bookstore-api",
      "tokenExpirationMinutes": 120,
      "roles": ["Admin", "Editor"]
    },
    "rateLimit": {
      "permitLimit": 200,
      "windowSeconds": 60,
      "segmentsPerWindow": 6
    }
  },
  "enums": {
    "BookStatus": ["Draft", "Published", "OutOfPrint"],
    "Genre": ["Fiction", "NonFiction", "Science", "History"]
  },
  "entities": {
    "Author": {
      "properties": {
        "Id": { "type": "Guid", "isKey": true },
        "Name": { "type": "string", "required": true, "maxLength": 200, "minLength": 1 },
        "Email": { "type": "string", "pattern": "^[^@]+@[^@]+\\.[^@]+$" }
      },
      "relationships": {
        "Books": { "relatedEntity": "Book", "type": "OneToMany" }
      },
      "seedData": [
        { "Id": "a1b2c3d4-0000-0000-0000-000000000001", "Name": "Jane Austen", "Email": "jane@example.com" }
      ]
    },
    "Book": {
      "properties": {
        "Id": { "type": "Guid", "isKey": true },
        "Title": { "type": "string", "required": true, "maxLength": 500 },
        "Isbn": { "type": "string", "pattern": "^\\d{13}$" },
        "Price": { "type": "decimal", "min": 0, "max": 9999 },
        "Status": { "type": "BookStatus" },
        "Genre": { "type": "Genre" },
        "PublishedDate": { "type": "DateOnly" }
      },
      "seedData": [
        {
          "Id": "b1b2c3d4-0000-0000-0000-000000000001",
          "Title": "Pride and Prejudice",
          "Isbn": "9780141439518",
          "Price": 12.99,
          "Status": 1,
          "Genre": 0,
          "PublishedDate": "1813-01-28"
        }
      ]
    }
  }
}
```

---

## Next Steps

- [Getting Started](/Ninjadog/getting-started) -- Step-by-step tutorial using this configuration
- [CLI Reference](/Ninjadog/cli) -- Commands for initializing, validating, and building projects
- [Generators](/Ninjadog/generators) -- Deep dive into all 34 generated files
- [Generated Examples](/Ninjadog/examples) -- See real generated code output
- [Seed Data Generator](/Ninjadog/generators/seed-data) -- Details on the DatabaseSeeder
