using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SoruDeneme.Models;

namespace SoruDeneme.Controllers
{
    public class HomeController : Controller
    {
        // Login sayfasýný ilk açan metod (GET)
        public IActionResult Index()
        {
            return View();
        }

        // Giriþ butonuna basýldýðýnda çalýþan metod (POST)
        [HttpPost]
        public IActionResult Index(string userType, string Isim, string Sifre)
        {
            // Kullanýcý adý ve þifre kontrolü
            if (Isim == "eðitmen" && Sifre == "eðitmen123")
            {
                if (userType == "Egitmen")
                {
                    return RedirectToAction("EgitmenHome");
                }
            }
            if (Isim == "öðrenci" && Sifre == "öðrenci123") // Öðrenci bilgilerini girdiðini varsayýyorum
            {
                if (userType == "Ogrenci")
                {
                    // BURAYI KONTROL ET: Index deðil, OgrenciHome olmalý
                    return RedirectToAction("OgrenciHome");
                }
            }

            // Eðer bilgiler yanlýþsa sayfayý yenile ve hata mesajý göster (opsiyonel)
            ViewBag.ErrorMessage = "Kullanýcý adý veya þifre hatalý!";
            return View();
        }

        public IActionResult EgitmenHome()
        {
            return View();
        }

        public IActionResult OgrenciHome()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}