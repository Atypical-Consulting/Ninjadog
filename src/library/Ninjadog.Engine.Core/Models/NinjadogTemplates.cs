namespace Ninjadog.Engine.Core.Models;

/// <summary>
/// Represents a collection of Ninjadog templates.
/// This class provides functionalities to manage a list of template instances.
/// </summary>
public class NinjadogTemplates : List<NinjadogTemplate>
{
    /// <summary>
    /// Adds multiple templates to the collection, potentially under a specified category.
    /// </summary>
    /// <param name="category">The category under which to group the templates.</param>
    /// <param name="templates">The templates to be added to the collection.</param>
    public void AddTemplates(string category, params NinjadogTemplate[] templates)
    {
        foreach (var template in templates)
        {
            // Set the category for each template
            template.Category = category;

            // Add the template to the collection
            Add(template);
        }
    }
}
