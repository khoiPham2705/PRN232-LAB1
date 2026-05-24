namespace PRN232.LMS.Models.BusinessModels;

public class SemesterBM
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int CourseCount { get; set; }
}

public class CourseBM
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = null!;
    public int SemesterId { get; set; }
    public string? SemesterName { get; set; }
    public int EnrollmentCount { get; set; }
}

public class SubjectBM
{
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = null!;
    public string SubjectName { get; set; } = null!;
    public int Credit { get; set; }
}

public class StudentBM
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public int EnrollmentCount { get; set; }
}

public class EnrollmentBM
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public string? StudentFullName { get; set; }
    public int CourseId { get; set; }
    public string? CourseName { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = null!;
}
