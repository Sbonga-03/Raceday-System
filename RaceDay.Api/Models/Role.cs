using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

[Table("Role")]
public class Role
{
    [Key]
    public int RoleID { get; set; }
    [Required, MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;
}