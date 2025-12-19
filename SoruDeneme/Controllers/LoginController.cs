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
            // 1. Durum: EĞİTMEN seçili VE bilgiler doğru mu?
            if (userType == "Egitmen" && username == "eğitmen" && password == "eğitmen123")
            {
                return RedirectToAction("EgitmenHome", "Home");
            }

            // 2. Durum: ÖĞRENCİ seçili VE bilgiler doğru mu?
            else if (userType == "Ogrenci" && username == "öğrenci" && password == "öğrenci123")
            {
                return RedirectToAction("OgrenciHome", "Home");
            }

            // 3. Durum: Herhangi bir uyumsuzluk veya hata
            else
            {
                // Kullanıcıya daha detaylı bilgi verebiliriz veya genel hata mesajı dönebiliriz.
                ViewBag.Error = "Seçiminizle girdiğiniz bilgiler uyuşmuyor veya hatalı!";
                return View("Index");
            }
        }
    }
}