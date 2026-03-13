---
title: Use Cases
description: "Built-in use cases that demonstrate Ninjadog's code generation capabilities with real-world domain models like TodoApp and RestaurantBooking."
layout: default
nav_order: 7
---

# Use Cases
{: .no_toc }

Ninjadog ships with built-in use cases -- complete domain models that serve as reference implementations and are validated by the test suite.
{: .fs-6 .fw-300 }

<details open markdown="block">
  <summary>Table of contents</summary>
  {: .text-delta }
1. TOC
{:toc}
</details>

---

## Overview

Use cases are predefined domain models defined as `ninjadog.json` files. They live in the `Ninjadog.Templates.CrudWebApi` project under `UseCases/` and serve three purposes:

1. **Reference implementations** -- Show how to structure entity definitions, relationships, and configuration for real-world domains.
2. **Test fixtures** -- The full template pipeline runs against each use case in the test suite, catching regressions across all generators.
3. **Learning examples** -- Each use case demonstrates the JSON configuration format with a complete, working domain model.

---

## Architecture

Each use case is simply a `ninjadog.json` file that is embedded as a resource and loaded at runtime via `UseCaseSettings`:

```
UseCases/
└── {Name}/
    └── ninjadog.json    -- the complete domain model
```

The `UseCaseSettings` class provides static methods to load each use case:

```csharp
var todoApp = UseCaseSettings.TodoApp();
var restaurant = UseCaseSettings.RestaurantBooking();
```

These methods parse the embedded JSON using the same `NinjadogSettings.FromJsonString()` path that the CLI uses, ensuring the use cases exercise the real parsing pipeline.

---

## TodoApp

A simple task management application with three entities and two relationships.

### Entities

| Entity | Key | Properties | Relationships |
|---|---|---|---|
| **TodoList** | `Id` (Guid) | Title | Items → TodoItem (OneToMany), Categories → TodoCategory (OneToMany) |
| **TodoItem** | `Id` (Guid) | Description, IsCompleted, DueDate | -- |
| **TodoCategory** | `Id` (Guid) | Name, Color, Icon | -- |

### JSON Configuration

```json
{
  "config": {
    "name": "TodoApp",
    "version": "1.0.0",
    "description": "An application to manage todo lists.",
    "rootNamespace": "TodoApp.CrudWebApi"
  },
  "entities": {
    "TodoList": {
      "properties": {
        "Id": { "type": "Guid", "isKey": true },
        "Title": { "type": "string" }
      },
      "relationships": {
        "Items": {
          "relatedEntity": "TodoItem",
          "relationshipType": "OneToMany"
        },
        "Categories": {
          "relatedEntity": "TodoCategory",
          "relationshipType": "OneToMany"
        }
      }
    },
    "TodoItem": {
      "properties": {
        "Id": { "type": "Guid", "isKey": true },
        "Description": { "type": "string" },
        "IsCompleted": { "type": "bool" },
        "DueDate": { "type": "DateTime" }
      }
    },
    "TodoCategory": {
      "properties": {
        "Id": { "type": "Guid", "isKey": true },
        "Name": { "type": "string" },
        "Color": { "type": "string" },
        "Icon": { "type": "string" }
      }
    }
  }
}
```

### What Gets Generated

Running all templates against TodoApp produces:

- **3 domain entities** -- `TodoList.cs`, `TodoItem.cs`, `TodoCategory.cs`
- **3 repositories + interfaces** -- CRUD SQL operations for each entity
- **3 services + interfaces** -- Business logic layer for each entity
- **15 endpoints** -- Create, GetAll, Get, Update, Delete for each entity
- **2 nested endpoints** -- `GET /todo-lists/{id}/todo-items`, `GET /todo-lists/{id}/todo-categories`
- **12 contracts** -- DTOs, request/response models for each entity
- **12 mappers** -- Domain ↔ DTO ↔ API contract conversions
- **6 validators** -- FluentValidation rules for Create/Update requests
- **1 database initializer** -- CREATE TABLE statements for all entities
- **1 Program.cs** -- Application entry point with DI wiring
- **Docker files** -- Dockerfile, docker-compose, .dockerignore

{: .tip }
> The relationship from `TodoList` to `TodoItem` and `TodoCategory` automatically generates nested `GetByParent` endpoints, allowing clients to fetch a list's items or categories via `GET /todo-lists/{todoListId}/todo-items`.

---

## RestaurantBooking

A comprehensive restaurant management system with 12 entities, demonstrating how Ninjadog handles larger domain models.

### Entities

| Entity | Key | Relationships |
|---|---|---|
| **Customer** | CustomerId (Guid) | Bookings → Booking (OneToMany) |
| **Booking** | BookingId (Guid) | -- |
| **Table** | TableNumber (Int32) | -- |
| **Order** | OrderId (Guid) | -- |
| **OrderMenuItem** | OrderMenuItemId (Guid) | -- |
| **Menu** | MenuId (Guid) | -- |
| **MenuItem** | MenuItemId (Guid) | -- |
| **MenuItemIngredient** | MenuItemIngredientId (Guid) | -- |
| **Ingredient** | IngredientId (Guid) | -- |
| **IngredientType** | IngredientTypeCode (Guid) | -- |
| **Staff** | StaffId (Guid) | -- |
| **StaffRole** | StaffRoleCode (Guid) | -- |

{: .note }
> The `Table` entity uses an `Int32` primary key (`TableNumber`) instead of a Guid, demonstrating that Ninjadog supports different key types with automatic route constraint resolution (`{tableNumber:int}` vs `{id:guid}`).

### Scale

RestaurantBooking generates **over 100 files** from a single configuration, including:
- 12 domain entities, repositories, services, and their interfaces
- 60 CRUD endpoints + 1 nested endpoint (bookings by customer)
- 48 contract models (DTOs, requests, responses)
- 48 mappers
- 24 validators
- Database initializer with 12 CREATE TABLE statements

---

## Next Steps

- [Getting Started](/Ninjadog/getting-started) -- Create your first API with `ninjadog init` and `ninjadog build`
- [Configuration Reference](/Ninjadog/configuration) -- Full `ninjadog.json` schema documentation
- [Generators](/Ninjadog/generators/) -- Detailed documentation for each code generator
- [Architecture](/Ninjadog/architecture) -- How the template pipeline works
