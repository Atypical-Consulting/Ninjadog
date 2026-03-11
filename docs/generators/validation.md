---
title: Validation Generators
description: "Ninjadog validation generators: type-aware FluentValidation rules that automatically skip value types and validate reference type properties."
layout: default
parent: Generators
nav_order: 6
---

# Validation Generators

Validation generators produce FluentValidation validators that are **type-aware** -- value types (`int`, `bool`, `decimal`) are automatically skipped since they always have default values.

## Validation Attributes

Beyond automatic type-aware rules, you can declare **validation attributes** directly in your `ninjadog.json` property definitions. These are translated into FluentValidation rules at generation time.

| Attribute | Applies to | FluentValidation rule |
|-----------|------------|----------------------|
| `required` | All types | `.NotEmpty()` |
| `maxLength` | String | `.MaximumLength(n)` |
| `minLength` | String | `.MinimumLength(n)` |
| `min` | Numeric | `.GreaterThanOrEqualTo(n)` |
| `max` | Numeric | `.LessThanOrEqualTo(n)` |
| `pattern` | String | `.Matches("regex")` |

### JSON Configuration Example

```json
{
  "Name": { "type": "String", "required": true, "maxLength": 100, "minLength": 2 },
  "Email": { "type": "String", "required": true, "pattern": "^[^@]+@[^@]+\\.[^@]+$" },
  "Age": { "type": "Int32", "min": 0, "max": 150 }
}
```

### Generated Output

```csharp
public partial class CreateContactRequestValidator : Validator<CreateContactRequest>
{
    public CreateContactRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required!")
            .MinimumLength(2)
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required!")
            .Matches("^[^@]+@[^@]+\\.[^@]+$");

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(150);

    }
}
```

{: .note }
> Validation attributes compose with type-aware rules. A `required` string property gets both `.NotEmpty()` and any length or pattern constraints you define.

---

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
