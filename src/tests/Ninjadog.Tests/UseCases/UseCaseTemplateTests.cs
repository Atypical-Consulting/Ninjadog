using Ninjadog.Settings;
using Ninjadog.Templates.CrudWebAPI.Setup;
using Ninjadog.Templates.CrudWebAPI.UseCases;
using Shouldly;

namespace Ninjadog.Tests.UseCases;

/// <summary>
/// End-to-end tests that validate UseCase settings produce valid template output
/// across all registered CrudWebApi templates.
/// </summary>
public class UseCaseTemplateTests
{
#pragma warning disable IDE0028 // CrudTemplates populates items in its constructor
    private readonly CrudTemplates _templates = new();
#pragma warning restore IDE0028

    [Fact]
    public void TodoApp_AllTemplates_ProduceValidOutput()
    {
        ValidateAllTemplatesProduceValidOutput(UseCaseSettings.TodoApp());
    }

    [Fact]
    public void RestaurantBooking_AllTemplates_ProduceValidOutput()
    {
        ValidateAllTemplatesProduceValidOutput(UseCaseSettings.RestaurantBooking());
    }

    [Fact]
    public void TodoApp_Entities_HaveExpectedCount()
    {
        var settings = UseCaseSettings.TodoApp();
        settings.Entities.Count.ShouldBe(3);
        settings.Entities.ShouldContainKey("TodoList");
        settings.Entities.ShouldContainKey("TodoItem");
        settings.Entities.ShouldContainKey("TodoCategory");
    }

    [Fact]
    public void TodoApp_TodoList_HasRelationships()
    {
        var settings = UseCaseSettings.TodoApp();
        var todoList = settings.Entities["TodoList"];
        todoList.Relationships.ShouldNotBeNull();
        todoList.Relationships.Count.ShouldBe(2);
        todoList.Relationships.ShouldContainKey("Items");
        todoList.Relationships.ShouldContainKey("Categories");
    }

    [Fact]
    public void RestaurantBooking_Entities_HaveExpectedCount()
    {
        var settings = UseCaseSettings.RestaurantBooking();
        settings.Entities.Count.ShouldBe(12);
    }

    [Fact]
    public void RestaurantBooking_Customer_HasRelationships()
    {
        var settings = UseCaseSettings.RestaurantBooking();
        var customer = settings.Entities["Customer"];
        customer.Relationships.ShouldNotBeNull();
        customer.Relationships.Count.ShouldBe(1);
        customer.Relationships.ShouldContainKey("Bookings");
    }

