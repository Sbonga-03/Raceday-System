using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

[Table("Event")]
public class Event
{
    [Key]
    public int EventID { get; set; }
    public int OrganiserID { get; set; }
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    [Column(TypeName = "date")]
    public DateTime EventDate { get; set; }
    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string? Description { get; set; }

    public User? Organiser { get; set; }
    public List<Category> Categories { get; set; } = new();
}