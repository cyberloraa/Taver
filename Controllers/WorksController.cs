using Microsoft.AspNetCore.Mvc;
using Taver.Services;

namespace Taver.Controllers;

public class WorksController : Controller
{
    private readonly IStaticSiteData _data;
    private const int PageSize = 12;

    public WorksController(IStaticSiteData data)
    {
        _data = data;
    }

    public IActionResult Index(int page = 1)
    {
        var all = _data.Artworks;
        var total = all.Count;
        var items = all
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        ViewData["TotalCount"] = total;
        ViewData["Page"] = page;
        ViewData["TotalPages"] = total == 0 ? 1 : (int)Math.Ceiling(total / (double)PageSize);
        return View(items);
    }

    public IActionResult Detail(int id)
    {
        var artwork = _data.Artworks.FirstOrDefault(a => a.ArtworkID == id);
        if (artwork == null)
            return NotFound();
        return View(artwork);
    }
}