    [Fact]
    public Task TodoApp_DatabaseInitializer_ProducesCorrectOutput()
    {
        var settings = UseCaseSettings.TodoApp();
        var template = _templates.First(t => t.Name == "DatabaseInitializer");
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task TodoApp_Program_ProducesCorrectOutput()
    {
        var settings = UseCaseSettings.TodoApp();
        var template = _templates.First(t => t.Name == "Program");
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task TodoApp_CrudWebApiExtensions_ProducesCorrectOutput()
    {
        var settings = UseCaseSettings.TodoApp();
        var template = _templates.First(t => t.Name == "CrudWebApiExtensions");
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task TodoApp_GetByParentEndpoint_ProducesNestedEndpoints()
    {
        var settings = UseCaseSettings.TodoApp();
        var template = _templates.First(t => t.Name == "GetByParentEndpoint");
        var results = template.GenerateMany(settings).Where(r => !r.IsEmpty).ToList();
        return Verify(results.Select(r => new { r.FileName, r.Content }));
    }

    [Fact]
    public Task RestaurantBooking_DatabaseInitializer_ProducesCorrectOutput()
    {
        var settings = UseCaseSettings.RestaurantBooking();
        var template = _templates.First(t => t.Name == "DatabaseInitializer");
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task RestaurantBooking_CrudWebApiExtensions_ProducesCorrectOutput()
    {
        var settings = UseCaseSettings.RestaurantBooking();
        var template = _templates.First(t => t.Name == "CrudWebApiExtensions");
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task TodoApp_DatabaseSeeder_ProducesCorrectOutput()
    {
        var settings = UseCaseSettings.TodoApp();
        var template = _templates.First(t => t.Name == "DatabaseSeeder");
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public Task RestaurantBooking_DatabaseSeeder_ProducesCorrectOutput()
    {
        var settings = UseCaseSettings.RestaurantBooking();
        var template = _templates.First(t => t.Name == "DatabaseSeeder");
        var result = template.GenerateOne(settings);
        return Verify(result.Content);
    }

    [Fact]
    public void TodoApp_TodoCategory_HasCsvSeedData()
    {
        var settings = UseCaseSettings.TodoApp();
        var todoCategory = settings.Entities["TodoCategory"];
        todoCategory.SeedData.ShouldNotBeNull();
        todoCategory.SeedData.Count.ShouldBe(4);
        todoCategory.SeedData[0]["Name"].ShouldBe("Work");
    }

    [Fact]
    public void RestaurantBooking_Table_HasCsvSeedData()
    {
        var settings = UseCaseSettings.RestaurantBooking();
        var table = settings.Entities["Table"];
        table.SeedData.ShouldNotBeNull();
        table.SeedData.Count.ShouldBe(6);
        table.SeedData[0]["TableDetails"].ShouldBe("Window seat for 2");
    }

    [Fact]
    public void RestaurantBooking_IngredientType_HasCsvSeedData()
    {
        var settings = UseCaseSettings.RestaurantBooking();
        var ingredientType = settings.Entities["IngredientType"];
        ingredientType.SeedData.ShouldNotBeNull();
        ingredientType.SeedData.Count.ShouldBe(6);
        ingredientType.SeedData[0]["IngredientTypeDescription"].ShouldBe("Vegetable");
    }

    [Fact]
    public void RestaurantBooking_StaffRole_HasCsvSeedData()
    {
        var settings = UseCaseSettings.RestaurantBooking();
        var staffRole = settings.Entities["StaffRole"];
        staffRole.SeedData.ShouldNotBeNull();
        staffRole.SeedData.Count.ShouldBe(6);
        staffRole.SeedData[0]["StaffRoleDescription"].ShouldBe("Chef");
    }

    [Fact]
    public Task RestaurantBooking_GetByParentEndpoint_ProducesNestedEndpoints()
    {
        var settings = UseCaseSettings.RestaurantBooking();
        var template = _templates.First(t => t.Name == "GetByParentEndpoint");
        var results = template.GenerateMany(settings).Where(r => !r.IsEmpty).ToList();
        return Verify(results.Select(r => new { r.FileName, r.Content }));
    }

    [Fact]
    public void TodoApp_TemplateCount_GeneratesFiles()
    {
        var totalFiles = CountGeneratedFiles(UseCaseSettings.TodoApp());
        totalFiles.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void RestaurantBooking_GeneratesMoreFilesThanTodoApp()
    {
        var todoFiles = CountGeneratedFiles(UseCaseSettings.TodoApp());
        var restaurantFiles = CountGeneratedFiles(UseCaseSettings.RestaurantBooking());

        restaurantFiles.ShouldBeGreaterThan(
            todoFiles,
            "RestaurantBooking has 12 entities vs TodoApp's 3, so it should generate more files");
    }

    private void ValidateAllTemplatesProduceValidOutput(NinjadogSettings settings)
    {
        var errors = new List<string>();

        foreach (var template in _templates)
        {
            try
            {
                var singleResult = template.GenerateOne(settings);
                if (!singleResult.IsEmpty)
                {
                    singleResult.Content.ShouldNotBeNullOrWhiteSpace(
                        $"Template '{template.Name}' GenerateOne produced empty content");
                }

                foreach (var result in template.GenerateMany(settings))
                {
                    if (!result.IsEmpty)
                    {
                        result.Content.ShouldNotBeNullOrWhiteSpace(
                            $"Template '{template.Name}' GenerateMany produced empty content for '{result.FileName}'");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Template '{template.Name}': {ex.Message}");
            }
        }

        errors.ShouldBeEmpty($"Template errors:\n{string.Join("\n", errors)}");
    }

    private int CountGeneratedFiles(NinjadogSettings settings)
    {
        var count = 0;
        foreach (var template in _templates)
        {
            var singleResult = template.GenerateOne(settings);
            if (!singleResult.IsEmpty)
            {
                count++;
            }

            count += template.GenerateMany(settings).Count(r => !r.IsEmpty);
        }

        return count;
    }
}
