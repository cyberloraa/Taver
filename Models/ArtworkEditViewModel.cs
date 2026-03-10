using System.ComponentModel.DataAnnotations;

namespace Taver.Models;

public class ArtworkEditViewModel
{
    public int? ArtworkID { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [MaxLength(300)]
    [Display(Name = "Artwork Title")]
    public string? Title { get; set; }

    [MaxLength(2000)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [MaxLength(100)]
    [Display(Name = "Category")]
    public string? Category { get; set; }

    [Display(Name = "Year")]
    [Range(1900, 2100)]
    public int? Year { get; set; }

    /// <summary>Current image path for display on edit (e.g. images/artworks/xxx.jpg).</summary>
    public string? CurrentImagePath { get; set; }
}
