using Ninjadog.Settings;
using Ninjadog.Settings.Config;
using Ninjadog.Settings.Entities;
using Ninjadog.Settings.Entities.Properties;

namespace Ninjadog.Evolution;

/// <summary>
/// Compares two <see cref="NinjadogSettings"/> instances and produces a structured <see cref="SchemaDiff"/>.
/// This is a pure function with no I/O or side effects.
/// </summary>
public static class SchemaDiffer
{
    /// <summary>
    /// Compares a previous settings snapshot against the current settings and returns all detected changes.
    /// </summary>
    /// <param name="previous">The previous schema state.</param>
    /// <param name="current">The current schema state.</param>
    /// <returns>A <see cref="SchemaDiff"/> describing all changes.</returns>
    public static SchemaDiff Compare(NinjadogSettings previous, NinjadogSettings current)
    {
        var entityChanges = CompareEntities(previous.Entities, current.Entities);
        var configChanges = CompareConfig(previous.Config, current.Config);
        var enumChanges = CompareEnums(previous.Enums, current.Enums);

        return new SchemaDiff(entityChanges, configChanges, enumChanges);
    }

    private static List<EntityDiff> CompareEntities(
        NinjadogEntities previous,
        NinjadogEntities current)
    {
        var diffs = new List<EntityDiff>();
        var allKeys = previous.Keys.Union(current.Keys).Distinct();

        foreach (var key in allKeys)
        {
            var existsInPrevious = previous.TryGetValue(key, out var prevEntity);
            var existsInCurrent = current.TryGetValue(key, out var currEntity);

            if (existsInPrevious && existsInCurrent)
            {
                var propertyDiffs = CompareProperties(prevEntity!.Properties, currEntity!.Properties);
                var relationshipDiffs = CompareRelationships(prevEntity.Relationships, currEntity.Relationships);

                if (propertyDiffs.Count > 0 || relationshipDiffs.Count > 0)
                {
                    diffs.Add(new EntityDiff(key, ChangeKind.Modified, propertyDiffs, relationshipDiffs));
                }
            }
            else if (existsInCurrent)
            {
                var allProps = currEntity!.Properties
                    .Select(kvp => new PropertyDiff(kvp.Key, ChangeKind.Added, null, kvp.Value))
                    .ToList();

                var allRels = currEntity.Relationships?
                    .Select(kvp => new RelationshipDiff(kvp.Key, ChangeKind.Added, null, kvp.Value))
                    .ToList() ?? [];

                diffs.Add(new EntityDiff(key, ChangeKind.Added, allProps, allRels));
            }
            else
            {
                var allProps = prevEntity!.Properties
                    .Select(kvp => new PropertyDiff(kvp.Key, ChangeKind.Removed, kvp.Value, null))
                    .ToList();

                var allRels = prevEntity.Relationships?
                    .Select(kvp => new RelationshipDiff(kvp.Key, ChangeKind.Removed, kvp.Value, null))
                    .ToList() ?? [];

                diffs.Add(new EntityDiff(key, ChangeKind.Removed, allProps, allRels));
            }
        }

        return diffs;
    }

    private static List<PropertyDiff> CompareProperties(
        NinjadogEntityProperties previous,
        NinjadogEntityProperties current)
    {
        var diffs = new List<PropertyDiff>();
        var allKeys = previous.Keys.Union(current.Keys).Distinct();

        foreach (var key in allKeys)
        {
            var existsInPrevious = previous.TryGetValue(key, out var prevProp);
            var existsInCurrent = current.TryGetValue(key, out var currProp);

            if (existsInPrevious && existsInCurrent)
            {
                if (!PropertiesAreEqual(prevProp!, currProp!))
                {
                    diffs.Add(new PropertyDiff(key, ChangeKind.Modified, prevProp, currProp));
                }
            }
            else if (existsInCurrent)
            {
                diffs.Add(new PropertyDiff(key, ChangeKind.Added, null, currProp));
            }
            else
            {
                diffs.Add(new PropertyDiff(key, ChangeKind.Removed, prevProp, null));
            }
        }

        return diffs;
    }

