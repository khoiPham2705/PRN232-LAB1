using System.Text.Json.Serialization;

namespace PRN232.LMS.Services.ResponseModels;

/// <summary>Generic API response wrapper – never exposes Entity Models directly.</summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public T? Data { get; set; }
    /// <summary>Null on success; populated with validation/business rule messages on failure.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)] // Always emit — even as null — per response contract
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Request processed successfully.") =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}

// ── Nested summary DTOs (used inside detail responses to prevent circular refs) ──

/// <summary>Enrollment summary embedded inside StudentDetailResponse and CourseDetailResponse.</summary>
public class EnrollmentSummaryResponse
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public string? CourseName { get; set; }
    public string? SemesterName { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = null!;
}

/// <summary>Student-enrollment summary embedded inside CourseDetailResponse.</summary>
public class StudentEnrollmentSummaryResponse
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public string? StudentFullName { get; set; }
    public string? StudentEmail { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = null!;
}

/// <summary>Course summary embedded inside SemesterDetailResponse.</summary>
public class CourseSummaryResponse
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = null!;
    public int EnrollmentCount { get; set; }
}

// ── Primary response types ──────────────────────────────────────────────────

/// <summary>
/// Semester response.
/// <para><c>Courses</c> is populated only on GET-by-ID to avoid heavy list payloads.</para>
/// </summary>
public class SemesterResponse
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int CourseCount { get; set; }
    /// <summary>Populated on GET /api/semesters/{id} – null on list endpoints.</summary>
    public List<CourseSummaryResponse>? Courses { get; set; }
}

/// <summary>
/// Course response.
/// <para><c>Enrollments</c> is populated only on GET-by-ID.</para>
/// </summary>
public class CourseResponse
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = null!;
    public int SemesterId { get; set; }
    public string? SemesterName { get; set; }
    public int EnrollmentCount { get; set; }
    /// <summary>Populated on GET /api/courses/{id} – null on list endpoints.</summary>
    public List<StudentEnrollmentSummaryResponse>? Enrollments { get; set; }
}

public class SubjectResponse
{
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = null!;
    public string SubjectName { get; set; } = null!;
    public int Credit { get; set; }
}

/// <summary>
/// Student response.
/// <para><c>Enrollments</c> is populated only on GET-by-ID.</para>
/// </summary>
public class StudentResponse
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public int EnrollmentCount { get; set; }
    /// <summary>Populated on GET /api/students/{id} – null on list endpoints.</summary>
    public List<EnrollmentSummaryResponse>? Enrollments { get; set; }
}

public class EnrollmentResponse
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public string? StudentFullName { get; set; }
    public string? StudentEmail { get; set; }
    public int CourseId { get; set; }
    public string? CourseName { get; set; }
    public int SemesterId { get; set; }
    public string? SemesterName { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = null!;
}
