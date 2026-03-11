---
title: Generated Examples
description: "Real generated C# code from Ninjadog: FastEndpoints with pagination, route constraints, FluentValidation validators, and SQLite schema generation."
layout: default
nav_order: 6
---

# Generated Output Examples
{: .no_toc }

Real generated code from Ninjadog's verified snapshot tests -- this is exactly what the CLI produces.
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

## Database -- SQLite Schema with Type-Aware Columns

The `DatabaseInitializer` creates SQLite tables for **all** entities in your project. Column types are mapped from C# types automatically (see [Data Layer](/Ninjadog/generators/data-layer) for the full mapping table).

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

---

## Next Steps

- [Generators](/Ninjadog/generators) -- Explore all 30 generators in detail
- [Architecture](/Ninjadog/architecture) -- Understand the full pipeline
- [Getting Started](/Ninjadog/getting-started) -- Try it yourself
