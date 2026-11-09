using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

// Matches an event summary as the API sends it back for the dashboard
public class OrganiserEventInfo
{
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? BannerUrl { get; set; }
    public int CategoryCount { get; set; }
    public int TotalEnrolments { get; set; }
    public bool IsUpcoming { get; set; }
}

// The form used to create and edit an event
public class EventFormViewModel
{
    public int EventId { get; set; }

    [Required(ErrorMessage = "Please enter an event name.")]
    [MaxLength(150, ErrorMessage = "The name must be 150 characters or fewer.")]
    [Display(Name = "Event name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose a date.")]
    [DataType(DataType.Date)]
    [Display(Name = "Date")]
    public DateTime EventDate { get; set; } = DateTime.Today.AddDays(30);

    [Required(ErrorMessage = "Please enter a location.")]
    [MaxLength(200, ErrorMessage = "The location must be 200 characters or fewer.")]
    public string Location { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "The description must be 1000 characters or fewer.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Please choose an event type.")]
    [RegularExpression("^(Run|Walk|Cycle)$", ErrorMessage = "The event type must be Run, Walk or Cycle.")]
    [Display(Name = "Event type")]
    public string EventType { get; set; } = "Run";

    [Range(0.1, 1000, ErrorMessage = "Please enter a distance between 0.1 and 1000 km.")]
    [Display(Name = "Distance (km)")]
    public decimal Distance { get; set; }

    [Display(Name = "Banner image")]
    public IFormFile? Banner { get; set; }

    public string? BannerUrl { get; set; }
}