using Taver.Services;

var builder = WebApplication.CreateBuilder(args);

var contentRoot = builder.Environment.ContentRootPath;
var dataPath = Path.Combine(contentRoot, "Data", "site-data.json");
var staticData = await StaticSiteData.LoadAsync(dataPath);
builder.Services.AddSingleton<IStaticSiteData>(staticData);

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
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
    name: "work-detail",
    pattern: "work/{id:int}",
    defaults: new { controller = "Works", action = "Detail" });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
