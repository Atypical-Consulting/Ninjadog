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

### From Source (recommended)

```bash
git clone https://github.com/Atypical-Consulting/Ninjadog.git
cd ninjadog
dotnet build
```

{: .note }
> NuGet package publishing is on the roadmap. Once published, you'll be able to install via `dotnet tool install -g Ninjadog.CLI`.

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

Initializes a new Ninjadog project in the current directory with a default configuration file containing a sample `Person` entity.

```bash
ninjadog init
```

### `ninjadog build`

Builds and runs the generator engine against your project configuration. This reads the `ninjadog.json` file in the current directory and produces the generated source files.

```bash
ninjadog build
```

### `ninjadog ninjadog`

Generates a complete Ninjadog project with full scaffolding -- ready to build and run.

```bash
ninjadog ninjadog
```

---

## Next Steps

- [Getting Started](/Ninjadog/getting-started) -- Step-by-step tutorial using the CLI tool
- [Architecture](/Ninjadog/architecture) -- Understand the project structure
- [Generators](/Ninjadog/generators) -- See what gets generated
