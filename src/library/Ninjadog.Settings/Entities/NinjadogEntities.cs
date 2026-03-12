namespace Ninjadog.Settings.Entities;

/// <summary>
/// Represents a collection of Ninjadog entities.
/// This abstract class serves as a dictionary mapping entity names to their corresponding Ninjadog entity definitions.
/// It can be used to define and manage the various entities involved in the templating and code generation process.
/// </summary>
public abstract class NinjadogEntities
    : Dictionary<string, NinjadogEntity>;
