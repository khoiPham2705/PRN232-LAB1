using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232.LMS.Repositories.Entities;

[Table("User")]
public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = null!; // Admin, Student, etc.
}
