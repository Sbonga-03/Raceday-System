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