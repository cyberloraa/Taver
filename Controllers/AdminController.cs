using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taver.Data;
using Taver.Models;

namespace Taver.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IWebHostEnvironment _env;

    public AdminController(
        ApplicationDbContext db,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _signInManager = signInManager;
        _env = env;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl ?? Url.Content("~/admin/dashboard");
        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        returnUrl ??= Url.Content("~/admin/dashboard");
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
            return LocalRedirect(returnUrl);

        ModelState.AddModelError(string.Empty, "Invalid username or password.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken = default)
    {
        var artist = await GetCurrentArtistAsync(cancellationToken);
        if (artist == null)
            return RedirectToAction(nameof(Login));

        var artworks = await _db.Artworks
            .Where(a => a.ArtistID == artist.ArtistID)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync(cancellationToken);

        ViewData["TotalCount"] = artworks.Count;
        return View(artworks);
    }

    [HttpGet]
    public async Task<IActionResult> AddWork(CancellationToken cancellationToken = default)
    {
        if (await GetCurrentArtistAsync(cancellationToken) == null)
            return RedirectToAction(nameof(Login));
        return View(new ArtworkEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddWork(ArtworkEditViewModel model, IFormFile? imageFile, CancellationToken cancellationToken = default)
    {
        var artist = await GetCurrentArtistAsync(cancellationToken);
        if (artist == null)
            return RedirectToAction(nameof(Login));

        if (imageFile == null || imageFile.Length == 0)
        {
            ModelState.AddModelError("", "Please upload an image.");
            return View(model);
        }

        var imagePath = await SaveArtworkImageAsync(imageFile, cancellationToken);
        if (imagePath == null)
        {
            ModelState.AddModelError("", "Invalid image. Allowed: jpg, jpeg, png, webp. Max 5 MB.");
            return View(model);
        }

        var artwork = new Artwork
        {
            Title = model.Title!,
            Description = model.Description,
            ImagePath = imagePath,
            Category = model.Category,
            Year = model.Year,
            ArtistID = artist.ArtistID
        };
        _db.Artworks.Add(artwork);
        await _db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public async Task<IActionResult> EditWork(int id, CancellationToken cancellationToken = default)
    {
        var artist = await GetCurrentArtistAsync(cancellationToken);
        if (artist == null)
            return RedirectToAction(nameof(Login));

        var artwork = await _db.Artworks.FirstOrDefaultAsync(a => a.ArtworkID == id && a.ArtistID == artist.ArtistID, cancellationToken);
        if (artwork == null)
            return NotFound();

        var model = new ArtworkEditViewModel
        {
            ArtworkID = artwork.ArtworkID,
            Title = artwork.Title,
            Description = artwork.Description,
            Category = artwork.Category,
            Year = artwork.Year,
            CurrentImagePath = artwork.ImagePath
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditWork(int id, ArtworkEditViewModel model, IFormFile? imageFile, CancellationToken cancellationToken = default)
    {
        var artist = await GetCurrentArtistAsync(cancellationToken);
        if (artist == null)
            return RedirectToAction(nameof(Login));

        var artwork = await _db.Artworks.FirstOrDefaultAsync(a => a.ArtworkID == id && a.ArtistID == artist.ArtistID, cancellationToken);
        if (artwork == null)
            return NotFound();

        artwork.Title = model.Title!;
        artwork.Description = model.Description;
        artwork.Category = model.Category;
        artwork.Year = model.Year;

        if (imageFile != null && imageFile.Length > 0)
        {
            var imagePath = await SaveArtworkImageAsync(imageFile, cancellationToken);
            if (imagePath != null)
            {
                DeleteArtworkImageIfExists(artwork.ImagePath);
                artwork.ImagePath = imagePath;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var artist = await GetCurrentArtistAsync(cancellationToken);
        if (artist == null)
            return RedirectToAction(nameof(Login));

        var artwork = await _db.Artworks.FirstOrDefaultAsync(a => a.ArtworkID == id && a.ArtistID == artist.ArtistID, cancellationToken);
        if (artwork == null)
            return NotFound();

        DeleteArtworkImageIfExists(artwork.ImagePath);
        _db.Artworks.Remove(artwork);
        await _db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Dashboard));
    }

    private async Task<Artist?> GetCurrentArtistAsync(CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return null;
        return await _db.Artists.FirstOrDefaultAsync(a => a.UserId == user.Id, cancellationToken);
    }

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private async Task<string?> SaveArtworkImageAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            return null;
        if (file.Length > MaxFileSizeBytes || file.Length == 0)
            return null;

        var artworksPath = Path.Combine(_env.WebRootPath, "images", "artworks");
        Directory.CreateDirectory(artworksPath);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(artworksPath, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(stream, cancellationToken);

        return $"images/artworks/{fileName}";
    }

    private void DeleteArtworkImageIfExists(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return;
        var fullPath = Path.Combine(_env.WebRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(fullPath))
            try { System.IO.File.Delete(fullPath); } catch { /* ignore */ }
    }
}
