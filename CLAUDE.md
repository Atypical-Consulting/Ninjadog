# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What is Ninjadog?

Ninjadog is a .NET CLI tool that generates CRUD Web API boilerplate from a single `ninjadog.json` config file. It produces ~30 files per entity (endpoints, DTOs, validators, repositories, database initializers, etc.) using a template engine.

## Build & Test

```bash
# Build
dotnet build

# Run all tests
dotnet test

# Run a single test by name
dotnet test --filter "FullyQualifiedName~RepositoryTemplateTests"

# Install CLI locally (preferred method)
./scripts/install.sh
```

The install script derives version from the latest git tag with a `-local` suffix, bypassing MinVer.

## Architecture

**Template pipeline:** `ninjadog.json` → `NinjadogSettings` (record) → `NinjadogEngine` → `NinjadogTemplate.GenerateOne()`/`GenerateMany()` → `NinjadogContentFile` → output processors write to disk.

**Key layers:**
- `Ninjadog.Settings` — Parses `ninjadog.json` into `NinjadogSettings` (Config + Entities + Enums)
- `Ninjadog.Engine.Core` — Base `NinjadogTemplate` abstraction, domain events, `NinjadogContentFile`
- `Ninjadog.Engine` — Orchestration: loads templates from manifest, runs pipeline
- `Ninjadog.Templates.CrudWebApi` — All code-gen templates, registered via `CrudTemplates`/`CrudTemplateManifest`
- `Ninjadog.CLI` — Spectre.Console.Cli commands: `init`, `build`, `validate`, `ui`, `add-entity`

**Adding a new template:** Extend `NinjadogTemplate`, implement `GenerateOneByEntity()`, register in `CrudTemplates.cs`.

**Entity naming:** `NinjadogEntityWithKey.StringTokens` converts an entity key (e.g. "TodoItem") into all naming variants used in templates (PascalCase, camelCase, plural, endpoint names, etc.).

## Testing

- **xUnit** + **Verify.XUnit** snapshot testing — `.verified.txt` files are committed and must be included with test changes
- **Shouldly** for assertions
- Update snapshots: delete the `.verified.txt` file and re-run the test, then review and commit the new snapshot

## Code Conventions

- .NET 10, C# latest, nullable enabled, implicit usings
- File-scoped namespaces only
- Records for immutable data models
- No copyright headers in `.cs` files (removed, StyleCop file header rules disabled)
- Code analysis enforced at build time (StyleCop, Roslynator, `EnforceCodeStyleInBuild=true`)
- Conventional commits: `feat:`, `fix:`, `chore:`, `docs:`

## Versioning

- **MinVer** derives version from git tags (`v{major}.{minor}.{patch}`)
- CI releases trigger on tag push matching `v*`
- Local installs use `./scripts/install.sh` which appends `-local` suffix

## Branching

- `dev` is the main development branch (PR target)
- Feature branches branch from `dev`
