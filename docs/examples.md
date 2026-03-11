---
title: Generated Examples
description: "Real generated C# code from Ninjadog: FastEndpoints with pagination, route constraints, FluentValidation validators, and multi-provider database schema generation."
layout: default
nav_order: 7
---

# Generated Output Examples
{: .no_toc }

Real generated code from Ninjadog verified snapshot tests -- this is exactly what the CLI produces.
{: .fs-6 .fw-300 }

<details open markdown="block">
  <summary>Table of contents</summary>
  {: .text-delta }
1. TOC
{:toc}
</details>

---

## Source Entity

All examples below are generated from this `TodoItem` entity defined in `ninjadog.json`:

```json
{
  "TodoItem": {
    "properties": {
      "Id": { "type": "Guid", "isKey": true },
      "Title": { "type": "string" },
      "Description": { "type": "string" },
      "IsCompleted": { "type": "bool" },
      "DueDate": { "type": "DateTime" },
      "Priority": { "type": "int" },
      "Cost": { "type": "decimal" }
    }
  }
}
```

{: .note }
> Notice the mix of reference types (`string`, `DateTime`) and value types (`bool`, `int`, `decimal`). Ninjadog handles each differently for validation and database mapping.

---

## Endpoint -- GetAll with Pagination

The GetAll endpoint provides built-in pagination via query parameters. Default values are `page=1` and `pageSize=10`.

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

{: .tip }
> The endpoint uses **FastEndpoints** conventions -- `Configure()` sets the route and auth, `HandleAsync()` contains the logic. Dependencies like `ITodoItemService` are resolved via primary constructor injection.

## Endpoint -- GetOne with Route Constraint

The route constraint `{id:guid}` is generated automatically because the `Id` property is a `Guid`. For an `int` key, the constraint would be `{id:int}`.

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

## Endpoint -- Nested Relationship

When an entity defines a `OneToMany` relationship, Ninjadog generates a nested GET endpoint that returns child resources scoped to a parent. This example is generated from an `Author` entity with a `Posts` relationship:

```json
{
  "Author": {
    "properties": {
      "Id": { "type": "Guid", "isKey": true },
      "Name": { "type": "String" }
    },
    "relationships": {
      "Posts": { "relatedEntity": "Post", "type": "OneToMany" }
    }
  }
}
```

```csharp
public partial class GetPostsByAuthorEndpoint(IPostService postService)
    : EndpointWithoutRequest<GetAllPostsResponse>
{
    public override void Configure()
    {
        Get("/authors/{authorId:guid}/posts");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var authorId = Route<string>("authorId");
        var page = int.TryParse(HttpContext.Request.Query["page"], out var p) && p > 0 ? p : 1;
        var pageSize = int.TryParse(HttpContext.Request.Query["pageSize"], out var ps) && ps > 0 ? ps : 10;

        var (posts, totalCount) = await postService.GetAllAsync(page, pageSize);
        var postsResponse = posts.ToPostsResponse(page, pageSize, totalCount);
        await SendOkAsync(postsResponse, ct);
    }
}
```

{: .tip }
> The class name `GetPostsByAuthorEndpoint` is derived automatically from the relationship -- combining the child entity name (`Posts`) with the parent entity name (`Author`). Pagination works the same way as the standard GetAll endpoint.

## Validator -- Type-Aware Rules

Ninjadog generates FluentValidation validators that only validate **reference types** (`string`, `DateTime`). Value types like `bool`, `int`, and `decimal` always have default values in C#, so they are skipped.

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

    }
}
```

## Validator -- Validation Attributes

When you add validation attributes (`required`, `maxLength`, `minLength`, `min`, `max`, `pattern`) to your property definitions, Ninjadog generates the corresponding FluentValidation rules automatically.

Given this entity configuration:

```json
{
  "Contact": {
    "properties": {
      "Id": { "type": "Guid", "isKey": true },
      "Name": { "type": "String", "required": true, "maxLength": 100, "minLength": 2 },
      "Email": { "type": "String", "required": true, "pattern": "^[^@]+@[^@]+\\.[^@]+$" },
      "Age": { "type": "Int32", "min": 0, "max": 150 }
    }
  }
}
```

The generated validator includes all declared constraints:

```csharp
public partial class CreateContactRequestValidator : Validator<CreateContactRequest>
{
    public CreateContactRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required!")
            .MinimumLength(2)
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required!")
            .Matches("^[^@]+@[^@]+\\.[^@]+$");

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(150);

    }
}
```

{: .note }
> Validation attributes compose with type-aware rules. Value types like `Int32` are only validated when explicit constraints (`min`, `max`) are declared -- otherwise they are skipped entirely.

## Database -- Schema with Type-Aware Columns


The `DatabaseInitializer` creates tables for **all** entities in your project. Column types are mapped from C# types automatically (see [Data Layer](/Ninjadog/generators/data-layer) for the full mapping table).

{: .note }
> Ninjadog supports **multiple database providers**: SQLite (default), PostgreSQL, and SQL Server. Set `config.database.provider` in your `ninjadog.json` to switch providers -- all generated SQL, type mappings, and connection factories adapt automatically. See [Database Provider Configuration](/Ninjadog/generators/data-layer#database-provider-configuration) for details.

The example below shows SQLite output (the default provider):

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

## Database -- Soft Delete

When `config.features.softDelete` is enabled in `ninjadog.json`, the generated schema adds `IsDeleted` and `DeletedAt` columns to every table, and all queries are adjusted automatically.

**Generated schema with soft delete:**

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
            Cost REAL NOT NULL,
            IsDeleted INTEGER NOT NULL DEFAULT 0,
            DeletedAt TEXT)");


    }
}
```

