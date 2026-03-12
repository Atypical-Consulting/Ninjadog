using Ninjadog.Settings.Schema;
using Ninjadog.Settings.Validation;

namespace Ninjadog.Tests.Validation;

public class SchemaValidationTests
{
    private const string ValidMinimalJson = """
        {
          "config": {
            "name": "TestApp",
            "version": "1.0.0",
            "description": "A test app",
            "rootNamespace": "Test.App"
          },
          "entities": {
            "TodoItem": {
              "properties": {
                "Id": { "type": "Guid", "isKey": true },
                "Title": { "type": "string" }
              }
            }
          }
        }
        """;

    private const string ValidFullJson = """
        {
          "config": {
            "name": "TestApp",
            "version": "1.0.0",
            "description": "A test app",
            "rootNamespace": "Test.App",
            "outputPath": "./output",
            "saveGeneratedFiles": true,
            "cors": {
              "origins": ["http://localhost:3000"],
              "methods": ["GET", "POST"],
              "headers": ["Content-Type"]
            },
            "features": {
              "softDelete": true,
              "auditing": true
            },
            "database": {
              "provider": "postgresql"
            }
          },
          "entities": {
            "TodoItem": {
              "properties": {
                "Id": { "type": "Guid", "isKey": true },
                "Title": { "type": "string", "required": true, "maxLength": 200, "minLength": 1 },
                "Priority": { "type": "int", "min": 1, "max": 5 }
              },
              "seedData": [
                { "Id": "550e8400-e29b-41d4-a716-446655440000", "Title": "First item", "Priority": 1 }
              ]
            }
          },
          "enums": {
            "Status": ["Active", "Inactive", "Archived"]
          }
        }
        """;

    [Fact]
    public void SchemaProvider_ReturnsNonNullSchema()
    {
        var schema = SchemaProvider.Schema;
        Assert.NotNull(schema);
    }

    [Fact]
    public void SchemaProvider_GetSchemaText_ReturnsNonEmptyString()
    {
        var text = SchemaProvider.GetSchemaText();
        Assert.False(string.IsNullOrWhiteSpace(text));
        Assert.Contains("\"$schema\"", text);
    }

    [Fact]
    public void Validate_ValidMinimalJson_ReturnsSuccess()
    {
        var result = SchemaValidator.Validate(ValidMinimalJson);
        Assert.True(result.IsValid);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Validate_ValidFullJson_ReturnsSuccess()
    {
        var result = SchemaValidator.Validate(ValidFullJson);
        Assert.True(result.IsValid);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Validate_InvalidJson_ReturnsError()
    {
        var result = SchemaValidator.Validate("{ not valid json }");
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validate_MissingConfig_ReturnsError()
    {
        const string json = """
            {
              "entities": {
                "TodoItem": {
                  "properties": {
                    "Id": { "type": "Guid", "isKey": true }
                  }
                }
              }
            }
            """;

        var result = SchemaValidator.Validate(json);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_MissingEntities_ReturnsError()
    {
        const string json = """
            {
              "config": {
                "name": "TestApp",
                "version": "1.0.0",
                "description": "A test app",
                "rootNamespace": "Test.App"
              }
            }
            """;

        var result = SchemaValidator.Validate(json);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_MissingRequiredConfigField_ReturnsError()
    {
        const string json = """
            {
              "config": {
                "name": "TestApp",
                "version": "1.0.0"
              },
              "entities": {}
            }
            """;

        var result = SchemaValidator.Validate(json);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_InvalidDatabaseProvider_ReturnsError()
    {
        const string json = """
            {
              "config": {
                "name": "TestApp",
                "version": "1.0.0",
                "description": "A test app",
                "rootNamespace": "Test.App",
                "database": {
                  "provider": "mysql"
                }
              },
              "entities": {}
            }
            """;

        var result = SchemaValidator.Validate(json);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_InvalidRelationshipType_ReturnsError()
    {
        const string json = """
            {
              "config": {
                "name": "TestApp",
                "version": "1.0.0",
                "description": "A test app",
                "rootNamespace": "Test.App"
              },
              "entities": {
                "Order": {
                  "properties": {
                    "Id": { "type": "Guid", "isKey": true }
                  },
                  "relationships": {
                    "Customer": {
                      "relatedEntity": "Customer",
                      "relationshipType": "InvalidType"
                    }
                  }
                }
              }
            }
            """;

        var result = SchemaValidator.Validate(json);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_AdditionalPropertyInConfig_ReturnsError()
    {
        const string json = """
            {
              "config": {
                "name": "TestApp",
                "version": "1.0.0",
                "description": "A test app",
                "rootNamespace": "Test.App",
                "unknownField": true
              },
              "entities": {}
            }
            """;

        var result = SchemaValidator.Validate(json);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_EnumsSection_ValidFormat()
    {
        const string json = """
            {
              "config": {
                "name": "TestApp",
                "version": "1.0.0",
                "description": "A test app",
                "rootNamespace": "Test.App"
              },
              "entities": {},
              "enums": {
                "Color": ["Red", "Green", "Blue"]
              }
            }
            """;

        var result = SchemaValidator.Validate(json);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ConfigValidator_ValidJson_ReturnsSuccess()
    {
        var result = NinjadogConfigValidator.Validate(ValidMinimalJson);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ConfigValidator_InvalidSchema_StopsBeforeSemantic()
    {
        var result = NinjadogConfigValidator.Validate("{ invalid }");
        Assert.False(result.IsValid);
        Assert.Contains(result.Diagnostics, d => d.Code == "SCHEMA");
    }
}
