using Ninjadog.Settings.Config;
using Ninjadog.Settings.Entities;

namespace Ninjadog.Settings;

/// <summary>
/// A concrete implementation of <see cref="NinjadogSettings"/> used when loading settings from a JSON file.
/// </summary>
public sealed record NinjadogLoadedSettings(
    NinjadogConfiguration Config,
    NinjadogEntities Entities,
    Dictionary<string, List<string>>? Enums = null)
    : NinjadogSettings(Config, Entities, Enums);
