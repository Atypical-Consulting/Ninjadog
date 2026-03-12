---
title: Project Setup Generators
description: "Ninjadog project setup generators: Program.cs entry point, appsettings.json configuration, index landing page, and domain entity class generation."
layout: default
parent: Generators
nav_order: 0
---

# Project Setup Generators

These generators produce the foundational project files needed for the generated API to compile and run.

## ProgramGenerator

| Scope | Single File |
|---|---|

Generates the `Program.cs` entry point for the web API application. Includes `AddNinjadog()` and `UseNinjadog()` calls to wire up all generated services, endpoints, and middleware.

The generated `Program.cs` includes:

- **Serilog** bootstrap logger for startup error capture
- **Structured logging** via `UseSerilog()` configured from `appsettings.json`
- **Request correlation middleware** (`RequestCorrelationMiddleware`) that assigns a unique correlation ID to every request
- **Request logging** via `UseSerilogRequestLogging()` for automatic HTTP request/response logging
- **Try/catch/finally** wrapper around the application lifecycle with `Log.CloseAndFlushAsync()` to ensure logs are flushed on shutdown

## IndexPageGenerator

| Scope | Single File |
|---|---|

Generates a `wwwroot/index.html` landing page for the API. The page displays:

- **Project name**, version, and description from your `ninjadog.json` config
- Quick links to **Swagger UI**, the **OpenAPI spec**, and generated **C#/TypeScript clients**
- A list of all **entity endpoints** with their routes

The generated API serves this page at the root URL (`/`) using ASP.NET Core static file middleware (`UseDefaultFiles` + `UseStaticFiles`), so navigating to `http://localhost:5000/` shows the landing page instead of a blank 404.

## RequestCorrelationMiddlewareGenerator

| Scope | Single File |
|---|---|

Generates a `Middleware/RequestCorrelationMiddleware.cs` that:

- Reads an incoming `X-Correlation-Id` header, or generates a new GUID if absent
- Pushes the correlation ID into the Serilog `LogContext` so every log entry during the request includes it
- Returns the correlation ID in the `X-Correlation-Id` response header for client-side tracing

## AppSettingsGenerator

| Scope | Single File |
|---|---|

Generates the `appsettings.json` configuration file with default settings for the application, including the SQLite connection string and Serilog structured logging configuration.

The Serilog configuration includes:

- **Console sink** with correlation ID in the output template
- **File sink** with daily rolling, 7-day retention, and correlation ID
- **Enrichers** for `LogContext`, `MachineName`, and `ThreadId`
- **Override levels** to reduce noise from `Microsoft.AspNetCore` and `System`

## DomainEntityGenerator

| Scope | Per Entity |
|---|---|

Generates the domain entity class for each entity defined in `ninjadog.json`. Each domain entity contains all properties from the configuration as a plain C# class.
