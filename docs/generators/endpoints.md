---
title: Endpoint Generators
description: "Ninjadog endpoint generators: Create, GetAll (paginated), Get (with typed route constraints), Update, and Delete using FastEndpoints."
layout: default
parent: Generators
nav_order: 2
---

# Endpoint Generators

Endpoint generators produce files using [FastEndpoints](https://fast-endpoints.com/). Most are **per-entity** CRUD endpoints; health check endpoints are generated once per project.

## CreateEndpointGenerator

Generates a `POST /{entities}` endpoint that accepts a create request, maps it to the domain entity, persists it via the service layer, and returns the created resource.

## GetAllEndpointGenerator

Generates a `GET /{entities}` endpoint with built-in **pagination**, **filtering**, and **sorting** via query parameters:

### Pagination

| Parameter | Default | Description |
|---|---|---|
| `page` | 1 | Page number (1-based) |
| `pageSize` | 10 | Items per page |

### Filtering

Filter results by passing entity property names as query parameters. Only non-key properties are filterable. Filters use exact equality matching and are combined with `AND`.

```
GET /todo-items?IsCompleted=true&Priority=5
```

Invalid property names are silently ignored, preventing SQL injection. The generated code maintains a compile-time whitelist of allowed column names.

### Sorting

| Parameter | Default | Description |
|---|---|---|
| `sortBy` | (none) | Property name to sort by |
| `sortDir` | `asc` | Sort direction: `asc` or `desc` |

```
GET /todo-items?sortBy=DueDate&sortDir=desc
```

The `sortBy` value is validated against the same property whitelist. Invalid sort fields are ignored (no sorting applied).

### Combined example

```
GET /products?Category=Electronics&sortBy=Price&sortDir=asc&page=2&pageSize=20
```

The response includes `TotalCount` metadata reflecting the filtered result set, enabling accurate client-side pagination controls.

## GetEndpointGenerator

Generates a `GET /{entities}/{id}` endpoint with dynamic route constraints based on key type:

| Key Type | Route Constraint | Example Route |
|---|---|---|
| `Guid` | `:guid` | `/products/{id:guid}` |
| `int` | `:int` | `/orders/{id:int}` |
| `string` | (none) | `/categories/{id}` |

Returns `404 Not Found` when the entity does not exist.

## UpdateEndpointGenerator

Generates a `PUT /{entities}/{id}` endpoint that accepts an update request, validates it using the generated FluentValidation validator, and persists the changes.

## DeleteEndpointGenerator

Generates a `DELETE /{entities}/{id}` endpoint that removes the entity and returns the appropriate status code.

## GetByParentEndpointGenerator

Generates nested `GET /{parents}/{parentId}/children` endpoints for entities that define **OneToMany** relationships. This allows clients to fetch child resources scoped to a specific parent.

This generator is only activated when an entity includes a `relationships` block in its configuration:

```json
{
  "Author": {
    "properties": {
      "Id": { "type": "Guid", "isKey": true },
      "Name": { "type": "String" }
    },
    "relationships": {
      "Posts": { "relatedEntity": "Post", "type": "OneToMany" }
    }
  }
}
```

The generated endpoint:

- Uses the parent entity's key type for the route constraint (e.g., `{authorId:guid}`)
- Includes built-in **pagination** via `page` and `pageSize` query parameters
- Is only generated for `OneToMany` relationships -- other relationship types are skipped
- Follows the same FastEndpoints conventions as the standard CRUD endpoints

{: .tip }
> If an entity has no `relationships` block, the generator produces no output for that entity. You can safely add relationships incrementally without affecting existing endpoints.

## Health Check Endpoints

Ninjadog automatically generates two infrastructure endpoints for container orchestrators (Kubernetes, ECS, etc.):

### `/health` -- Liveness Check

A lightweight endpoint that returns `200 OK` with `{ "Status": "Healthy" }`. Use this as your **liveness probe** to confirm the process is running.

### `/ready` -- Readiness Check

Verifies that the database connection is available before returning `200 OK` with `{ "Status": "Ready" }`. Returns `503 Service Unavailable` if the database cannot be reached. Use this as your **readiness probe** to prevent traffic from reaching the service before it can serve requests.

Both endpoints allow anonymous access and require no configuration.

**Kubernetes example:**

```yaml
livenessProbe:
  httpGet:
    path: /health
    port: 8080
readinessProbe:
  httpGet:
    path: /ready
    port: 8080
```

## Generated HTTP Endpoints

For an entity called `Product` with a `Guid` key:

```
GET    /health                Liveness check
GET    /ready                 Readiness check (verifies DB)
POST   /products              Create a new product
GET    /products              List all products (paginated)
GET    /products/{id:guid}    Get a single product
PUT    /products/{id:guid}    Update a product
DELETE /products/{id:guid}    Delete a product
```

When a `Category` entity defines a OneToMany relationship to `Product`:

```
GET    /categories/{categoryId:guid}/products    List products by category (paginated)
```

{: .note }
> All endpoints are generated as `partial` classes, so you can extend them with custom authorization, middleware, or additional logic.
