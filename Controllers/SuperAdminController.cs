using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Taver.Models;

namespace Taver.Controllers;

[Authorize(Roles = "SuperAdmin")]
public class SuperAdminController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;

    public SuperAdminController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var users = _userManager.Users.OrderBy(u => u.Email).ToList();
        var list = new List<UserWithRolesViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            list.Add(new UserWithRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? user.UserName ?? user.Id,
                RolesDisplay = roles.Count > 0 ? string.Join(", ", roles) : "—"
            });
        }
        return View(list);
    }

    [HttpGet]
    public IActionResult CreateUser()
    {
        return View(new CreateUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true
        };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
            return RedirectToAction(nameof(Index));

        foreach (var err in result.Errors)
            ModelState.AddModelError(string.Empty, err.Description);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ChangePassword(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();
        return View(new ChangePasswordViewModel
        {
            UserId = user.Id,
            UserEmail = user.Email ?? user.UserName ?? id
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            model.UserEmail = user.Email ?? user.UserName ?? model.UserId;
            return View(model);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
        if (result.Succeeded)
            return RedirectToAction(nameof(Index));

        foreach (var err in result.Errors)
            ModelState.AddModelError(string.Empty, err.Description);
        model.UserEmail = user.Email ?? user.UserName ?? model.UserId;
        return View(model);
    }
}
