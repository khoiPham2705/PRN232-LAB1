using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232.LMS.Repositories.Entities;

[Table("Subject")]
public class Subject
{
    [Key]
    public int SubjectId { get; set; }

    [Required]
    [MaxLength(20)]
    public string SubjectCode { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string SubjectName { get; set; } = null!;

    public int Credit { get; set; }
}
