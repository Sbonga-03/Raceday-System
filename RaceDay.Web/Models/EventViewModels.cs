namespace RaceDay.Web.Models;

// Matches the event details the API sends back
public class EventSummary
{
    public int EventId { get; set; }
    public int OrganiserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string EventType { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public string? BannerUrl { get; set; }
}

public class CategoryInfo
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public decimal Fee { get; set; }
}

public class EventDetail : EventSummary
{
    public List<CategoryInfo> Categories { get; set; } = new();
}

public class EventsIndexViewModel
{
    public List<EventSummary> Events { get; set; } = new();
    public string? Location { get; set; }
    public DateTime? Date { get; set; }
    public string? Error { get; set; }
}