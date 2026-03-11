---
title: Mapping Generators
description: "Ninjadog mapping generators: static extension methods for domain-to-DTO, DTO-to-domain, request-to-domain, and domain-to-response conversions."
layout: default
parent: Generators
nav_order: 5
---

# Mapping Generators

All mapping generators produce **single files** containing extension methods for all entities. This means one file covers mappings for `Product`, `Order`, `Movie`, etc.

## ApiContractToDomainMapperGenerator

Maps API contract (request) objects to domain entities. Used by Create and Update endpoints to convert incoming requests into domain objects.

```csharp
// Example: generated extension method
public static Product ToProduct(this CreateProductRequest request)
{
    return new Product
    {
        Id = Guid.NewGuid(),
        Name = request.Name,
        Price = request.Price
    };
}
```

## DomainToApiContractMapperGenerator

Maps domain entities to API contract (response) objects. Used by all read endpoints to convert domain objects into responses sent to the client.

## DomainToDtoMapperGenerator

Maps domain entities to DTOs. Used internally by the data layer when writing to the database.

## DtoToDomainMapperGenerator

Maps DTOs back to domain entities. Used when reading from the database layer, converting raw data rows into domain objects.

{: .tip }
> All mappers are generated as `static` extension methods, so you can call them naturally: `product.ToProductResponse()` or `request.ToProduct()`.
