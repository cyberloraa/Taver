using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Taver.Data;
using Taver.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.Cookie.SameSite = SameSiteMode.Lax;
    // In development, allow cookies on same-origin HTTP (avoids HTTPS/HTTP schema issues on localhost)
    if (builder.Environment.IsDevelopment())
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed default artist, super admin role and super admin user
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    const string superAdminRole = "SuperAdmin";
    if (!await roleManager.RoleExistsAsync(superAdminRole))
        await roleManager.CreateAsync(new IdentityRole(superAdminRole));

    if (!db.Artists.Any())
    {
        var user = new IdentityUser
        {
            UserName = "artist@example.com",
            Email = "artist@example.com",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, "Artist123!");
        if (result.Succeeded)
        {
            await db.Artists.AddAsync(new Artist
            {
                UserId = user.Id,
                Name = "Artist",
                Bio = "Welcome to my portfolio. I create drawings and illustrations."
            });
            await db.SaveChangesAsync();
        }
    }

    var superAdminEmail = "superadmin@example.com";
    if (await userManager.FindByEmailAsync(superAdminEmail) == null)
    {
        var superUser = new IdentityUser
        {
            UserName = superAdminEmail,
            Email = superAdminEmail,
            EmailConfirmed = true
        };
        if ((await userManager.CreateAsync(superUser, "SuperAdmin123!")).Succeeded)
            await userManager.AddToRoleAsync(superUser, superAdminRole);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "login",
    pattern: "login",
    defaults: new { controller = "Admin", action = "Login" });
app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{action=Dashboard}/{id?}",
    defaults: new { controller = "Admin" });
app.MapControllerRoute(
    name: "superadmin",
    pattern: "superadmin/{action=Index}/{id?}",
    defaults: new { controller = "SuperAdmin" });
app.MapControllerRoute(
    name: "work-detail",
    pattern: "work/{id:int}",
    defaults: new { controller = "Works", action = "Detail" });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
