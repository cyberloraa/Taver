namespace Taver.Services;

public interface IStaticSiteData
{
    Taver.Models.Artist? Artist { get; }
    IReadOnlyList<Taver.Models.Artwork> Artworks { get; }
}
