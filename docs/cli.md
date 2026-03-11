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
ninjadog validate          Check your configuration for errors
       ↓
ninjadog build             Run the generators
       ↓
  dotnet run               Launch your API
```

## Commands

### `ninjadog init`

Initializes a new Ninjadog project in the current directory with a default configuration file containing a sample `Person` entity.

```bash
ninjadog init
```

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

### `ninjadog validate`

Validates your `ninjadog.json` configuration file by running two passes:

1. **Schema validation** -- checks that the JSON structure conforms to the ninjadog schema (required fields, correct types, allowed values).
2. **Semantic validation** -- checks logical rules such as every entity having exactly one key property, relationship references pointing to existing entities, seed data fields matching property names, and more.

```bash
ninjadog validate
```

**Options:**

| Option | Description |
|:---|:---|
| `-f`, `--file <path>` | Path to the `ninjadog.json` file to validate. Defaults to `ninjadog.json` in the current directory. |
| `--strict` | Treat warnings as errors. The command returns a non-zero exit code if any warnings are present. |

**Example -- validating with a custom path:**

```bash
ninjadog validate --file ./config/ninjadog.json
```

**Example output (valid configuration):**

```
Validating ninjadog.json...

[v] Configuration is valid!
```

**Example output (errors found):**

```
Validating ninjadog.json...

  [x] NINJ001: Entity 'Product' has no key property. Exactly one property must have isKey: true. at entities.Product
  [!] NINJ008: Entity name 'product_item' is not PascalCase. at entities.product_item

Validation failed: 1 error(s), 1 warning(s)
```

{: .tip }
> Run `ninjadog validate` before `ninjadog build` to catch configuration mistakes early. In CI pipelines, use `--strict` to ensure warnings are not ignored.

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
