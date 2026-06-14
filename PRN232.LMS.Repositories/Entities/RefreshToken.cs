using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232.LMS.Repositories.Entities;

[Table("RefreshToken")]
public class RefreshToken
{
    [Key]
    public int TokenId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Token { get; set; } = null!;

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    public bool IsActive => !IsRevoked && !IsExpired;
}
