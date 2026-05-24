using System.Dynamic;

namespace PRN232.LMS.Services.Helpers;

/// <summary>Service-layer helper for field selection and sort-string parsing.</summary>
public static class QueryHelper
{
    /// <summary>
    /// Applies field selection to a collection of DTOs.
    /// Returns the original collection when <paramref name="fields"/> is null/empty,
    /// or a collection of ExpandoObjects containing only the requested fields.
    /// </summary>
    public static IEnumerable<object> ApplyFieldSelection<T>(IEnumerable<T> items, string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
            return items.Cast<object>();

        var requested = fields
            .Split(',')
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var props = typeof(T).GetProperties()
            .Where(p => requested.Contains(p.Name))
            .ToList();

        return items.Select(item =>
        {
            IDictionary<string, object?> expando = new ExpandoObject();
            foreach (var prop in props)
                expando[prop.Name] = prop.GetValue(item);
            return (object)(ExpandoObject)expando;
        });
    }

    /// <summary>
    /// Parses a sort string like "fullName,-dateOfBirth" into a list of (Field, Descending) tuples.
    /// The field name is PascalCased for matching entity properties.
    /// </summary>
    public static IEnumerable<(string Field, bool Descending)> ParseSort(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            yield break;

        foreach (var part in sort.Split(',').Select(s => s.Trim()))
        {
            if (string.IsNullOrEmpty(part)) continue;
            var desc  = part.StartsWith('-');
            var field = desc ? part[1..] : part;
            yield return (ToPascalCase(field), desc);
        }
    }

    private static string ToPascalCase(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];
}
