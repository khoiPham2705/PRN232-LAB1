using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232.LMS.Models.Entities;

[Table("Course")]
public class Course
{
    [Key]
    public int CourseId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CourseName { get; set; } = null!;

    public int SemesterId { get; set; }

    // Navigation
    [ForeignKey(nameof(SemesterId))]
    public Semester Semester { get; set; } = null!;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
