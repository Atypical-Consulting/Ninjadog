---
title: Integration Test Generators
description: "Ninjadog integration test generators: WebApplicationFactory setup, test base class, and per-entity CRUD endpoint tests using xUnit and FluentAssertions."
layout: default
parent: Generators
nav_order: 9
---

# Integration Test Generators

Ninjadog generates a complete integration test scaffold for your Web API, using `WebApplicationFactory` to spin up a real test server with an in-memory SQLite database. Each entity gets a dedicated test class exercising all CRUD endpoints.

## Generated Files

| Generator | Scope | Output |
|---|---|---|
| IntegrationTestCsproj | Single file | `{ProjectName}.IntegrationTests.csproj` |
| CustomWebApplicationFactory | Single file | `CustomWebApplicationFactory.cs` |
| IntegrationTestBase | Single file | `IntegrationTestBase.cs` |
| EntityIntegrationTest | Per entity | `{Entity}EndpointTests.cs` |

## Test Project (`IntegrationTestCsproj`)

Generates an xUnit test project referencing the main API project with the following packages:

- `Microsoft.AspNetCore.Mvc.Testing` -- WebApplicationFactory support
- `Microsoft.NET.Test.Sdk` -- test runner infrastructure
- `xunit` -- test framework
- `xunit.runner.visualstudio` -- VS/IDE integration
- `FluentAssertions` -- readable assertions

## WebApplicationFactory (`CustomWebApplicationFactory`)

Creates a `CustomWebApplicationFactory` that:

- Sets the environment to `"Testing"`
- Replaces the `IDbConnectionFactory` with an in-memory SQLite connection
- Ensures tests run against a clean, isolated database per test run

## Test Base Class (`IntegrationTestBase`)

Provides shared infrastructure for all test classes:

- Implements `IClassFixture<CustomWebApplicationFactory>` for factory reuse
- Provides a pre-configured `HttpClient`
- Includes `DeserializeAsync<T>()` and `ToJsonContent<T>()` JSON helpers
- Implements `IDisposable` for proper cleanup

## Per-Entity Tests (`EntityIntegrationTest`)

Each entity gets **8 test methods** covering the full CRUD lifecycle:

| Test | HTTP Method | Expected Status |
|---|---|---|
| `Create{Entity}_WithValidRequest_ReturnsCreated` | POST | 201 Created |
| `GetAll{Entities}_AfterCreating_ReturnsCollection` | GET | 200 OK |
| `Get{Entity}_WithValidId_ReturnsEntity` | GET | 200 OK |
| `Get{Entity}_WithInvalidId_ReturnsNotFound` | GET | 404 Not Found |
| `Update{Entity}_WithValidRequest_ReturnsOk` | PUT | 200 OK |
| `Delete{Entity}_WithValidId_ReturnsNoContent` | DELETE | 204 No Content |
| `Delete{Entity}_WithInvalidId_ReturnsNotFound` | DELETE | 404 Not Found |
| `FullCrudLifecycle_{Entity}_WorksEndToEnd` | All | Full lifecycle |

Test data is auto-generated based on entity property types:

- `string` properties get `"Test {PropertyName}"` values
- `bool` properties default to `false` (create) / `true` (update)
- `int` properties use `1` (create) / `42` (update)
- `decimal` properties use `19.99m` (create) / `99.99m` (update)
- `DateTime` properties use `UtcNow + 1 day` (create) / `UtcNow + 7 days` (update)
- `Guid` properties use `Guid.NewGuid()`

## Example Output

For a `TodoItem` entity with properties `Title (string)`, `IsCompleted (bool)`, and `DueDate (DateTime)`:

```csharp
public class TodoItemEndpointTests : IntegrationTestBase
{
    private const string BaseUrl = "/todo-items";

    public TodoItemEndpointTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task CreateTodoItem_WithValidRequest_ReturnsCreated()
    {
        var request = new CreateTodoItemRequest
        {
            Title = "Test Title",
            IsCompleted = false,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        var response = await Client.PostAsJsonAsync(BaseUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await DeserializeAsync<TodoItemResponse>(response);
        created.Should().NotBeNull();
        created!.Title.Should().Be("Test Title");
    }

    // ... 7 more tests
}
```
