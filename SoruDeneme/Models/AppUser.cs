namespace SoruDeneme.Models
{
    public class AppUser
    {
        public int Id { get; set; }

        public string Username { get; set; } = "";

        // Basit yaklaşım: şifreyi hash’li saklayacağız
        public string PasswordHash { get; set; } = "";

        // "Egitmen" / "Ogrenci"
        public string Role { get; set; } = "";
    }
}
