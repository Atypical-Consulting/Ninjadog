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

## IndexPageGenerator

| Scope | Single File |
|---|---|

Generates a `wwwroot/index.html` landing page for the API. The page displays:

- **Project name**, version, and description from your `ninjadog.json` config
- Quick links to **Swagger UI**, the **OpenAPI spec**, and generated **C#/TypeScript clients**
- A list of all **entity endpoints** with their routes

The generated API serves this page at the root URL (`/`) using ASP.NET Core static file middleware (`UseDefaultFiles` + `UseStaticFiles`), so navigating to `http://localhost:5000/` shows the landing page instead of a blank 404.

## AppSettingsGenerator

| Scope | Single File |
|---|---|

Generates the `appsettings.json` configuration file with default settings for the application, including the SQLite connection string.

## DomainEntityGenerator

| Scope | Per Entity |
|---|---|

Generates the domain entity class for each entity defined in `ninjadog.json`. Each domain entity contains all properties from the configuration as a plain C# class.
