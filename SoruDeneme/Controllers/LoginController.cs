using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // Session işlemleri için gerekli

namespace SoruDeneme.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            // Eğer zaten giriş yapılmışsa tekrar giriş ekranı gösterme (İsteğe bağlı)
            var role = HttpContext.Session.GetString("UserRole");
            if (role == "Egitmen") return RedirectToAction("EgitmenHome", "Home");
            if (role == "Ogrenci") return RedirectToAction("OgrenciHome", "Home");

            return View();
        }

        [HttpPost]
        public IActionResult Login(string userType, string username, string password)
        {
            // 1. Durum: EĞİTMEN girişi
            if (userType == "Egitmen" && username == "eğitmen" && password == "eğitmen123")
            {
                // --- KRİTİK EKLEME: Session'a Kayıt ---
                HttpContext.Session.SetString("UserRole", "Egitmen");

                return RedirectToAction("EgitmenHome", "Home");
            }

            // 2. Durum: ÖĞRENCİ girişi
            else if (userType == "Ogrenci" && username == "öğrenci" && password == "öğrenci123")
            {
                // --- KRİTİK EKLEME: Session'a Kayıt ---
                HttpContext.Session.SetString("UserRole", "Ogrenci");

                return RedirectToAction("OgrenciHome", "Home");
            }

            // 3. Durum: Hatalı Giriş
            else
            {
                ViewBag.Error = "Seçiminizle girdiğiniz bilgiler uyuşmuyor veya hatalı!";
                return View("Index");
            }
        }

        // Çıkış Yapma (Logout) - Navbar'a eklemek istersen diye
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Tüm oturum verilerini temizle
            return RedirectToAction("Index");
        }
    }
}