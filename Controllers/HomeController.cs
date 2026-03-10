using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taver.Data;
using Taver.Models;

namespace Taver.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _db;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var artist = await _db.Artists.Include(a => a.Artworks).FirstOrDefaultAsync(cancellationToken);
        var featured = artist?.Artworks
            .OrderByDescending(a => a.CreatedDate)
            .Take(6)
            .ToList() ?? new List<Artwork>();
        ViewData["Artist"] = artist;
        ViewData["FeaturedArtworks"] = featured;
        return View();
    }

    public async Task<IActionResult> About(CancellationToken cancellationToken = default)
    {
        var artist = await _db.Artists.FirstOrDefaultAsync(cancellationToken);
        if (artist == null)
            return NotFound();
        return View(artist);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
