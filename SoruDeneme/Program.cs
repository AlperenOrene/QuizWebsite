using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoruDeneme.Data;

var builder = WebApplication.CreateBuilder(args);

// Veritabanı bağlantısı
builder.Services.AddDbContext<SoruDenemeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SoruDenemeContext") ?? throw new InvalidOperationException("Connection string 'SoruDenemeContext' not found.")));

// Controller ve View servisleri
builder.Services.AddControllersWithViews();

// --- 1. SESSION SERVİSİNİ EKLEME (Ayarlı Halde) ---
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // 60 dk işlem yapmazsa oturum düşer
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Hata yönetimi ve HSTS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// --- 2. SESSION MİDDLEWARE (UseRouting'den SONRA olmalı) ---
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();