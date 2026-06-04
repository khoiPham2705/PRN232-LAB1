using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.Services.RequestModels;

/// <summary>Universal query parameters for all list endpoints.</summary>
public class QueryParams
{
    /// <summary>Keyword filter (searches resource-specific text fields).</summary>
    public string? Search { get; set; }

    /// <summary>Comma-separated sort fields. Prefix with '-' for descending. E.g. "fullName,-dateOfBirth"</summary>
    public string? Sort { get; set; }

    private int _page = 1;
    /// <summary>1-based page number.</summary>
    public int Page { get => _page; set => _page = value < 1 ? 1 : value; }

    private int _size = 10;
    /// <summary>Items per page (1-100).</summary>
    public int Size { get => _size; set => _size = (value < 1 || value > 100) ? 10 : value; }

    /// <summary>Comma-separated field names to include in response. E.g. "studentId,fullName,email"</summary>
    public string? Fields { get; set; }

    /// <summary>Comma-separated related resources to expand. E.g. "student,course"</summary>
    public string? Expand { get; set; }

    public bool HasExpand(string name) =>
        Expand?.Split(',').Any(e => e.Trim().Equals(name, StringComparison.OrdinalIgnoreCase)) == true;
}
