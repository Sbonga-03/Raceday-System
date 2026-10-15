using System.ComponentModel.DataAnnotations;
using RaceDay.Api.Models;

namespace RaceDay.Api.DTOs;

public class EventRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime EventDate { get; set; }

    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }
}

public class CategoryRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 1000)]
    public decimal Distance { get; set; }

    [Range(0, 100000)]
    public decimal Fee { get; set; }
}

public class CategoryResponse
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public decimal Fee { get; set; }

    public static CategoryResponse From(Category c) => new()
    {
        CategoryId = c.CategoryID,
        EventId = c.EventID,
        Name = c.Name,
        Distance = c.Distance,
        Fee = c.Fee
    };
}

public class EventResponse
{
    public int EventId { get; set; }
    public int OrganiserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }

    public static EventResponse From(Event e) => new()
    {
        EventId = e.EventID,
        OrganiserId = e.OrganiserID,
        Name = e.Name,
        EventDate = e.EventDate,
        Location = e.Location,
        Description = e.Description
    };
}

public class EventDetailResponse : EventResponse
{
    public List<CategoryResponse> Categories { get; set; } = new();
}