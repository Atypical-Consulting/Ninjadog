---
title: CLI Reference
description: "Ninjadog CLI reference: install the .NET global tool, initialize projects with ninjadog init, validate configs, launch the web UI, and generate APIs with ninjadog build."
layout: default
nav_order: 5
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
ninjadog init              Create a new project (interactive)
       |
  edit config              Define your entities in ninjadog.json
       |
ninjadog validate          Check your config for errors
       |
ninjadog add-entity        (Optional) Add more entities from the CLI
       |
ninjadog build             Run the generators
       |
  dotnet run               Launch your API
```

## Commands

### `ninjadog init`

Initializes a new Ninjadog project in the current directory. By default, `init` runs in interactive mode, prompting you for project details, then writes the configuration with a sample `Person` entity to get you started.

```bash
ninjadog init
```

**Options:**

| Option | Description |
|---|---|
| `--default` | Skip prompts and use default values. |
| `--name <name>` | Set the project name (skips the name prompt). |
| `--namespace <ns>` | Set the root namespace (skips the namespace prompt). |

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

**Examples (non-interactive):**

```bash
# Non-interactive with defaults
ninjadog init --default

# Non-interactive with custom values
ninjadog init --name MyApi --namespace MyApi.Web
```

{: .tip }
> The generated `Person` entity is a starter template. Replace or extend it with your own domain entities before running `ninjadog build`.

{: .note }
> A `ninjadog.json` file must **not** already exist in the current directory. If one is found, the command exits with an error to avoid overwriting your configuration.

### `ninjadog validate`

Validates your `ninjadog.json` configuration file against the schema and checks for common errors.

```bash
ninjadog validate [OPTIONS]
```

**Options:**

| Option | Description |
|---|---|
| `--file <path>` | Path to the configuration file. Default: `ninjadog.json` |
| `--strict` | Treat warnings as errors (exit code 1). |

**Exit codes:**

| Code | Meaning |
|---|---|
| `0` | Configuration is valid. |
| `1` | Validation errors found. |
| `2` | File not found or JSON parse error. |

**Example output:**

```
$ ninjadog validate

Validating ninjadog.json...

  ERROR  NINJ001  Entity "Product" has no properties defined.
  ERROR  NINJ003  Property "Product.Id" is marked as key but type "string" has no maxLength.
  WARN   NINJ007  Entity "Order" has no key property. A Guid key named "Id" will be added.

Result: 2 errors, 1 warning
```

**Validation rules:**

| Code | Severity | Description |
|---|---|---|
| NINJ001 | Error | Entity has no properties defined. |
| NINJ002 | Error | Entity has multiple key properties. |
| NINJ003 | Error | String key property has no `maxLength`. |
| NINJ004 | Error | Property type is not recognized. |
| NINJ005 | Error | Relationship references a non-existent entity. |
| NINJ006 | Error | Enum name conflicts with an entity name. |
| NINJ007 | Warning | Entity has no key property (a default will be added). |
| NINJ008 | Warning | Seed data property does not match any entity property. |
| NINJ009 | Warning | `outputPath` directory does not exist. |
| NINJ010 | Warning | Config file does not include `$schema` property. |

{: .tip }
> Run `ninjadog validate` before `ninjadog build` to catch configuration mistakes early. In CI pipelines, use `--strict` to ensure warnings are not ignored.

### `ninjadog add-entity`

Adds a new entity to your existing `ninjadog.json` configuration file. The entity is created with a default `Guid` primary key.

```bash
ninjadog add-entity <EntityName>
```

{: .note }
> The entity name should be in **PascalCase** (e.g., `Product`, `OrderItem`). A `ninjadog.json` file must already exist in the current directory -- run `ninjadog init` first if you haven't already.

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

### `ninjadog ui`

Starts a local web server with a visual configuration builder for your `ninjadog.json`.

```bash
ninjadog ui [OPTIONS]
```

**Options:**

| Option | Description |
|---|---|
| `--port <number>` | Port number for the web server. Default: `5391` |
| `--no-browser` | Do not open a browser window automatically. |

**What it does:**

The web UI provides a browser-based interface for editing your Ninjadog configuration. Features include:

- **Form-based editing** -- Add and configure entities, properties, enums, and relationships without writing JSON by hand.
- **Live JSON preview** -- See the resulting `ninjadog.json` update in real time as you make changes.
- **Validation** -- Errors and warnings are displayed inline as you edit.
- **Build trigger** -- Run `ninjadog build` directly from the UI and see the output.

{: .tip }
> The web UI is especially useful for exploring configuration options and getting started with a new project. It reads and writes the same `ninjadog.json` file that the CLI uses.

**Example:**

```bash
# Start on the default port and open the browser
ninjadog ui

# Start on a custom port without opening a browser
ninjadog ui --port 8080 --no-browser
```

---

## Next Steps

- [Getting Started](/Ninjadog/getting-started) -- Step-by-step tutorial using the CLI tool
- [Configuration Reference](/Ninjadog/configuration) -- Full reference for ninjadog.json
- [Architecture](/Ninjadog/architecture) -- Understand the project structure
- [Generators](/Ninjadog/generators) -- See what gets generated
- [Generated Examples](/Ninjadog/examples) -- Real generated code from snapshot tests
