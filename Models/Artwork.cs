using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Taver.Models;

public class Artwork
{
    public int ArtworkID { get; set; }

    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImagePath { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Category { get; set; }

    public int? Year { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int ArtistID { get; set; }
    [ForeignKey(nameof(ArtistID))]
    public Artist Artist { get; set; } = null!;
}
