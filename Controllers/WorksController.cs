using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taver.Data;

namespace Taver.Controllers;

public class WorksController : Controller
{
    private readonly ApplicationDbContext _db;
    private const int PageSize = 12;

    public WorksController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        var query = _db.Artworks
            .Include(a => a.Artist)
            .OrderByDescending(a => a.CreatedDate);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync(cancellationToken);

        ViewData["TotalCount"] = total;
        ViewData["Page"] = page;
        ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)PageSize);
        return View(items);
    }

    public async Task<IActionResult> Detail(int id, CancellationToken cancellationToken = default)
    {
        var artwork = await _db.Artworks
            .Include(a => a.Artist)
            .FirstOrDefaultAsync(a => a.ArtworkID == id, cancellationToken);
        if (artwork == null)
            return NotFound();
        return View(artwork);
    }
}
