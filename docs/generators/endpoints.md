---
title: Endpoint Generators
description: "Ninjadog endpoint generators: Create, GetAll (paginated), Get (with typed route constraints), Update, and Delete using FastEndpoints."
layout: default
parent: Generators
nav_order: 2
---

# Endpoint Generators

All endpoint generators produce **per-entity** files using [FastEndpoints](https://fast-endpoints.com/).

## CreateEndpointGenerator

Generates a `POST /{entities}` endpoint that accepts a create request, maps it to the domain entity, persists it via the service layer, and returns the created resource.

## GetAllEndpointGenerator

Generates a `GET /{entities}` endpoint with built-in pagination support via query parameters:

| Parameter | Default | Description |
|---|---|---|
| `page` | 1 | Page number (1-based) |
| `pageSize` | 10 | Items per page |

The response includes `TotalCount` metadata for client-side pagination controls.

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

## Generated HTTP Endpoints

For an entity called `Product` with a `Guid` key:

```
POST   /products              Create a new product
GET    /products              List all products (paginated)
GET    /products/{id:guid}    Get a single product
PUT    /products/{id:guid}    Update a product
DELETE /products/{id:guid}    Delete a product
```

{: .note }
> All endpoints are generated as `partial` classes, so you can extend them with custom authorization, middleware, or additional logic.
