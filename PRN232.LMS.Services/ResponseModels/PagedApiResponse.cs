using System.Text.Json.Serialization;

namespace PRN232.LMS.Services.ResponseModels;

public class PaginationMetadata
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }

    public static PaginationMetadata Create(int page, int size, int total) => new()
    {
        Page       = page,
        PageSize   = size,
        TotalItems = total,
        TotalPages = (int)Math.Ceiling(total / (double)size)
    };
}

/// <summary>Paged response used by all list endpoints. Data may be full DTOs or field-selected ExpandoObjects.</summary>
public class PagedApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public PaginationMetadata? Pagination { get; set; }
    /// <summary>Null on success; populated on failure.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)] // Always emit — even as null — per response contract
    public List<string>? Errors { get; set; }

    public static PagedApiResponse Ok(object data, PaginationMetadata pagination,
        string message = "Request processed successfully.") =>
        new() { Success = true, Data = data, Pagination = pagination, Message = message };

    public static PagedApiResponse Fail(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}
