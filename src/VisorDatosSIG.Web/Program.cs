using Microsoft.AspNetCore.Authentication.Cookies;
using VisorDatosSIG.Application.Interfaces;
using VisorDatosSIG.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Inyección de dependencias de la solución limpia
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGeoDataService, GeoDataService>();

// Autenticación por Cookies según pliego (RF-SEG-01 / RF-SEG-04)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
