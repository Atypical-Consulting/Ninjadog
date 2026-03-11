---
title: Getting Started
description: "Step-by-step tutorial to install Ninjadog, create a JSON config, and generate a full CRUD REST API with .NET in under 2 minutes."
layout: default
nav_order: 2
---

# Getting Started
{: .no_toc }

Get a full CRUD REST API running in under 2 minutes.
{: .fs-6 .fw-300 }

<details open markdown="block">
  <summary>Table of contents</summary>
  {: .text-delta }
1. TOC
{:toc}
</details>

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later

{: .tip }
> Verify your SDK version with `dotnet --version`. You need **10.0** or higher.

## Installation

### Option 1 -- CLI tool (recommended)

Install the Ninjadog CLI as a global .NET tool:

```bash
dotnet tool install -g Ninjadog
```

### Option 2 -- From Source

```bash
git clone https://github.com/Atypical-Consulting/Ninjadog.git
cd ninjadog
dotnet build
```

## Your First API

Follow these steps to create a fully functional REST API from a simple JSON configuration.

### Step 1 -- Initialize a new project

```bash
mkdir MyApi && cd MyApi
ninjadog init
```

This creates a `ninjadog.json` configuration file with a default `Person` entity (Id, FirstName, LastName, BirthDate) to get you started.

### Step 2 -- Define your domain entities

Open `ninjadog.json` and edit it to define the entities you need. For example, replace the default content with a `Product` entity:

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

### Step 3 -- Generate the code

```bash
ninjadog build
```

{: .note }
> Ninjadog generates ~33 files to disk including endpoints, DTOs, validators, repositories, services, mappers, domain entities, a database initializer, and a complete project structure (`.sln`, `.csproj`, `Program.cs`, `appsettings.json`). All files are written to the `outputPath` directory specified in your configuration.

### Step 4 -- Run the API

Navigate to the generated project and start it:

```bash
cd src/applications/MyApi
dotnet run
```

### Step 5 -- Verify it works

Open your browser or use `curl` to test the endpoints:

```bash
# Create a product
curl -X POST http://localhost:5000/products \
  -H "Content-Type: application/json" \
  -d '{"name": "Widget", "price": 9.99}'

# List all products (paginated)
curl http://localhost:5000/products

# Get a single product
curl http://localhost:5000/products/{id}

# Update a product
curl -X PUT http://localhost:5000/products/{id} \
  -H "Content-Type: application/json" \
  -d '{"name": "Updated Widget", "price": 12.99}'

# Delete a product
curl -X DELETE http://localhost:5000/products/{id}
```

You should see JSON responses for each operation. The GetAll endpoint returns paginated results with `totalCount` metadata.

## Multiple Entities

Each entity gets its own isolated set of generated files. Add as many entities as you need in `ninjadog.json`:

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
    },
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

Then regenerate with `ninjadog build`. Each entity produces its own full set of CRUD files.

{: .tip }
> Route constraints are dynamic -- `:guid`, `:int`, or untyped -- based on your entity's key type. An `int` key produces `/orders/{id:int}`, while a `Guid` key produces `/movies/{id:guid}`.

## CORS Configuration

By default, the generated API allows requests from `https://localhost:7270`. You can customize the CORS policy by adding a `cors` block inside `config` in your `ninjadog.json`:

```json
{
  "config": {
    "name": "MyApi",
    "version": "1.0.0",
    "description": "My API",
    "rootNamespace": "MyApi",
    "outputPath": "src/applications/MyApi",
    "saveGeneratedFiles": true,
    "cors": {
      "origins": ["https://myapp.com", "https://staging.myapp.com"],
      "methods": ["GET", "POST", "PUT", "DELETE"],
      "headers": ["Content-Type", "Authorization"]
    }
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

### CORS Options

| Option | Type | Required | Description |
|---|---|---|---|
| `origins` | `string[]` | Yes | The allowed origins for cross-origin requests. |
| `methods` | `string[]` | No | The allowed HTTP methods. When omitted, all methods are allowed. |
| `headers` | `string[]` | No | The allowed request headers. When omitted, all headers are allowed. |

{: .tip }
> If you omit the `cors` block entirely, Ninjadog defaults to allowing `https://localhost:7270` as the only origin -- useful for local development without extra configuration.

### Generated CORS Policy

When you provide a `cors` configuration, the generated `Program.cs` includes a named CORS policy that is registered and applied automatically:

```csharp
services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("https://myapp.com", "https://staging.myapp.com")
                .WithMethods("GET", "POST", "PUT", "DELETE")
                .WithHeaders("Content-Type", "Authorization");
        });
});

// ...

app.UseCors(myAllowSpecificOrigins);
```

## What Happens Under the Hood

When you run `ninjadog build`, the CLI reads your `ninjadog.json` configuration and generates a complete .NET web API project to disk:

| What | Example for `Product` |
|---|---|
| **Project** | `.sln`, `.csproj`, `Program.cs` with `AddNinjadog()` and `UseNinjadog()` |
| **Endpoints** | `CreateProductEndpoint`, `GetAllProductsEndpoint`, etc. (FastEndpoints) |
| **Contracts** | `CreateProductRequest`, `ProductResponse`, `ProductDto` |
| **Validation** | `CreateProductRequestValidator` with FluentValidation rules |
| **Data Layer** | `IProductRepository`, `ProductRepository` (Dapper + SQLite), `IProductService`, `ProductService` |
| **Mapping** | Extension methods like `.ToProductResponse()`, `.ToProductDto()` |
| **Database** | `DatabaseInitializer` with SQLite `CREATE TABLE` for all entities |

{: .note }
> The generated code uses primary constructor injection, FastEndpoints for routing, Dapper with SQLite for data access, and FluentValidation for request validation.

---

## Next Steps

- [Architecture](/Ninjadog/architecture) -- Understand the design decisions and tech stack
- [CLI Reference](/Ninjadog/cli) -- Scaffold projects with the CLI tool
- [Generators](/Ninjadog/generators) -- Deep dive into all 30 generators
- [Generated Examples](/Ninjadog/examples) -- See real generated code output
