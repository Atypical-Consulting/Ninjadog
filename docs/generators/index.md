---
title: Generators
description: "Overview of Ninjadog's 33 code generators: endpoints, contracts, data layer, mapping, validation, OpenAPI summaries, and project setup."
layout: default
nav_order: 5
has_children: true
---

# Generators
{: .no_toc }

Ninjadog includes **33 generators** organized into 12 categories. Each generator produces either a single shared file or a per-entity file.
{: .fs-6 .fw-300 }

---

## Generator Overview

| Category | Generators | Scope |
|---|---|---|
| **[Project Setup](/Ninjadog/generators/project-setup)** | ProgramGenerator, AppSettingsGenerator, DomainEntityGenerator | Single file / Per entity |
| **[Core](/Ninjadog/generators/core)** | NinjadogGenerator | Single file |
| **[Contracts -- Data](/Ninjadog/generators/contracts)** | DtoGenerator | Per entity |
| **[Contracts -- Requests](/Ninjadog/generators/contracts)** | Create, Delete, Get, Update | Per entity |
| **[Contracts -- Responses](/Ninjadog/generators/contracts)** | GetAllResponse, Response | Per entity |
| **[Database](/Ninjadog/generators/data-layer)** | DatabaseInitializer, DbConnectionFactory | Single file |
| **[Endpoints](/Ninjadog/generators/endpoints)** | Create, Delete, GetAll, Get, Update | Per entity |
| **[Mapping](/Ninjadog/generators/mapping)** | ApiContract-to-Domain, Domain-to-ApiContract, Domain-to-Dto, Dto-to-Domain | Single file |
| **[Repositories](/Ninjadog/generators/data-layer)** | Repository, RepositoryInterface | Per entity |
| **[Services](/Ninjadog/generators/data-layer)** | Service, ServiceInterface | Per entity |
| **[Summaries](/Ninjadog/generators/openapi)** | Create, Delete, GetAll, Get, Update | Per entity |
| **[Validation](/Ninjadog/generators/validation)** | CreateRequestValidator, UpdateRequestValidator | Per entity |

## Generation Modes

- **Single File** -- One file generated regardless of how many entities exist (e.g., the database initializer creates tables for all entities in one file)
- **Per Entity** -- One file per entity defined in `ninjadog.json` (e.g., each entity gets its own repository, service, and endpoints)

{: .tip }
> All generated classes are `partial`, so you can extend them with custom logic without modifying generated code.
