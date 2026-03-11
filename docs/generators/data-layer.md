---
title: Data Layer Generators
description: "Ninjadog data layer generators: SQLite database initializer, Dapper repositories, service layer, and connection factory with automatic type mapping."
layout: default
parent: Generators
nav_order: 4
---

# Data Layer Generators

## Database

### DatabaseInitializerGenerator

| Scope | Single File |
|---|---|

Generates a `DatabaseInitializer` class that creates SQLite tables for all entities. Column types are mapped automatically:

| C# Type | SQLite Type |
|---|---|
| `string` | TEXT |
| `int` | INTEGER |
| `bool` | INTEGER |
| `decimal` | REAL |
| `DateTime` | TEXT |
| `DateOnly` | TEXT |
| `Guid` | CHAR(36) |
| `enum` | INTEGER |

### DbConnectionFactoryGenerator

| Scope | Single File |
|---|---|

Generates an `IDbConnectionFactory` interface and implementation for creating SQLite database connections.

## Repositories

### RepositoryGenerator

| Scope | Per Entity |
|---|---|

Generates a concrete repository with Dapper-based CRUD operations (`GetAllAsync`, `GetAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`).

### RepositoryInterfaceGenerator

| Scope | Per Entity |
|---|---|

Generates the repository interface for dependency injection.

## Services

### ServiceGenerator

| Scope | Per Entity |
|---|---|

Generates a service layer that delegates to the repository, providing a clean abstraction for endpoints.

### ServiceInterfaceGenerator

| Scope | Per Entity |
|---|---|

Generates the service interface for dependency injection.
