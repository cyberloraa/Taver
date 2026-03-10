using System.ComponentModel.DataAnnotations;

namespace Taver.Models;

public class Artist
{
    public int ArtistID { get; set; }

    [Required]
    [MaxLength(256)]
    public string UserId { get; set; } = string.Empty; // FK to AspNetUsers

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Bio { get; set; }

    [MaxLength(500)]
    public string? ProfileImage { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<Artwork> Artworks { get; set; } = new List<Artwork>();
}
