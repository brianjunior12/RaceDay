using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using raceday_mvc.Services;

//same number and date formats when running locally and inside docker
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    //every post form must carry an anti-forgery token
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddHttpContextAccessor();

//session management keeps the user's logged in state and role on every page
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//the only way this project gets data: calling the Part 2 API
builder.Services.AddHttpClient<api_client>((services, client) =>
{
    var settings = services.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(settings["ApiSettings:BaseUrl"] ?? "http://localhost:5080");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseCookies = false,        //the session cookie of the API is kept in the mvc session instead
    AllowAutoRedirect = false
});

var app = builder.Build();

//friendly error pages, the user never sees a raw error
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
