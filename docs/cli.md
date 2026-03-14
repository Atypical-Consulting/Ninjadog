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
       |                       --- or ---
ninjadog add-entity        (Optional) Add more      ninjadog ui
       |                   entities from the CLI    Launch the visual
ninjadog validate          Check your config         config builder
       |                   for errors                    |
ninjadog build             Run the generators        ninjadog build
       |
  dotnet run               Launch your API

ninjadog update            Refresh schema after upgrading the CLI
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
| `--template <name>` | Template to use (e.g. `CrudWebAPI`). Skips the template prompt. |
| `--use-case <name>` | Use case to scaffold (`TodoApp`, `RestaurantBooking`, or `Custom`). Skips the use case prompt. |
| `--from-prompt <text>` | Generate config from a natural language description using AI. Requires `ANTHROPIC_API_KEY` environment variable. |

#### Template and use case selection

When you run `ninjadog init` interactively, the CLI first asks you to choose a **template** and a **use case**:

| Prompt | Choices | Description |
|---|---|---|
| **Template** | `CrudWebAPI` | The code generation template to use. Currently only CrudWebAPI is available. |
| **Use case** | `TodoApp`, `RestaurantBooking`, `Custom` | A pre-built use case with entities already defined, or `Custom` to define your own entities interactively. |

Selecting `TodoApp` or `RestaurantBooking` creates a fully configured `ninjadog.json` with pre-defined entities, relationships, and seed data. Selecting `Custom` continues with the interactive prompts below.

#### Interactive prompts (Custom use case)

When you select the `Custom` use case, the CLI asks for the following project settings:

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

**Examples:**

```bash
# Interactive mode (prompts for template and use case)
ninjadog init

# Scaffold TodoApp use case directly
ninjadog init --use-case TodoApp

# Scaffold RestaurantBooking use case
ninjadog init -u RestaurantBooking

# Non-interactive with defaults
ninjadog init --default

# Non-interactive with custom values
ninjadog init --name MyApi --namespace MyApi.Web

# Generate from natural language (requires ANTHROPIC_API_KEY)
ninjadog init --from-prompt "A blog platform with users, posts, tags, and comments"
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

### `ninjadog update`

Updates the `ninjadog.schema.json` file in the current directory to the version embedded in the installed CLI tool. This is useful after upgrading Ninjadog to get IDE autocompletion and validation for newly added configuration options.

```bash
ninjadog update
```

**Example:**

```
$ ninjadog update
Schema file updated successfully.
  -> ninjadog.schema.json
```

{: .tip }
> Run `ninjadog update` after upgrading the CLI tool to ensure your local schema stays in sync with the installed version.

### `ninjadog build`

Builds and runs the generator engine against your project configuration. This reads the `ninjadog.json` file in the current directory and produces the generated source files.

```bash
ninjadog build
```

### `ninjadog ui`

Launches a local web server that hosts a visual configuration builder for your `ninjadog.json` file. The UI lets you create and edit entities, enums, and seed data through a browser-based interface instead of editing JSON by hand.

```bash
ninjadog ui [OPTIONS]
```

**Options:**

| Option | Description | Default |
|---|---|---|
| `--port <number>` | Port number for the local web server | `5111` |
| `--path <path>` | Path to the `ninjadog.json` file to edit | `./ninjadog.json` |
| `--no-open` | Do not open the browser automatically | `false` |

**Visual builder features:**

- **Entity editor** -- Add, rename, clone, and remove entities. Define properties with types, key markers, and validation rules. Drag-and-drop property reordering, quick-add presets (ID, Name, Timestamps, Email, Description), and bulk property actions.
- **Enum editor** -- Define enums with named values. Inline add/remove with two-click delete confirmation.
- **Seed data editor** -- Populate initial data rows for each entity. Auto-generated key values (Guid, int, long), CSV/JSON import, and cell-level type validation.
- **Live JSON preview** -- See the generated `ninjadog.json` update in real time with Monaco Editor syntax highlighting and schema validation.
- **Validation** -- Live schema validation with error paths displayed inline.
- **Undo / Redo** -- Full undo/redo history (up to 50 states) with <kbd>Ctrl+Z</kbd> / <kbd>Ctrl+Y</kbd> keyboard shortcuts.
- **AI Assistant** -- Describe your API in plain English and generate a complete configuration. Toggle the chat panel with the sparkle icon or <kbd>Ctrl+Shift+A</kbd>. Supports multi-turn conversation for iterating on the schema. Requires `ANTHROPIC_API_KEY` environment variable.
- **Keyboard shortcuts** -- <kbd>Ctrl+S</kbd> save, <kbd>Ctrl+B</kbd> build, <kbd>Ctrl+E</kbd> add entity, <kbd>Ctrl+Shift+A</kbd> AI assistant, <kbd>1</kbd>--<kbd>4</kbd> switch tabs, <kbd>?</kbd> show shortcut overlay.
- **Auto-save** -- Optional auto-save to localStorage with 3-second debounce.
- **View modes** -- Toggle between Split View (form + JSON), Form Only, or JSON Only layouts.
- **Template picker** -- Start from pre-built templates (Blank, Todo App, Blog API, E-Commerce) instead of an empty configuration.
- **Import** -- Import seed data from CSV or JSON arrays via a modal dialog.
- **Export** -- Download the current configuration as a JSON file.
- **Entity color coding** -- Each entity gets a unique color dot for quick visual identification across all tabs.

{: .tip }
> By default, `ninjadog ui` opens your browser automatically. Use `--no-open` if you are running on a headless server or want to open the URL manually.

{: .note }
> A `ninjadog.json` file must already exist in the target directory. Run `ninjadog init` first if you haven't already.

**Example:**

```bash
# Start on the default port and open the browser
ninjadog ui

# Start on a custom port without opening a browser
ninjadog ui --port 8080 --no-open
```

---

## Next Steps

- [Getting Started](/Ninjadog/getting-started) -- Step-by-step tutorial using the CLI tool
- [Configuration Reference](/Ninjadog/configuration) -- Full reference for ninjadog.json
- [Architecture](/Ninjadog/architecture) -- Understand the project structure
- [Generators](/Ninjadog/generators) -- See what gets generated
- [Generated Examples](/Ninjadog/examples) -- Real generated code from snapshot tests
