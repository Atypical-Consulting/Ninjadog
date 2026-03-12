---
title: Core Generator
description: "Ninjadog's core NinjadogGenerator: the root orchestrator that reads entity definitions and produces dependency injection registration code."
layout: default
parent: Generators
nav_order: 1
---

# Core Generator

## NinjadogGenerator

| | |
|---|---|
| **Scope** | Single File |
| **Namespace** | Ninjadog |

The root generator that orchestrates the entire code generation pipeline. It reads all entities from the `ninjadog.json` configuration and delegates to the appropriate category-specific generators.

### What It Does

1. Reads entity definitions from the `ninjadog.json` configuration
2. Extracts metadata for each entity: class name, namespace, properties, key type
3. Invokes each category generator (endpoints, contracts, data layer, etc.) with the collected metadata
4. Produces the dependency injection registration code that wires all generated services into the DI container

{: .note }
> You never call `NinjadogGenerator` directly -- it runs automatically when you execute `ninjadog build` via the CLI.

### Generated Registration

The core generator also produces service registration extension methods so all generated repositories, services, and validators are automatically registered in your DI container.

### Global Error Handling (RFC 9457)

The core generator produces a global exception handler that returns standardized [RFC 9457 Problem Details](https://www.rfc-editor.org/rfc/rfc9457) responses for all errors. Every error response uses the `application/problem+json` content type.

**Validation errors (400)** -- When a `FluentValidation.ValidationException` is thrown (e.g., invalid create/update request), the handler returns:

```json
{
  "status": 400,
  "title": "One or more validation errors occurred.",
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "detail": "See the errors property for details.",
  "errors": {
    "Name": ["'Name' must not be empty."],
    "Price": ["'Price' must be greater than or equal to '0'."]
  }
}
```

**Unhandled exceptions (500)** -- Any other unhandled exception returns a generic response without leaking internal details:

```json
{
  "status": 500,
  "title": "An unexpected error occurred.",
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1"
}
```

{: .note }
> The generated OpenAPI summaries document `ProblemDetails` as the 400 response type for create and update endpoints.
