using Ninjadog.Settings;
using Ninjadog.Settings.Entities.Properties;
using Ninjadog.Settings.Extensions;
using Ninjadog.Settings.Extensions.Entities.Properties;

namespace Ninjadog.Tests.Settings;

public class NinjadogEntityPropertyWithKeyTests
{
    [Fact]
    public void GenerateMemberProperty_StringType_IncludesDefaultBang()
    {
        var prop = new NinjadogEntityPropertyWithKey("Name", new NinjadogEntityProperty("string", false));
        var result = prop.GenerateMemberProperty();
        Assert.Contains("public string Name { get; init; } = default!;", result);
    }

    [Fact]
    public void GenerateMemberProperty_IntType_NoDefault()
    {
        var prop = new NinjadogEntityPropertyWithKey("Count", new NinjadogEntityProperty("Int32", false));
        var result = prop.GenerateMemberProperty();
        Assert.Contains("public Int32 Count { get; init; }", result);
        Assert.DoesNotContain("default!", result);
    }

    [Fact]
    public void GenerateMemberProperty_GuidType_NoDefault()
    {
        var prop = new NinjadogEntityPropertyWithKey("Id", new NinjadogEntityId());
        var result = prop.GenerateMemberProperty();
        Assert.Contains("public Guid Id { get; init; }", result);
        Assert.DoesNotContain("default!", result);
    }

    [Fact]
    public void GenerateMemberProperty_BoolType_NoDefault()
    {
        var prop = new NinjadogEntityPropertyWithKey("IsActive", new NinjadogEntityProperty<bool>());
        var result = prop.GenerateMemberProperty();
        Assert.Contains("Boolean IsActive { get; init; }", result);
        Assert.DoesNotContain("default!", result);
    }
}

public class NinjadogEntityPropertiesExtensionsTests
{
    [Fact]
    public void FromKeys_ConvertsToListOfPropertyWithKey()
    {
        var properties = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityId() },
            { "Name", new NinjadogEntityProperty<string>() },
        };

        var result = properties.FromKeys();

        Assert.Equal(2, result.Count);
        Assert.Equal("Id", result[0].Key);
        Assert.Equal("Name", result[1].Key);
    }

    [Fact]
    public void GetEntityKey_ReturnsKeyProperty()
    {
        var properties = new NinjadogEntityProperties
        {
            { "Id", new NinjadogEntityId() },
            { "Name", new NinjadogEntityProperty<string>() },
        };

        var key = properties.GetEntityKey();

        Assert.Equal("Id", key.Key);
        Assert.True(key.IsKey);
    }

    [Fact]
    public void GetEntityKey_WithNonStandardKey_FindsCorrectProperty()
    {
        var properties = new NinjadogEntityProperties
        {
            { "OrderId", new NinjadogEntityProperty("Int32", true) },
            { "CustomerName", new NinjadogEntityProperty<string>() },
        };

        var key = properties.GetEntityKey();

        Assert.Equal("OrderId", key.Key);
        Assert.Equal("Int32", key.Type);
    }
}

public class NinjadogEntityWithKeyTests
{
    [Fact]
    public void StringTokens_DerivedFromKey()
    {
        var entity = Helpers.TestEntities.CreateGuidKeyEntity();
        Assert.Equal("TodoItem", entity.StringTokens.Model);
        Assert.Equal("/todo-items", entity.StringTokens.ModelEndpoint);
    }

    [Fact]
    public void GenerateMemberProperties_GeneratesAllProperties()
    {
        var entity = Helpers.TestEntities.CreateGuidKeyEntity();
        var result = entity.GenerateMemberProperties();

        Assert.Contains("Id", result);
        Assert.Contains("Title", result);
        Assert.Contains("Description", result);
        Assert.Contains("IsCompleted", result);
        Assert.Contains("DueDate", result);
        Assert.Contains("Priority", result);
        Assert.Contains("Cost", result);
    }

    [Fact]
    public void GenerateMemberProperties_ContainsAllPropertyNames()
    {
        var entity = Helpers.TestEntities.CreateStringOnlyEntity();
        var result = entity.GenerateMemberProperties();

        Assert.Contains("Name { get; init; }", result);
        Assert.Contains("Color { get; init; }", result);
    }
}

public class NinjadogInitialSettingsTests
{
    [Fact]
    public void Default_HasExpectedName()
    {
        var settings = new NinjadogInitialSettings();
        Assert.Equal("NinjadogApp", settings.Config.Name);
    }

    [Fact]
    public void Default_HasExpectedVersion()
    {
        var settings = new NinjadogInitialSettings();
        Assert.Equal("1.0.0", settings.Config.Version);
    }

    [Fact]
    public void Default_HasDefaultEntity()
    {
        var settings = new NinjadogInitialSettings();
        Assert.NotEmpty(settings.Entities);
    }

    [Fact]
    public void Default_SaveGeneratedFilesIsTrue()
    {
        var settings = new NinjadogInitialSettings();
        Assert.True(settings.Config.SaveGeneratedFiles);
    }

    [Fact]
    public void CustomName_OverridesDefault()
    {
        var settings = new NinjadogInitialSettings(name: "MyApp");
        Assert.Equal("MyApp", settings.Config.Name);
    }

    [Fact]
    public void CustomSaveGeneratedFiles_OverridesDefault()
    {
        var settings = new NinjadogInitialSettings(saveGeneratedFiles: false);
        Assert.False(settings.Config.SaveGeneratedFiles);
    }

