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

# Run the web-based config builder UI (requires local install)
ninjadog ui --port 5391 --no-browser
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

## Feature / Bugfix Workflow

> **MANDATORY — DO NOT SKIP ANY STEP.**
> Never edit code directly on `dev`. Always use a worktree + feature branch, even for small fixes.
> This workflow applies to every code change, no matter how trivial.

Every feature or bugfix follows this procedure:

1. **Sync dev** — Before starting any work, checkout `dev` and pull the latest changes:
   ```bash
   git checkout dev && git pull origin dev
   ```

2. **Work in a worktree** — Use `git worktree add` (or the `EnterWorktree` tool) to isolate work from the main checkout. This keeps `dev` clean and allows parallel work. **Do NOT edit files on `dev` directly — create the worktree BEFORE writing any code.**

3. **Create a branch** — Branch from `dev` using conventional naming:
   - Features: `feat/<short-description>`
   - Bugfixes: `fix/<short-description>`

4. **Implement & commit** — Make one or more focused commits using conventional commit messages (`feat:`, `fix:`, `chore:`, `docs:`). Each commit should be atomic and build successfully.

5. **Update documentation** — If the change adds or modifies user-facing behavior, update the relevant pages under `docs/` (Jekyll site). Common files to update:
   - `docs/cli.md` — new or changed CLI commands/flags
   - `docs/configuration.md` — new `ninjadog.json` options
   - `docs/generators/` — new or changed templates
   - `docs/getting-started.md` — workflow changes
   - Create new doc pages when the feature warrants its own section

6. **Run all tests** — Ensure the full test suite passes:
   ```bash
   dotnet test
   ```
   If snapshot tests changed, review and commit the updated `.verified.txt` files.

7. **Simplify & verify** — Run `/simplify` to review changed code for reuse, quality, and efficiency. Then launch the UI (`ninjadog ui --port 5391 --no-browser`) and use the browser MCP to test your implementation end-to-end. Fix any problems found during testing before proceeding.

8. **Create a PR** — Push the branch and open a pull request targeting `dev`:
   ```bash
   gh pr create --base dev --title "feat: short description" --body "..."
   ```
