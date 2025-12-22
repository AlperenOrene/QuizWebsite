using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SoruDeneme.Data;
using SoruDeneme.Models;
using SoruDeneme.Filters;

namespace SoruDeneme.Controllers
{
    [RequireLogin]
    [RequireRole("Egitmen")]
    public class QuestionsController : Controller
    {
        private readonly SoruDenemeContext _context;

        public QuestionsController(SoruDenemeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var soruDenemeContext = _context.Question.Include(q => q.Quiz);
            return View(await soruDenemeContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var question = await _context.Question
                .Include(q => q.Quiz)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (question == null) return NotFound();
            return View(question);
        }

        public IActionResult Create()
        {
            ViewBag.QuizId = new SelectList(_context.Quiz, "Id", "QuizName");
            return View();
        }

        // ✅ imageFile alıyoruz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Text,QuestionNum,ChoiceA,ChoiceB,ChoiceC,CorrectOption,QuizId")] Question question,
            IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.QuizId = new SelectList(_context.Quiz, "Id", "QuizName", question.QuizId);
                return View(question);
            }

            // ✅ resim varsa kaydet
            if (imageFile != null && imageFile.Length > 0)
            {
                question.ImagePath = await SaveQuestionImageAsync(imageFile);
            }

            _context.Add(question);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var question = await _context.Question.FindAsync(id);
            if (question == null) return NotFound();

            ViewBag.QuizId = new SelectList(_context.Quiz, "Id", "QuizName", question.QuizId);
            return View(question);
        }

        // ✅ imageFile + removeImage alıyoruz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Text,QuestionNum,ChoiceA,ChoiceB,ChoiceC,CorrectOption,QuizId")] Question question,
            IFormFile? imageFile,
            bool removeImage = false)
        {
            if (id != question.Id) return NotFound();

            // DB’den eski kaydı çek (ImagePath'i korumak için)
            var existing = await _context.Question.AsNoTracking().FirstOrDefaultAsync(q => q.Id == id);
            if (existing == null) return NotFound();

            if (!ModelState.IsValid)
            {
                // mevcut resmi view’da göstermek için
                question.ImagePath = existing.ImagePath;
                ViewBag.QuizId = new SelectList(_context.Quiz, "Id", "QuizName", question.QuizId);
                return View(question);
            }

            // ImagePath yönetimi:
            // 1) removeImage true ise sil
            // 2) yeni imageFile geldiyse eskisini silip yenisini kaydet
            // 3) hiçbiri yoksa eskisini koru
            question.ImagePath = existing.ImagePath;

            if (removeImage && !string.IsNullOrWhiteSpace(existing.ImagePath))
            {
                DeletePhysicalFile(existing.ImagePath);
                question.ImagePath = null;
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                // eskiyi sil
                if (!string.IsNullOrWhiteSpace(existing.ImagePath))
                    DeletePhysicalFile(existing.ImagePath);

                // yeniyi kaydet
                question.ImagePath = await SaveQuestionImageAsync(imageFile);
            }

            try
            {
                _context.Update(question);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestionExists(question.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var question = await _context.Question
                .Include(q => q.Quiz)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (question == null) return NotFound();
            return View(question);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var question = await _context.Question.FindAsync(id);
            if (question != null)
            {
                // ✅ resim dosyasını da sil
                if (!string.IsNullOrWhiteSpace(question.ImagePath))
                    DeletePhysicalFile(question.ImagePath);

                _context.Question.Remove(question);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool QuestionExists(int id)
        {
            return _context.Question.Any(e => e.Id == id);
        }

        // ===================== IMAGE HELPERS =====================
        private async Task<string> SaveQuestionImageAsync(IFormFile imageFile)
        {
            // uploads/questions
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "questions");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // uzantı
            var ext = Path.GetExtension(imageFile.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".png";

            // dosya adı
            var fileName = $"q_{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // web path
            return $"/uploads/questions/{fileName}";
        }

        private void DeletePhysicalFile(string webPath)
        {
            // webPath: "/uploads/questions/abc.png"
            var relative = webPath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relative);

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }
    }
}
