using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.Models.RequestModels;

public class SemesterRequest
{
    [Required]
    [MaxLength(100)]
    public string SemesterName { get; set; } = null!;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}

public class CourseRequest
{
    [Required]
    [MaxLength(100)]
    public string CourseName { get; set; } = null!;

    [Required]
    public int SemesterId { get; set; }
}

public class SubjectRequest
{
    [Required]
    [MaxLength(20)]
    public string SubjectCode { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string SubjectName { get; set; } = null!;

    [Range(1, 10)]
    public int Credit { get; set; }
}

public class StudentRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = null!;

    [Required]
    public DateTime DateOfBirth { get; set; }
}

public class EnrollmentRequest
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CourseId { get; set; }

    [Required]
    public DateTime EnrollDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = null!;
}
