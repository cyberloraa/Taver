using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Taver.Models;
using Taver.Services;

namespace Taver.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IStaticSiteData _data;

    public HomeController(ILogger<HomeController> logger, IStaticSiteData data)
    {
        _logger = logger;
        _data = data;
    }

    public IActionResult Index()
    {
        var artist = _data.Artist;
        var featured = _data.Artworks.Take(6).ToList();
        ViewData["Artist"] = artist;
        ViewData["FeaturedArtworks"] = featured;
        return View();
    }

    public IActionResult About()
    {
        var artist = _data.Artist;
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