    private static bool PropertiesAreEqual(NinjadogEntityProperty a, NinjadogEntityProperty b)
    {
        return a.Type == b.Type
            && a.IsKey == b.IsKey
            && a.Required == b.Required
            && a.MaxLength == b.MaxLength
            && a.MinLength == b.MinLength
            && a.Min == b.Min
            && a.Max == b.Max
            && a.Pattern == b.Pattern;
    }

    private static List<RelationshipDiff> CompareRelationships(
        NinjadogEntityRelationships? previous,
        NinjadogEntityRelationships? current)
    {
        var diffs = new List<RelationshipDiff>();

        var prevKeys = previous?.Keys ?? Enumerable.Empty<string>();
        var currKeys = current?.Keys ?? Enumerable.Empty<string>();
        var allKeys = prevKeys.Union(currKeys).Distinct();

        foreach (var key in allKeys)
        {
            var inPrevious = previous is not null && previous.TryGetValue(key, out _);
            var inCurrent = current is not null && current.TryGetValue(key, out _);

            if (inPrevious && inCurrent)
            {
                var prevRel = previous![key];
                var currRel = current![key];

                if (prevRel.RelatedEntity != currRel.RelatedEntity
                    || prevRel.RelationshipType != currRel.RelationshipType)
                {
                    diffs.Add(new RelationshipDiff(key, ChangeKind.Modified, prevRel, currRel));
                }
            }
            else if (inCurrent)
            {
                diffs.Add(new RelationshipDiff(key, ChangeKind.Added, null, current![key]));
            }
            else
            {
                diffs.Add(new RelationshipDiff(key, ChangeKind.Removed, previous![key], null));
            }
        }

        return diffs;
    }

    private static ConfigDiff CompareConfig(NinjadogConfiguration previous, NinjadogConfiguration current)
    {
        var softDeleteChanged = previous.SoftDelete != current.SoftDelete;
        var auditingChanged = previous.Auditing != current.Auditing;
        var dbProviderChanged = !string.Equals(previous.DatabaseProvider, current.DatabaseProvider, StringComparison.OrdinalIgnoreCase);
        var aotChanged = previous.Aot != current.Aot;
        var corsChanged = !NullableRecordEqual(previous.Cors, current.Cors);
        var authChanged = !NullableRecordEqual(previous.Auth, current.Auth);
        var rateLimitChanged = !NullableRecordEqual(previous.RateLimit, current.RateLimit);
        var versioningChanged = !NullableRecordEqual(previous.Versioning, current.Versioning);

        return new ConfigDiff(
            softDeleteChanged,
            softDeleteChanged ? current.SoftDelete : null,
            auditingChanged,
            auditingChanged ? current.Auditing : null,
            dbProviderChanged,
            dbProviderChanged ? previous.DatabaseProvider : null,
            dbProviderChanged ? current.DatabaseProvider : null,
            aotChanged,
            corsChanged,
            authChanged,
            rateLimitChanged,
            versioningChanged);
    }

    private static bool NullableRecordEqual<T>(T? a, T? b)
        where T : class
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    private static List<EnumDiff> CompareEnums(
        Dictionary<string, List<string>>? previous,
        Dictionary<string, List<string>>? current)
    {
        var diffs = new List<EnumDiff>();

        var prevKeys = previous?.Keys ?? Enumerable.Empty<string>();
        var currKeys = current?.Keys ?? Enumerable.Empty<string>();
        var allKeys = prevKeys.Union(currKeys).Distinct();

        foreach (var key in allKeys)
        {
            var inPrevious = previous is not null && previous.ContainsKey(key);
            var inCurrent = current is not null && current.ContainsKey(key);

            if (inPrevious && inCurrent)
            {
                var prevVals = previous![key];
                var currVals = current![key];

                if (!prevVals.SequenceEqual(currVals))
                {
                    diffs.Add(new EnumDiff(key, ChangeKind.Modified, prevVals, currVals));
                }
            }
            else if (inCurrent)
            {
                diffs.Add(new EnumDiff(key, ChangeKind.Added, null, current![key]));
            }
            else
            {
                diffs.Add(new EnumDiff(key, ChangeKind.Removed, previous![key], null));
            }
        }

        return diffs;
    }
}