    [Fact]
    public void ToJsonString_ProducesValidJson()
    {
        var settings = new NinjadogInitialSettings();
        var json = settings.ToJsonString();

        Assert.NotNull(json);
        Assert.Contains("NinjadogApp", json);
        Assert.Contains("1.0.0", json);
    }

    [Fact]
    public void FromJsonString_WithAotFeature_ParsesAotFlag()
    {
        const string json = """
            {
              "config": {
                "name": "TestApp",
                "version": "1.0.0",
                "description": "Test",
                "rootNamespace": "TestApp.Api",
                "features": {
                  "aot": true
                }
              },
              "entities": {}
            }
            """;

        var settings = NinjadogSettings.FromJsonString(json);

        Assert.True(settings.Config.Aot);
    }

    [Fact]
    public void FromJsonString_WithoutAotFeature_DefaultsToFalse()
    {
        const string json = """
            {
              "config": {
                "name": "TestApp",
                "version": "1.0.0",
                "description": "Test",
                "rootNamespace": "TestApp.Api"
              },
              "entities": {}
            }
            """;

        var settings = NinjadogSettings.FromJsonString(json);

        Assert.False(settings.Config.Aot);
    }

    [Fact]
    public void FromJsonString_WithAuthConfig_ParsesAuthSettings()
    {
        const string json = """
            {
              "config": {
                "name": "TodoApp",
                "version": "1.0.0",
                "description": "A todo app with auth",
                "rootNamespace": "MyCustomer.TodoApp",
                "auth": {
                  "provider": "jwt",
                  "issuer": "https://myapp.com",
                  "audience": "myapp-api",
                  "tokenExpirationMinutes": 120,
                  "roles": ["Admin", "User"],
                  "generateLoginEndpoint": true,
                  "generateRegisterEndpoint": false
                }
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

        var settings = NinjadogSettings.FromJsonString(json);

        Assert.NotNull(settings.Config.Auth);
        Assert.Equal("jwt", settings.Config.Auth.Provider);
        Assert.Equal("https://myapp.com", settings.Config.Auth.Issuer);
        Assert.Equal("myapp-api", settings.Config.Auth.Audience);
        Assert.Equal(120, settings.Config.Auth.TokenExpirationMinutes);
        Assert.NotNull(settings.Config.Auth.Roles);
        Assert.Equal(["Admin", "User"], settings.Config.Auth.Roles);
        Assert.True(settings.Config.Auth.GenerateLoginEndpoint);
        Assert.False(settings.Config.Auth.GenerateRegisterEndpoint);
    }

    [Fact]
    public void FromJsonString_WithoutAuthConfig_AuthIsNull()
    {
        const string json = """
            {
              "config": {
                "name": "TodoApp",
                "version": "1.0.0",
                "description": "A todo app",
                "rootNamespace": "MyCustomer.TodoApp"
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

        var settings = NinjadogSettings.FromJsonString(json);

        Assert.Null(settings.Config.Auth);
    }

    [Fact]
    public void FromJsonString_WithMinimalAuthConfig_UsesDefaults()
    {
        const string json = """
            {
              "config": {
                "name": "TodoApp",
                "version": "1.0.0",
                "description": "A todo app",
                "rootNamespace": "MyCustomer.TodoApp",
                "auth": {}
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

        var settings = NinjadogSettings.FromJsonString(json);

        Assert.NotNull(settings.Config.Auth);
        Assert.Equal("jwt", settings.Config.Auth.Provider);
        Assert.Equal("https://localhost", settings.Config.Auth.Issuer);
        Assert.Equal("api", settings.Config.Auth.Audience);
        Assert.Equal(60, settings.Config.Auth.TokenExpirationMinutes);
        Assert.Null(settings.Config.Auth.Roles);
        Assert.True(settings.Config.Auth.GenerateLoginEndpoint);
        Assert.True(settings.Config.Auth.GenerateRegisterEndpoint);
    }

    [Fact]
    public void FromJsonString_WithExplicitNullOptionalSections_LoadsSettings()
    {
        const string json = """
            {
              "config": {
                "name": "TodoApp",
                "version": "1.0.0",
                "description": "A simple todo app",
                "rootNamespace": "MyCustomer.TodoApp",
                "outputPath": null,
                "saveGeneratedFiles": null,
                "cors": null,
                "features": null,
                "database": null,
                "auth": null
              },
              "entities": {
                "TodoItem": {
                  "properties": {
                    "Id": {
                      "type": "Guid",
                      "isKey": true,
                      "required": null,
                      "maxLength": null,
                      "minLength": null,
                      "min": null,
                      "max": null,
                      "pattern": null
                    },
                    "Title": {
                      "type": "string"
                    }
                  },
                  "relationships": null,
                  "seedData": null
                }
              },
              "enums": null
            }
            """;

        var settings = NinjadogSettings.FromJsonString(json);

        Assert.Equal("TodoApp", settings.Config.Name);
        Assert.Equal(".", settings.Config.OutputPath);
        Assert.False(settings.Config.SaveGeneratedFiles);
        Assert.Null(settings.Config.Cors);
        Assert.False(settings.Config.SoftDelete);
        Assert.False(settings.Config.Auditing);
        Assert.Equal("sqlite", settings.Config.DatabaseProvider);
        Assert.Null(settings.Config.Auth);
        Assert.Single(settings.Entities);
        Assert.Null(settings.Entities["TodoItem"].Relationships);
        Assert.Null(settings.Entities["TodoItem"].SeedData);
        Assert.Null(settings.Enums);
    }
}
