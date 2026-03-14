using System.Text.Json;
using Taver.Models;

namespace Taver.Services;

public class StaticSiteData : IStaticSiteData
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Artist? Artist { get; private set; }
    public IReadOnlyList<Artwork> Artworks { get; private set; } = new List<Artwork>();

    public static async Task<StaticSiteData> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = new StaticSiteData();
        if (!File.Exists(path))
            return data;

        await using var stream = File.OpenRead(path);
        var root = await JsonSerializer.DeserializeAsync<SiteDataRoot>(stream, JsonOptions, cancellationToken);
        if (root == null)
            return data;

        if (root.Artist != null)
        {
            data.Artist = new Artist
            {
                ArtistID = 1,
                UserId = "",
                Name = root.Artist.Name ?? "Artist",
                Bio = root.Artist.Bio,
                ProfileImage = root.Artist.ProfileImage,
                CreatedDate = DateTime.UtcNow
            };

            var list = new List<Artwork>();
            var id = 1;
            foreach (var a in root.Artworks ?? Array.Empty<ArtworkDto>())
            {
                list.Add(new Artwork
                {
                    ArtworkID = id++,
                    Title = a.Title ?? "",
                    Description = a.Description,
                    ImagePath = a.ImagePath ?? "",
                    Category = a.Category,
                    Year = a.Year,
                    CreatedDate = a.CreatedDate ?? DateTime.UtcNow,
                    ArtistID = data.Artist.ArtistID,
                    Artist = data.Artist
                });
            }
            data.Artworks = list.OrderByDescending(x => x.CreatedDate).ToList();
        }

        return data;
    }

    private sealed class SiteDataRoot
    {
        public ArtistDto? Artist { get; set; }
        public ArtworkDto[]? Artworks { get; set; }
    }

    private sealed class ArtistDto
    {
        public string? Name { get; set; }
        public string? Bio { get; set; }
        public string? ProfileImage { get; set; }
    }

    private sealed class ArtworkDto
    {
        public int ArtworkID { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public string? Category { get; set; }
        public int? Year { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
