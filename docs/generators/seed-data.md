---
title: Seed Data Generator
description: "Ninjadog seed data generator: generate a DatabaseSeeder class that populates tables with initial data at startup."
layout: default
parent: Generators
nav_order: 8
---

# Seed Data Generator
{: .no_toc }

Generates a `DatabaseSeeder` class that inserts initial rows into your database tables at application startup.
{: .fs-6 .fw-300 }

<details open markdown="block">
  <summary>Table of contents</summary>
  {: .text-delta }
1. TOC
{:toc}
</details>

---

## What It Generates

The seed data generator produces a single `DatabaseSeeder.cs` file containing INSERT statements for every entity that defines a `seedData` array in `ninjadog.json`. The seeder is registered in the DI container and called during application startup, immediately after the database schema is initialized.

{: .note }
> The `DatabaseSeeder` file is only generated when at least one entity includes a `seedData` array. If no entities define seed data, the file is not emitted.

## Configuration

Add a `seedData` array to any entity in your `ninjadog.json`. Each element is an object whose keys match the entity's property names.

```json
{
  "entities": {
    "Category": {
      "properties": {
        "Id": { "type": "Guid", "isKey": true },
        "Name": { "type": "string" },
        "SortOrder": { "type": "int" },
        "IsActive": { "type": "bool" }
      },
      "seedData": [
        { "Id": "550e8400-e29b-41d4-a716-446655440001", "Name": "Electronics", "SortOrder": 1, "IsActive": true },
        { "Id": "550e8400-e29b-41d4-a716-446655440002", "Name": "Books", "SortOrder": 2, "IsActive": true },
        { "Id": "550e8400-e29b-41d4-a716-446655440003", "Name": "Archive", "SortOrder": 99, "IsActive": false }
      ]
    }
  }
}
```

### Rules

- Each seed object must include the key property (e.g., `Id`).
- Boolean values are converted to `1` (true) or `0` (false) in the generated SQL.
- String values are escaped and wrapped in single quotes.
- Numeric values are inserted as literals.
- You can seed multiple entities -- the seeder handles all of them in a single file.

## Generated Output

Given the configuration above, Ninjadog generates:

```csharp
public partial class DatabaseSeeder(IDbConnectionFactory connectionFactory)
{
    public async Task SeedAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();

        await connection.ExecuteAsync("INSERT INTO Categories (Id, Name, SortOrder, IsActive) VALUES ('550e8400-e29b-41d4-a716-446655440001', 'Electronics', 1, 1)");

        await connection.ExecuteAsync("INSERT INTO Categories (Id, Name, SortOrder, IsActive) VALUES ('550e8400-e29b-41d4-a716-446655440002', 'Books', 2, 1)");

        await connection.ExecuteAsync("INSERT INTO Categories (Id, Name, SortOrder, IsActive) VALUES ('550e8400-e29b-41d4-a716-446655440003', 'Archive', 99, 0)");

    }
}
```

## Integration with DI and Startup

The `DatabaseSeeder` is automatically registered in the dependency injection container and invoked at startup. In the generated `Program.cs` and DI extensions:

1. **DI Registration** -- `DatabaseSeeder` is registered as a singleton service in `CrudWebApiExtensions.AddNinjadog()`.
2. **Startup Call** -- `DatabaseSeeder.SeedAsync()` is called in `CrudWebApiExtensions.UseNinjadog()`, right after `DatabaseInitializer.InitializeAsync()`.

```csharp
// In AddNinjadog()
services.AddSingleton<DatabaseSeeder>();

// In UseNinjadog()
var initializer = app.Services.GetRequiredService<DatabaseInitializer>();
await initializer.InitializeAsync();

var seeder = app.Services.GetRequiredService<DatabaseSeeder>();
await seeder.SeedAsync();
```

{: .tip }
> Because `DatabaseSeeder` is a `partial` class, you can extend it with custom seeding logic in a separate file without modifying generated code.

## Multiple Entities

When several entities define seed data, all INSERT statements are combined in a single `DatabaseSeeder` class:

```json
{
  "entities": {
    "Category": {
      "properties": { ... },
      "seedData": [ ... ]
    },
    "Tag": {
      "properties": {
        "Id": { "type": "Guid", "isKey": true },
        "Label": { "type": "string" }
      },
      "seedData": [
        { "Id": "660e8400-0000-0000-0000-000000000001", "Label": "Featured" },
        { "Id": "660e8400-0000-0000-0000-000000000002", "Label": "Sale" }
      ]
    }
  }
}
```

The generated seeder includes INSERT statements for both `Categories` and `Tags` tables.

---

## Next Steps

- [Configuration Reference](/Ninjadog/configuration#seed-data) -- Full seed data configuration options
- [Generated Examples](/Ninjadog/examples#database----seed-data) -- See the generated seeder in context
- [Data Layer Generators](/Ninjadog/generators/data-layer) -- Database initializer and repositories
- [Getting Started](/Ninjadog/getting-started) -- End-to-end tutorial for creating your first API
