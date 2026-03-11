---
title: Contributing
layout: default
nav_order: 7
---

# Contributing
{: .no_toc }

Contributions are welcome! Here's how to get started.
{: .fs-6 .fw-300 }

<details open markdown="block">
  <summary>Table of contents</summary>
  {: .text-delta }
1. TOC
{:toc}
</details>

---

## Getting Started

```bash
git clone https://github.com/Atypical-Consulting/Ninjadog.git
cd ninjadog
dotnet build
dotnet test
```

{: .tip }
> All tests should pass before you start making changes. If they don't, open an issue.

## Development Workflow

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Run the tests: `dotnet test`
4. Commit using [conventional commits](https://www.conventionalcommits.org/) (`git commit -m 'feat: add amazing feature'`)
5. Push to the branch (`git push origin feature/amazing-feature`)
6. Open a Pull Request

## Where to Look

| Area | Path |
|---|---|
| Generator templates | `src/templates/Ninjadog.Templates.CrudWebApi/` |
| Snapshot tests | `src/tests/Ninjadog.Tests/Templates/` |
| CLI commands | `src/tools/Ninjadog.CLI/` |
| Generator docs | `doc/generators/` |
| This documentation | `docs/` |

## Testing

Ninjadog uses **snapshot testing** -- each generator has a corresponding expected output file. When you modify a generator:

1. Run `dotnet test` to see if snapshots still match
2. If your change is intentional, update the snapshot files in `src/tests/Ninjadog.Tests/Templates/`
3. Run tests again to confirm they pass

## License

This project is licensed under the Apache License 2.0 -- see the [LICENSE](https://github.com/Atypical-Consulting/Ninjadog/blob/dev/LICENSE) file for details.
