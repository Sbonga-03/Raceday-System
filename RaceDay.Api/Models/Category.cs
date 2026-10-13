using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

[Table("Category")]
public class Category
{
    [Key]
    public int CategoryID { get; set; }
    public int EventID { get; set; }
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [Column(TypeName = "decimal(6,2)")]
    public decimal Distance { get; set; }
    [Column(TypeName = "decimal(8,2)")]
    public decimal Fee { get; set; }

    public Event? Event { get; set; }
}