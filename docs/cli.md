---
title: CLI
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
dotnet tool install -g Ninjadog.CLI
```

{: .tip }
> After installation, run `ninjadog --help` to verify the tool is available.

## Typical Workflow

```
ninjadog init       Create a new project
       ↓
  edit config       Define your entities in ninjadog.json
       ↓
ninjadog build      Run the generators
       ↓
  dotnet run        Launch your API
```

## Commands

### `ninjadog init`

Initializes a new Ninjadog project in the current directory with a default configuration file and project structure.

```bash
ninjadog init
ninjadog init --name MyApp --output ./my-app
```

| Option | Description | Default |
|---|---|---|
| `--name` | Project name | Current directory name |
| `--output` | Output directory | Current directory |

### `ninjadog build`

Builds and runs the generator engine against your project configuration. This reads the `ninjadog.json` file and produces the generated source files.

```bash
ninjadog build
ninjadog build --input ./ninjadog.json
```

| Option | Description | Default |
|---|---|---|
| `--input` | Path to ninjadog.json configuration file | `./ninjadog.json` |

### `ninjadog ninjadog`

Generates a complete Ninjadog project with full scaffolding -- ready to build and run.

```bash
ninjadog ninjadog
```

## Uninstall

```bash
dotnet tool uninstall -g Ninjadog.CLI
```

---

## Next Steps

- [Getting Started](/Ninjadog/getting-started) -- Step-by-step tutorial using the NuGet package
- [Architecture](/Ninjadog/architecture) -- Understand the project structure
- [Generators](/Ninjadog/generators) -- See what gets generated
