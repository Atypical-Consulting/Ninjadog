---
title: CLI Reference
description: "Ninjadog CLI reference: install the .NET global tool, initialize projects with ninjadog init, and generate APIs with ninjadog build."
layout: default
nav_order: 4
---

# CLI Reference
{: .no_toc }

The `ninjadog` CLI is distributed as a .NET global tool for project scaffolding and code generation.
{: .fs-6 .fw-300 }

<details open markdown="block">
  <summary>Table of contents</summary>
  {: .text-delta }
1. TOC
{:toc}
</details>

---

## Installation

```bash
dotnet tool install -g Ninjadog
```

{: .tip }
> After installation, run `ninjadog --help` to verify the tool is available.

## Typical Workflow

```
ninjadog init              Create a new project
       ↓
  edit config              Define your entities in ninjadog.json
       ↓
ninjadog add-entity        (Optional) Add more entities from the CLI
       ↓
ninjadog build             Run the generators
       ↓
  dotnet run               Launch your API
```

## Commands

### `ninjadog init`

Initializes a new Ninjadog project in the current directory by generating a `ninjadog.json` configuration file. The command walks you through a series of interactive prompts to customize the project settings, then writes the configuration with a sample `Person` entity to get you started.

```bash
ninjadog init
```

#### Interactive prompts

When you run `ninjadog init`, the CLI asks for the following project settings:

| Prompt | Default | Description |
|---|---|---|
| **Project name** | `NinjadogApp` | The display name for your API project. |
| **Version** | `1.0.0` | The initial semantic version. |
| **Description** | `Welcome to Ninjadog!` | A short description of the project. |
| **Root namespace** | `NinjadogApp` | The C# root namespace for generated code. |
| **Output path** | `.` | Directory where generated files are written (relative to the config file). |

Press <kbd>Enter</kbd> at any prompt to accept the default value.

#### Example session

```
$ ninjadog init
? Project name: MyApi
? Version: 1.0.0
? Description: My REST API
? Root namespace: MyApi
? Output path: src/applications/MyApi
Ninjadog settings file created successfully.
```

This creates a `ninjadog.json` in the current directory:

```json
{
  "config": {
    "name": "MyApi",
    "version": "1.0.0",
    "description": "My REST API",
    "rootNamespace": "MyApi",
    "outputPath": "src/applications/MyApi",
    "saveGeneratedFiles": true
  },
  "entities": {
    "Person": {
      "properties": {
        "Id": { "type": "Guid", "isKey": true },
        "FirstName": { "type": "string" },
        "LastName": { "type": "string" },
        "BirthDate": { "type": "DateTime" }
      }
    }
  }
}
```

{: .tip }
> The generated `Person` entity is a starter template. Replace or extend it with your own domain entities before running `ninjadog build`.

{: .note }
> A `ninjadog.json` file must **not** already exist in the current directory. If one is found, the command exits with an error to avoid overwriting your configuration.

### `ninjadog add-entity`

Adds a new entity to your existing `ninjadog.json` configuration file. The entity is created with a default `Guid` primary key.

```bash
ninjadog add-entity <EntityName>
```

{: .note }
> The entity name should be in **PascalCase** (e.g., `Product`, `OrderItem`). A `ninjadog.json` file must already exist in the current directory — run `ninjadog init` first if you haven't already.

**Example:**

```bash
ninjadog add-entity Product
```

This appends a `Product` entity to the `entities` section of your `ninjadog.json`:

```json
{
  "entities": {
    "Person": { ... },
    "Product": {
      "properties": {
        "Id": {
          "type": "Guid",
          "isKey": true
        }
      }
    }
  }
}
```

{: .tip }
> After adding an entity, open `ninjadog.json` to define additional properties before running `ninjadog build`.

### `ninjadog build`

Builds and runs the generator engine against your project configuration. This reads the `ninjadog.json` file in the current directory and produces the generated source files.

```bash
ninjadog build
```

---

## Next Steps

- [Getting Started](/Ninjadog/getting-started) -- Step-by-step tutorial using the CLI tool
- [Architecture](/Ninjadog/architecture) -- Understand the project structure
- [Generators](/Ninjadog/generators) -- See what gets generated
