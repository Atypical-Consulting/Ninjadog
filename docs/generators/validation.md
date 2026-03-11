---
title: Validation Generators
description: "Ninjadog validation generators: type-aware FluentValidation rules that automatically skip value types and validate reference type properties."
layout: default
parent: Generators
nav_order: 6
---

# Validation Generators

Validation generators produce FluentValidation validators that are **type-aware** -- value types (`int`, `bool`, `decimal`) are automatically skipped since they always have default values.

## CreateRequestValidatorGenerator

| Scope | Per Entity |
|---|---|

Generates validation rules for the Create request. Reference type properties get `.NotEmpty()` rules automatically.

### Example Output

```csharp
public partial class CreateTodoItemRequestValidator : Validator<CreateTodoItemRequest>
{
    public CreateTodoItemRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required!");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required!");

        RuleFor(x => x.DueDate)
            .NotEmpty()
            .WithMessage("DueDate is required!");

    }
}
```

{: .note }
> Notice that `IsCompleted` (bool), `Priority` (int), and `Cost` (decimal) are skipped because they are value types.

## UpdateRequestValidatorGenerator

| Scope | Per Entity |
|---|---|

Generates validation rules for the Update request, following the same type-aware pattern as the Create validator.
