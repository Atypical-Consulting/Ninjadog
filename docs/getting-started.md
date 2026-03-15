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

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later (required for Options 2 and 3 only — Homebrew installs a self-contained binary)

{: .tip }
> Verify your SDK version with `dotnet --version`. You need **10.0** or higher.

## Installation

### Option 1 -- Homebrew (macOS / Linux)

```bash
brew tap atypical-consulting/tap
brew install ninjadog
```

{: .tip }
> This installs a self-contained binary — no .NET SDK required on the machine.

### Option 2 -- .NET tool

Install the Ninjadog CLI as a global .NET tool (requires .NET 10 SDK):

```bash
dotnet tool install -g Ninjadog
```

### Option 3 -- From Source

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

The CLI walks you through a series of interactive prompts (project name, version, description, root namespace, and output path). Press <kbd>Enter</kbd> at each prompt to accept the defaults, or type your own values.

Once complete, a `ninjadog.json` configuration file is created with a sample `Person` entity (Id, FirstName, LastName, BirthDate) to get you started.

{: .tip }
> See the [CLI Reference -- `ninjadog init`](/Ninjadog/cli#ninjadog-init) for the full list of prompts and their default values.

### Step 2 -- Define your domain entities

Open `ninjadog.json` and edit it to define the entities you need. For example, replace the default content with a `Product` entity:

```json
{
  "$schema": "./ninjadog.schema.json",
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

{: .tip }
> You can also use `ninjadog ui` to visually build your configuration in a web browser.

### Step 3 -- Validate your config

Before generating code, check your configuration for errors:

```bash
ninjadog validate
```

If there are issues, the validator reports them with error codes and descriptions. Fix any errors before proceeding.

### Step 4 -- Generate the code

```bash
ninjadog build
```

{: .note }
> Ninjadog generates ~33 files to disk including endpoints, DTOs, validators, repositories, services, mappers, domain entities, a database initializer, and a complete project structure (`.sln`, `.csproj`, `Program.cs`, `appsettings.json`). All files are written to the `outputPath` directory specified in your configuration.

### Step 5 -- Run the API

Navigate to the generated project and start it:

```bash
cd src/applications/MyApi
dotnet run
```

### Step 6 -- Verify it works

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
  "$schema": "./ninjadog.schema.json",
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

## Troubleshooting

### .NET SDK version mismatch

If you see an error like `The framework 'Microsoft.NETCore.App', version '10.0.0' was not found`, your .NET SDK is too old.

```bash
dotnet --version
```

{: .warning }
> Ninjadog requires **.NET 10 SDK** or later. Download it from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/10.0). If you have multiple SDKs installed, check that a `global.json` in a parent directory is not pinning an older version.

### Missing `ninjadog.json`

If `ninjadog build` reports that it cannot find a configuration file, make sure you are running the command from the directory that contains `ninjadog.json`.

```bash
# Verify the file exists
ls ninjadog.json

# If missing, initialize a new project
ninjadog init
```

{: .tip }
> You can also run `ninjadog validate` to check whether the file is present and well-formed. See the [CLI Reference](/Ninjadog/cli#ninjadog-validate) for details.

### Port already in use

If `dotnet run` fails with `Address already in use` or `Failed to bind to address`, another process is occupying the default port.

```bash
# Find what is using port 5000
lsof -i :5000

# Option 1 -- stop the other process
kill <PID>

# Option 2 -- run on a different port
dotnet run --urls "http://localhost:5050"
```

### Generated project does not compile

If the generated code fails to build, check the following:

1. **Outdated generation** -- Re-run `ninjadog build` after every change to `ninjadog.json`.
2. **Invalid property types** -- Make sure all `type` values are [supported types](/Ninjadog/configuration#supported-types). Run `ninjadog validate` to catch type errors.
3. **Duplicate entity names** -- Entity names must be unique and in PascalCase.

{: .note }
> If you modified a generated file and then re-ran `ninjadog build`, your changes will be overwritten. Use `partial` classes in separate files to add custom logic safely.

### `ninjadog` command not found

If your shell cannot find the `ninjadog` command after installation:

```bash
# Verify the tool is installed
dotnet tool list -g

# If missing, install it
dotnet tool install -g Ninjadog

# If installed but not on PATH, add the .NET tools directory
export PATH="$PATH:$HOME/.dotnet/tools"
```

{: .tip }
> On macOS and Linux, you may need to add `$HOME/.dotnet/tools` to your shell profile (`~/.bashrc`, `~/.zshrc`, etc.) for the path to persist across sessions.

---

## Next Steps

- [Configuration Reference](/Ninjadog/configuration) -- Full reference for ninjadog.json
- [Architecture](/Ninjadog/architecture) -- Understand the design decisions and tech stack
- [CLI Reference](/Ninjadog/cli) -- Scaffold projects with the CLI tool
- [Generators](/Ninjadog/generators) -- Deep dive into all 34 generators
- [Generated Examples](/Ninjadog/examples) -- See real generated code output
