// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

using Ninjadog.Settings;
using Ninjadog.Settings.Config;
using Ninjadog.Settings.Entities;
using Ninjadog.Settings.Entities.Properties;
using Ninjadog.Settings.Validation;

namespace Ninjadog.Tests.Validation;

public class SemanticValidationTests
{
    [Fact]
    public void NINJ001_EntityWithOneKey_NoError()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) },
            { "Name", new NinjadogEntityProperty("string") }
        };

        var settings = CreateSettings(new Dictionary<string, NinjadogEntity>
        {
            { "TodoItem", CreateEntity(props) }
        });

        var result = SemanticValidator.Validate(settings);
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == "NINJ001");
    }

    [Fact]
    public void NINJ001_EntityWithNoKey_ReturnsError()
    {
        var props = new NinjadogEntityProperties
        {
            { "Name", new NinjadogEntityProperty("string") }
        };

        var settings = CreateSettings(new Dictionary<string, NinjadogEntity>
        {
            { "TodoItem", CreateEntity(props) }
        });

        var result = SemanticValidator.Validate(settings);
        Assert.Contains(result.Diagnostics, d => d.Code == "NINJ001");
        Assert.False(result.IsValid);
    }

    [Fact]
    public void NINJ001_EntityWithMultipleKeys_ReturnsError()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) },
            { "Code", new NinjadogEntityProperty("string", IsKey: true) }
        };

        var settings = CreateSettings(new Dictionary<string, NinjadogEntity>
        {
            { "TodoItem", CreateEntity(props) }
        });

        var result = SemanticValidator.Validate(settings);
        Assert.Contains(result.Diagnostics, d => d.Code == "NINJ001");
    }

    [Fact]
    public void NINJ002_ValidRelationship_NoError()
    {
        var orderProps = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) }
        };
        var customerProps = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) }
        };
        var rels = new NinjadogEntityRelationships
        {
            { "Customer", new NinjadogEntityRelationship("Customer", NinjadogEntityRelationshipType.OneToMany) }
        };

        var settings = CreateSettings(new Dictionary<string, NinjadogEntity>
        {
            { "Order", CreateEntity(orderProps, rels) },
            { "Customer", CreateEntity(customerProps) }
        });

        var result = SemanticValidator.Validate(settings);
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == "NINJ002");
    }

    [Fact]
    public void NINJ002_InvalidRelatedEntity_ReturnsError()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) }
        };
        var rels = new NinjadogEntityRelationships
        {
            { "Customer", new NinjadogEntityRelationship("NonExistent", NinjadogEntityRelationshipType.OneToMany) }
        };

        var settings = CreateSettings(new Dictionary<string, NinjadogEntity>
        {
            { "Order", CreateEntity(props, rels) }
        });

        var result = SemanticValidator.Validate(settings);
        Assert.Contains(result.Diagnostics, d => d.Code == "NINJ002");
    }

    [Fact]
    public void NINJ003_ValidSeedDataFields_NoError()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) },
            { "Name", new NinjadogEntityProperty("string") }
        };
        var seedData = new List<Dictionary<string, object>>
        {
            new() { { "Id", "550e8400-e29b-41d4-a716-446655440000" }, { "Name", "Test" } }
        };

        var settings = CreateSettings(new Dictionary<string, NinjadogEntity>
        {
            { "TodoItem", CreateEntity(props, seedData: seedData) }
        });

        var result = SemanticValidator.Validate(settings);
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == "NINJ003");
    }

    [Fact]
    public void NINJ003_InvalidSeedDataField_ReturnsError()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) },
            { "Name", new NinjadogEntityProperty("string") }
        };
        var seedData = new List<Dictionary<string, object>>
        {
            new() { { "Id", "550e8400-e29b-41d4-a716-446655440000" }, { "NonExistentField", "Test" } }
        };

        var settings = CreateSettings(new Dictionary<string, NinjadogEntity>
        {
            { "TodoItem", CreateEntity(props, seedData: seedData) }
        });

        var result = SemanticValidator.Validate(settings);
        Assert.Contains(result.Diagnostics, d => d.Code == "NINJ003");
    }

    [Fact]
    public void NINJ004_MatchingTypes_NoWarning()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) },
            { "Name", new NinjadogEntityProperty("string") },
            { "Count", new NinjadogEntityProperty("int") }
        };
        var seedData = new List<Dictionary<string, object>>
        {
            new() { { "Id", "550e8400-e29b-41d4-a716-446655440000" }, { "Name", "Test" }, { "Count", 5 } }
        };

        var settings = CreateSettings(new Dictionary<string, NinjadogEntity>
        {
            { "TodoItem", CreateEntity(props, seedData: seedData) }
        });

        var result = SemanticValidator.Validate(settings);
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == "NINJ004");
    }

    [Fact]
    public void NINJ004_MismatchedType_ReturnsWarning()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) },
            { "Count", new NinjadogEntityProperty("int") }
        };
        var seedData = new List<Dictionary<string, object>>
        {
            new() { { "Id", "550e8400-e29b-41d4-a716-446655440000" }, { "Count", "not-a-number" } }
        };

        var settings = CreateSettings(new Dictionary<string, NinjadogEntity>
        {
            { "TodoItem", CreateEntity(props, seedData: seedData) }
        });

        var result = SemanticValidator.Validate(settings);
        Assert.Contains(result.Diagnostics, d => d.Code == "NINJ004" && d.Severity == ValidationSeverity.Warning);
    }

    [Fact]
    public void NINJ005_BuiltInType_NoError()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) },
            { "Name", new NinjadogEntityProperty("string") },
            { "Tags", new NinjadogEntityProperty("List<string>") }
        };

        var settings = CreateSettings(new Dictionary<string, NinjadogEntity>
        {
            { "TodoItem", CreateEntity(props) }
        });

        var result = SemanticValidator.Validate(settings);
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == "NINJ005");
    }

    [Fact]
    public void NINJ005_CustomTypeInEnums_NoError()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) },
            { "Status", new NinjadogEntityProperty("Status") }
        };
        var enums = new Dictionary<string, List<string>>
        {
            { "Status", ["Active", "Inactive"] }
        };

        var settings = CreateSettings(
            new Dictionary<string, NinjadogEntity>
            {
                { "TodoItem", CreateEntity(props) }
            },
            enums);

        var result = SemanticValidator.Validate(settings);
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == "NINJ005");
    }

    [Fact]
    public void NINJ005_UnknownType_ReturnsError()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) },
            { "Status", new NinjadogEntityProperty("UnknownType") }
        };

        var settings = CreateSettings(new Dictionary<string, NinjadogEntity>
        {
            { "TodoItem", CreateEntity(props) }
        });

        var result = SemanticValidator.Validate(settings);
        Assert.Contains(result.Diagnostics, d => d.Code == "NINJ005");
    }

    [Fact]
    public void NINJ006_MinLengthExceedsMaxLength_ReturnsError()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) },
            { "Name", new NinjadogEntityProperty("string", MinLength: 100, MaxLength: 10) }
        };

        var settings = CreateSettings(new Dictionary<string, NinjadogEntity>
        {
            { "TodoItem", CreateEntity(props) }
        });

        var result = SemanticValidator.Validate(settings);
        Assert.Contains(result.Diagnostics, d => d.Code == "NINJ006");
    }

    [Fact]
    public void NINJ006_ValidLengthRange_NoError()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) },
            { "Name", new NinjadogEntityProperty("string", MinLength: 1, MaxLength: 200) }
        };

        var settings = CreateSettings(new Dictionary<string, NinjadogEntity>
        {
            { "TodoItem", CreateEntity(props) }
        });

        var result = SemanticValidator.Validate(settings);
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == "NINJ006");
    }

    [Fact]
    public void NINJ007_MinExceedsMax_ReturnsError()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) },
            { "Priority", new NinjadogEntityProperty("int", Min: 10, Max: 1) }
        };

        var settings = CreateSettings(new Dictionary<string, NinjadogEntity>
        {
            { "TodoItem", CreateEntity(props) }
        });

        var result = SemanticValidator.Validate(settings);
        Assert.Contains(result.Diagnostics, d => d.Code == "NINJ007");
    }

    [Fact]
    public void NINJ007_ValidRange_NoError()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) },
            { "Priority", new NinjadogEntityProperty("int", Min: 1, Max: 10) }
        };

        var settings = CreateSettings(new Dictionary<string, NinjadogEntity>
        {
            { "TodoItem", CreateEntity(props) }
        });

        var result = SemanticValidator.Validate(settings);
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == "NINJ007");
    }

    [Fact]
    public void NINJ008_PascalCaseEntityName_NoWarning()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) }
        };

        var settings = CreateSettings(new Dictionary<string, NinjadogEntity>
        {
            { "TodoItem", CreateEntity(props) }
        });

        var result = SemanticValidator.Validate(settings);
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == "NINJ008");
    }

    [Fact]
    public void NINJ008_NonPascalCaseEntityName_ReturnsWarning()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) }
        };

        var settings = CreateSettings(new Dictionary<string, NinjadogEntity>
        {
            { "todoItem", CreateEntity(props) }
        });

        var result = SemanticValidator.Validate(settings);
        Assert.Contains(result.Diagnostics, d => d.Code == "NINJ008" && d.Severity == ValidationSeverity.Warning);
    }

    [Fact]
    public void NINJ009_UniqueEnumValues_NoWarning()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) }
        };
        var enums = new Dictionary<string, List<string>>
        {
            { "Status", ["Active", "Inactive", "Archived"] }
        };

        var settings = CreateSettings(
            new Dictionary<string, NinjadogEntity>
            {
                { "TodoItem", CreateEntity(props) }
            },
            enums);

        var result = SemanticValidator.Validate(settings);
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == "NINJ009");
    }

    [Fact]
    public void NINJ009_DuplicateEnumValues_ReturnsWarning()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) }
        };
        var enums = new Dictionary<string, List<string>>
        {
            { "Status", ["Active", "Inactive", "Active"] }
        };

        var settings = CreateSettings(
            new Dictionary<string, NinjadogEntity>
            {
                { "TodoItem", CreateEntity(props) }
            },
            enums);

        var result = SemanticValidator.Validate(settings);
        Assert.Contains(result.Diagnostics, d => d.Code == "NINJ009" && d.Severity == ValidationSeverity.Warning);
    }

    [Fact]
    public void NINJ010_UniqueSeedDataKeys_NoWarning()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) },
            { "Name", new NinjadogEntityProperty("string") }
        };
        var seedData = new List<Dictionary<string, object>>
        {
            new() { { "Id", "aaa" }, { "Name", "First" } },
            new() { { "Id", "bbb" }, { "Name", "Second" } }
        };

        var settings = CreateSettings(new Dictionary<string, NinjadogEntity>
        {
            { "TodoItem", CreateEntity(props, seedData: seedData) }
        });

        var result = SemanticValidator.Validate(settings);
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == "NINJ010");
    }

    [Fact]
    public void NINJ010_DuplicateSeedDataKeys_ReturnsWarning()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) },
            { "Name", new NinjadogEntityProperty("string") }
        };
        var seedData = new List<Dictionary<string, object>>
        {
            new() { { "Id", "same-key" }, { "Name", "First" } },
            new() { { "Id", "same-key" }, { "Name", "Second" } }
        };

        var settings = CreateSettings(new Dictionary<string, NinjadogEntity>
        {
            { "TodoItem", CreateEntity(props, seedData: seedData) }
        });

        var result = SemanticValidator.Validate(settings);
        Assert.Contains(result.Diagnostics, d => d.Code == "NINJ010" && d.Severity == ValidationSeverity.Warning);
    }

    [Fact]
    public void Validate_ValidSettings_ReturnsNoErrors()
    {
        var props = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityProperty("Guid", IsKey: true) },
            { "Name", new NinjadogEntityProperty("string", Required: true, MinLength: 1, MaxLength: 200) },
            { "Priority", new NinjadogEntityProperty("int", Min: 1, Max: 5) }
        };

        var settings = CreateSettings(new Dictionary<string, NinjadogEntity>
        {
            { "TodoItem", CreateEntity(props) }
        });

        var result = SemanticValidator.Validate(settings);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void SchemaValidationResult_ErrorsAndWarnings_FilterCorrectly()
    {
        var diagnostics = new List<ValidationDiagnostic>
        {
            new("NINJ001", "Error message", ValidationSeverity.Error, "$"),
            new("NINJ008", "Warning message", ValidationSeverity.Warning, "$"),
            new("NINJ009", "Another warning", ValidationSeverity.Warning, "$")
        };

        var result = new SchemaValidationResult(false, diagnostics);

        Assert.Single(result.Errors);
        Assert.Equal(2, result.Warnings.Count);
    }

    private static NinjadogLoadedSettings CreateSettings(
        Dictionary<string, NinjadogEntity>? entities = null,
        Dictionary<string, List<string>>? enums = null)
    {
        var config = new NinjadogLoadedConfiguration(
            Name: "TestApp",
            Version: "1.0.0",
            Description: "Test",
            RootNamespace: "Test.App",
            OutputPath: ".");

        var loadedEntities = new NinjadogLoadedEntities();
        if (entities is not null)
        {
            foreach (var (key, value) in entities)
            {
                loadedEntities.Add(key, value);
            }
        }

        return new NinjadogLoadedSettings(config, loadedEntities, enums);
    }

    private static NinjadogEntity CreateEntity(
        NinjadogEntityProperties? properties = null,
        NinjadogEntityRelationships? relationships = null,
        List<Dictionary<string, object>>? seedData = null)
    {
        return new NinjadogEntity(properties ?? [], relationships, seedData);
    }
}
