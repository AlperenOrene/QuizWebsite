using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SoruDeneme.Models;

namespace SoruDeneme.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string userType, string Isim, string Sifre)
        {
            if (Isim == "eðitmen" && Sifre == "eðitmen123")
            {
                if (userType == "Egitmen")
                {
                    return RedirectToAction("EgitmenHome");
                }
            }
            if (Isim == "öðrenci" && Sifre == "öðrenci123") 
            {
                if (userType == "Ogrenci")
                {
                    return RedirectToAction("OgrenciHome");
                }
            }

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