**Generated soft-delete SQL (UPDATE instead of DELETE):**

```sql
UPDATE TodoItems SET IsDeleted = 1, DeletedAt = datetime('now') WHERE Id = @Id
```

**Generated filtered SELECT:**

```sql
SELECT * FROM TodoItems WHERE IsDeleted = 0 ORDER BY Id LIMIT @PageSize OFFSET @Offset
```

{: .note }
> Soft delete is disabled by default. Enable it by adding `"features": { "softDelete": true }` to the `config` section of your `ninjadog.json`. See [Data Layer Generators](/Ninjadog/generators/data-layer#soft-delete) for full details.

## Database -- Audit Fields

When `config.features.auditing` is set to `true`, the generated schema gains `CreatedAt` and `UpdatedAt` columns, and the repository SQL manages their values automatically.

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
            Cost REAL NOT NULL,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT)");

    }
}
```

The repository INSERT sets both timestamps, while UPDATE only refreshes `UpdatedAt`:

```csharp
// INSERT
await connection.ExecuteAsync(
    @"INSERT INTO TodoItems (Id, Title, Description, IsCompleted, DueDate, Priority, Cost, CreatedAt, UpdatedAt) VALUES (@Id, @Title, @Description, @IsCompleted, @DueDate, @Priority, @Cost, datetime('now'), datetime('now'))",
    todoItem);

// UPDATE
await connection.ExecuteAsync(
    @"UPDATE TodoItems SET Title = @Title, Description = @Description, IsCompleted = @IsCompleted, DueDate = @DueDate, Priority = @Priority, Cost = @Cost, UpdatedAt = datetime('now') WHERE Id = @Id",
    todoItem);
```

{: .note }
> Audit fields are opt-in. Without `"auditing": true` in your config, the generated output is identical to the standard schema shown above.

## Database -- Seed Data

When entities include a `seedData` array in `ninjadog.json`, a `DatabaseSeeder` class is generated to populate tables with initial rows at startup. It is called right after `DatabaseInitializer.InitializeAsync()`.

Given this configuration:

```json
{
  "Category": {
    "properties": {
      "Id": { "type": "Guid", "isKey": true },
      "Name": { "type": "String" },
      "IsActive": { "type": "Boolean" }
    },
    "seedData": [
      { "Id": "550e8400-...", "Name": "Default Category", "IsActive": true },
      { "Id": "550e8400-...", "Name": "Archive", "IsActive": false }
    ]
  }
}
```

Ninjadog produces:

```csharp
public partial class DatabaseSeeder(IDbConnectionFactory connectionFactory)
{
    public async Task SeedAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();

        await connection.ExecuteAsync("INSERT INTO Categories (Id, Name, IsActive) VALUES ('550e8400-...', 'Default Category', 1)");

        await connection.ExecuteAsync("INSERT INTO Categories (Id, Name, IsActive) VALUES ('550e8400-...', 'Archive', 0)");

    }
}
```

{: .note }
> The seeder file is only generated when at least one entity defines `seedData`. If no entities have seed data, no file is emitted.

---

## Enum -- C# Code Generation

Ninjadog can generate C# enum types from your `ninjadog.json` configuration. Define enums as named arrays of string values:

```json
{
  "enums": {
    "Priority": ["Low", "Medium", "High", "Critical"],
    "Status": ["Draft", "Active", "Archived"]
  }
}
```

Ninjadog generates a separate `.cs` file for each enum under the `Domain` namespace:

```csharp
namespace TestApp.Api.Domain;

public enum Priority
{
    Low,
    Medium,
    High,
    Critical
}
```

```csharp
namespace TestApp.Api.Domain;

public enum Status
{
    Draft,
    Active,
    Archived
}
```

{: .note }
> Enum-typed columns are mapped to `INTEGER` in the SQLite database. The DatabaseInitializer automatically uses `INTEGER` for any property whose type matches a defined enum name.

---

## Next Steps

- [Configuration Reference](/Ninjadog/configuration) -- Full reference for ninjadog.json options used in these examples
- [Generators](/Ninjadog/generators) -- Explore all 34 generators in detail
- [Seed Data Generator](/Ninjadog/generators/seed-data) -- Deep dive into the DatabaseSeeder
- [Architecture](/Ninjadog/architecture) -- Understand the full pipeline
- [Getting Started](/Ninjadog/getting-started) -- Try it yourself
