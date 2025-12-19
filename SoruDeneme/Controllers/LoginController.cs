using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; 

namespace SoruDeneme.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == "Egitmen") return RedirectToAction("EgitmenHome", "Home");
            if (role == "Ogrenci") return RedirectToAction("OgrenciHome", "Home");

            return View();
        }

        [HttpPost]
        public IActionResult Login(string userType, string username, string password)
        {
            if (userType == "Egitmen" && username == "eğitmen" && password == "eğitmen123")
            {
                HttpContext.Session.SetString("UserRole", "Egitmen");

                return RedirectToAction("EgitmenHome", "Home");
            }

            else if (userType == "Ogrenci" && username == "öğrenci" && password == "öğrenci123")
            {
                HttpContext.Session.SetString("UserRole", "Ogrenci");

                return RedirectToAction("OgrenciHome", "Home");
            }

            else
            {
                ViewBag.Error = "Seçiminizle girdiğiniz bilgiler uyuşmuyor veya hatalı!";
                return View("Index");
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); 
            return RedirectToAction("Index");
        }
    }
}