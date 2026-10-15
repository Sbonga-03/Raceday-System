using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

[Table("User")]
public class User
{
    [Key]
    public int UserID { get; set; }
    public int RoleID { get; set; }
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;
    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Role? Role { get; set; }
}