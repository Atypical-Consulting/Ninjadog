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

---

## Next Steps

- [Getting Started](/Ninjadog/getting-started) -- Step-by-step tutorial using the CLI tool
- [Architecture](/Ninjadog/architecture) -- Understand the project structure
- [Generators](/Ninjadog/generators) -- See what gets generated
