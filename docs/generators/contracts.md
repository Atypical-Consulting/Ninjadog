---
title: Contracts
layout: default
parent: Generators
nav_order: 3
---

# Contract Generators

Contract generators produce the request/response DTOs and data transfer objects for each entity.

## DtoGenerator

| Scope | Per Entity |
|---|---|

Generates a data transfer object (DTO) for each entity, containing all public properties.

## Request Generators

### CreateRequestGenerator

Generates the request object for the Create endpoint, including all writable properties (excluding the key).

### DeleteRequestGenerator

Generates the request object for the Delete endpoint, containing only the entity key.

### GetRequestGenerator

Generates the request object for the Get endpoint, containing only the entity key.

### UpdateRequestGenerator

Generates the request object for the Update endpoint, including the key and all writable properties.

## Response Generators

### ResponseGenerator

Generates the single-entity response object, wrapping the DTO.

### GetAllResponseGenerator

Generates the paginated response object with properties for the entity collection (named after the plural entity, e.g., `Products`, `TodoItems`), `Page`, `PageSize`, and `TotalCount`.
