using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

public class ProfileViewModel
{
    [Required(ErrorMessage = "Please enter your full name.")]
    [MaxLength(100, ErrorMessage = "Your name must be 100 characters or fewer.")]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
}

// Matches what the API sends back after a picture upload
public class PictureResult
{
    public string? ProfilePictureUrl { get; set; }
}