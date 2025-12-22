using Microsoft.EntityFrameworkCore;
using SoruDeneme.Data;
using SoruDeneme.Models;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SoruDenemeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SoruDenemeContext")
        ?? throw new InvalidOperationException("Connection string 'SoruDenemeContext' not found.")));

builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ✅ DB migrate + seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SoruDenemeContext>();

    // Migration kullanıyorsan:
    db.Database.Migrate();

    // Basit hash
    static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    // Seed Users
    if (!db.Users.Any())
    {
        db.Users.AddRange(
            new AppUser { Username = "eğitmen", PasswordHash = HashPassword("eğitmen123"), Role = "Egitmen" },
            new AppUser { Username = "öğrenci", PasswordHash = HashPassword("öğrenci123"), Role = "Ogrenci" }
        );

        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
