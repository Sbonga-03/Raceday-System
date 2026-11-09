using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

// The form used to add and edit a category
public class CategoryFormViewModel
{
    public int EventId { get; set; }
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Please enter a category name.")]
    [MaxLength(100, ErrorMessage = "The name must be 100 characters or fewer.")]
    [Display(Name = "Category name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter a distance.")]
    [Range(0.01, 1000, ErrorMessage = "Please enter a distance between 0.01 and 1000 km.")]
    [Display(Name = "Distance (km)")]
    public decimal? Distance { get; set; }

    [Required(ErrorMessage = "Please enter an entry fee (0 for free).")]
    [Range(0, 100000, ErrorMessage = "Please enter a fee between 0 and 100000.")]
    [Display(Name = "Entry fee (R)")]
    public decimal? Fee { get; set; }
}

public class ManageCategoriesViewModel
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public List<CategoryInfo> Categories { get; set; } = new();
    public CategoryFormViewModel NewCategory { get; set; } = new();
}