# SoruDeneme - Online Quiz Platformu

Bu proje, eğitmenlerin sınav oluşturabileceği ve öğrencilerin bu sınavları çözebileceği ASP.NET Core tabanlı bir web uygulamasıdır. Veritabanı olarak SQLite kullanılmıştır ve kurulum gerektirmez.

## Gereksinimler

Projenin sorunsuz çalışması için aşağıdaki araçların yüklü olması önerilir:
* **Visual Studio 2022** (Güncel sürüm önerilir)
* **.NET SDK** (Proje sürümü ile uyumlu)

## Kurulum ve Çalıştırma Adımları

### 1. Projeyi Açma
Proje dizininde yeni nesil çözüm dosyası formatı olan **`.slnx`** kullanılmıştır.

* **Önerilen Yöntem:** Eğer Visual Studio sürümünüz güncelse (VS 2022 Preview veya son güncellemeler), ana dizindeki `SoruDeneme.slnx` dosyasına çift tıklayarak projeyi açabilirsiniz.
* **Alternatif Yöntem:** Eğer `.slnx` dosyası açılmazsa veya Visual Studio tarafından tanınmazsa, `SoruDeneme` klasörü içindeki **`SoruDeneme.csproj`** dosyasını seçerek projeyi açabilirsiniz.

### 2. Veritabanı Kurulumu (Otomatik)
Proje **SQLite** veritabanı kullanmaktadır ve herhangi bir SQL Server kurulumu veya ayarı **gerektirmez**.

* Uygulama ilk kez çalıştırıldığında, veritabanı dosyası (`app.db`) `App_Data` klasörü altında **otomatik olarak oluşturulacak** ve gerekli tablolar kurulacaktır.
* Ayrıca test kullanıcıları da otomatik olarak veritabanına eklenecektir. Ekstra bir komut (Update-Database vb.) çalıştırmanıza gerek yoktur.

### 3. Uygulamayı Başlatma
* Visual Studio üst menüsündeki "Başlat" (Start) butonuna basarak veya klavyeden `F5` tuşu ile uygulmayı tarayıcıda çalıştırabilirsiniz.

---

## Giriş Bilgileri (Test Kullanıcıları)

Uygulama açıldığında test edebilmeniz için aşağıdaki hazır hesaplar otomatik olarak tanımlanmıştır:

**Eğitmen K.ADI ve ŞİFRE:**  `egitmen` `egitmen123`
**Öğrenci K.ADI ve ŞİFRE:** `ogrenci` `ogrenci123`

---

### Proje Yapısı Hakkında Notlar
* **Yetkilendirme:** Eğitmen ve Öğrenci sayfaları `RequireRole` attribute'ları ile korunmaktadır.
* **Veritabanı Yolu:** `appsettings.json` dosyasında Connection String tanımlıdır ve proje içerisindeki yerel bir dosyayı işaret eder.
