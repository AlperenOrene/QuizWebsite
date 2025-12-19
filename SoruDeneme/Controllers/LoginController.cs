using Microsoft.AspNetCore.Mvc;

namespace SoruDeneme.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string userType, string username, string password)
        {
            // Kullanıcı doğrulama işlemleri burada (Veritabanı kontrolü vb.)

            // Eğer doğrulama başarılıysa:
            if (userType == "Egitmen")
            {
                return RedirectToAction("EgitmenHome", "Home");
            }
            else if (userType == "Ogrenci")
            {
                return RedirectToAction("OgrenciHome", "Home");
            }

            ViewBag.Error = "İsim veya şifre hatalı";
            return View();
        }
    }
}
