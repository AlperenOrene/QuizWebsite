using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoruDeneme.Data;
using SoruDeneme.Filters;
using SoruDeneme.Models;
using SoruDeneme.Models.ViewModels;

namespace SoruDeneme.Controllers
{
    [RequireLogin]
    public class QuizsController : Controller
    {
        private readonly SoruDenemeContext _context;
        private readonly IWebHostEnvironment _env;

        private const string KEY_ACTIVE_ATTEMPT_ID = "ActiveAttemptId";
        private const string KEY_ACTIVE_QUIZID = "ActiveAttemptQuizId";

        // Kodlu quiz unlock listesi (session)
        private const string KEY_UNLOCKED_QUIZ_IDS = "UnlockedQuizIds"; // "1,5,9"

        public QuizsController(SoruDenemeContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ===================== INDEX =====================
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (string.IsNullOrEmpty(role) || userId == null)
                return RedirectToAction("Index", "Login");

            if (role == "Egitmen")
            {
                var teacherId = userId.Value;

                var teacherQuizzes = await _context.Quiz
                    .Where(q => q.OwnerTeacherId == teacherId)
                    .OrderByDescending(q => q.Id)
                    .ToListAsync();

                return View(teacherQuizzes);
            }

            // Öğrenci: public + unlock edilmiş kodlu
            var unlockedIds = GetUnlockedQuizIds();

            var studentQuizzes = await _context.Quiz
                .Where(q => q.IsPublic || unlockedIds.Contains(q.Id))
                .OrderByDescending(q => q.Id)
                .ToListAsync();

            return View(studentQuizzes);
        }

        // ===================== STUDENT: UNLOCK BY CODE =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Ogrenci")]
        public async Task<IActionResult> UnlockByCode(string accessCode)
        {
            accessCode = (accessCode ?? "").Trim();

            if (string.IsNullOrWhiteSpace(accessCode))
            {
                TempData["UnlockError"] = "Kod boş olamaz.";
                return RedirectToAction(nameof(Index));
            }

            // Kodlu quiz bul
            var quiz = await _context.Quiz
                .FirstOrDefaultAsync(q => !q.IsPublic && q.AccessCode != null && q.AccessCode == accessCode);

            if (quiz == null)
            {
                TempData["UnlockError"] = "Kod hatalı veya böyle bir kodlu sınav yok.";
                return RedirectToAction(nameof(Index));
            }

            var unlocked = GetUnlockedQuizIds();
            unlocked.Add(quiz.Id);
            SaveUnlockedQuizIds(unlocked);

            TempData["UnlockSuccess"] = $"Kod doğru ✅ \"{quiz.QuizName}\" sınavı açıldı.";
            return RedirectToAction(nameof(Index));
        }

        // ===================== CREATE (GET) - WIZARD =====================
        [HttpGet]
        [RequireRole("Egitmen")]
        public async Task<IActionResult> Create(int? id)
        {
            var teacherId = HttpContext.Session.GetInt32("UserId");
            if (teacherId == null) return RedirectToAction("Index", "Login");

            // Yeni quiz
            if (id == null)
            {
                return View(new CreateQuizViewModel
                {
                    QuizCreated = false,
                    Quiz = new Quiz { IsPublic = true }
                });
            }

            // Var olan quiz wizard
            var quiz = await _context.Quiz
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id.Value);

            if (quiz == null) return NotFound();
            if (quiz.OwnerTeacherId != teacherId.Value) return Forbid();

            return View(new CreateQuizViewModel
            {
                QuizCreated = true,
                Quiz = quiz,
                Questions = quiz.Questions.OrderBy(x => x.QuestionNum).ToList(),
                NewQuestion = new Question { QuizId = quiz.Id }
            });
        }

        // ===================== CREATE (POST) - WIZARD =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Egitmen")]
        public async Task<IActionResult> Create(CreateQuizViewModel model, string step, IFormFile? imageFile)
        {
            var teacherId = HttpContext.Session.GetInt32("UserId");
            if (teacherId == null) return RedirectToAction("Index", "Login");

            step = (step ?? "").Trim();

            // ---------------- step: createQuiz ----------------
            if (step == "createQuiz")
            {
                model.Quiz.OwnerTeacherId = teacherId.Value;

                if (!model.Quiz.IsPublic && string.IsNullOrWhiteSpace(model.Quiz.AccessCode))
                {
                    TempData["WizardError"] = "Kodlu quiz için AccessCode zorunludur.";
                    return RedirectToAction(nameof(Create));
                }

                if (string.IsNullOrWhiteSpace(model.Quiz.QuizName))
                {
                    TempData["WizardError"] = "Sınav adı boş olamaz.";
                    return RedirectToAction(nameof(Create));
                }

                _context.Quiz.Add(model.Quiz);
                await _context.SaveChangesAsync();

                TempData["WizardSuccess"] = "Quiz oluşturuldu ✅ Şimdi soruları ekleyin.";
                return RedirectToAction(nameof(Create), new { id = model.Quiz.Id });
            }

            // ---------------- step: addQuestion ----------------
            if (step == "addQuestion")
            {
                var quizId = model.Quiz?.Id ?? 0;
                if (quizId <= 0)
                {
                    TempData["WizardError"] = "Quiz bulunamadı. Önce quiz oluşturmalısın.";
                    return RedirectToAction(nameof(Create));
                }

                var quiz = await _context.Quiz
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.Id == quizId);

                if (quiz == null) return NotFound();
                if (quiz.OwnerTeacherId != teacherId.Value) return Forbid();

                var q = model.NewQuestion ?? new Question();
                q.QuizId = quiz.Id;

                if (q.QuestionNum <= 0)
                {
                    TempData["WizardError"] = "Soru numarası 1 veya daha büyük olmalı.";
                    return RedirectToAction(nameof(Create), new { id = quiz.Id });
                }

                if (string.IsNullOrWhiteSpace(q.Text))
                {
                    TempData["WizardError"] = "Soru metni boş olamaz.";
                    return RedirectToAction(nameof(Create), new { id = quiz.Id });
                }

                if (quiz.Questions.Any(x => x.QuestionNum == q.QuestionNum))
                {
                    TempData["WizardError"] = $"Bu quizde {q.QuestionNum} numaralı soru zaten var.";
                    return RedirectToAction(nameof(Create), new { id = quiz.Id });
                }

                q.CorrectOption = (q.CorrectOption ?? "").Trim().ToUpperInvariant();
                var validOpts = new[] { "A", "B", "C", "D", "E" };
                if (!validOpts.Contains(q.CorrectOption))
                {
                    TempData["WizardError"] = "Doğru şık A/B/C/D/E olmalı.";
                    return RedirectToAction(nameof(Create), new { id = quiz.Id });
                }

                // resim upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                    var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    if (!allowed.Contains(ext))
                    {
                        TempData["WizardError"] = "Sadece JPG/PNG/WEBP yükleyebilirsin.";
                        return RedirectToAction(nameof(Create), new { id = quiz.Id });
                    }

                    var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsDir))
                        Directory.CreateDirectory(uploadsDir);

                    var fileName = $"{Guid.NewGuid():N}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await imageFile.CopyToAsync(stream);

                    q.ImagePath = "/uploads/" + fileName;
                }

                _context.Question.Add(q);
                await _context.SaveChangesAsync();

                TempData["WizardSuccess"] = "Soru eklendi ✅";
                return RedirectToAction(nameof(Create), new { id = quiz.Id });
            }

            TempData["WizardError"] = "Geçersiz işlem (step).";
            return RedirectToAction(nameof(Create));
        }
        // ===================== DETAILS (GET) =====================
        [HttpGet]
        [RequireRole("Egitmen")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var teacherId = HttpContext.Session.GetInt32("UserId");
            if (teacherId == null) return RedirectToAction("Index", "Login");

            var quiz = await _context.Quiz
                .Include(q => q.OwnerTeacher)
                .FirstOrDefaultAsync(q => q.Id == id.Value);

            if (quiz == null) return NotFound();

            if (quiz.OwnerTeacherId != teacherId.Value) return Forbid();

            return View(quiz);
        }

        // ===================== EDIT (GET) =====================
        [HttpGet]
        [RequireRole("Egitmen")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var teacherId = HttpContext.Session.GetInt32("UserId");
            if (teacherId == null) return RedirectToAction("Index", "Login");

            var quiz = await _context.Quiz.FirstOrDefaultAsync(q => q.Id == id.Value);
            if (quiz == null) return NotFound();

            if (quiz.OwnerTeacherId != teacherId.Value) return Forbid();

            return View(quiz);
        }

        // ===================== EDIT (POST) =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Egitmen")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,QuizName,Genre,TimeLimitMinutes,IsPublic,AccessCode")] Quiz incoming)
        {
            if (id != incoming.Id) return NotFound();

            var teacherId = HttpContext.Session.GetInt32("UserId");
            if (teacherId == null) return RedirectToAction("Index", "Login");

            var quizDb = await _context.Quiz.FirstOrDefaultAsync(q => q.Id == id);
            if (quizDb == null) return NotFound();

            if (quizDb.OwnerTeacherId != teacherId.Value) return Forbid();

            // Kodlu ise AccessCode zorunlu
            if (!incoming.IsPublic && string.IsNullOrWhiteSpace(incoming.AccessCode))
            {
                ModelState.AddModelError(nameof(incoming.AccessCode), "Kodlu quiz için AccessCode zorunludur.");
                return View(incoming);
            }

            quizDb.QuizName = incoming.QuizName;
            quizDb.Genre = incoming.Genre;
            quizDb.TimeLimitMinutes = incoming.TimeLimitMinutes;
            quizDb.IsPublic = incoming.IsPublic;
            quizDb.AccessCode = incoming.AccessCode;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ===================== DELETE QUESTION (WIZARD) =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Egitmen")]
        public async Task<IActionResult> DeleteQuestion(int id, int questionId)
        {
            var teacherId = HttpContext.Session.GetInt32("UserId");
            if (teacherId == null) return RedirectToAction("Index", "Login");

            var quiz = await _context.Quiz.FirstOrDefaultAsync(q => q.Id == id);
            if (quiz == null) return NotFound();
            if (quiz.OwnerTeacherId != teacherId.Value) return Forbid();

            var question = await _context.Question.FirstOrDefaultAsync(q => q.Id == questionId && q.QuizId == id);
            if (question == null)
            {
                TempData["WizardError"] = "Soru bulunamadı.";
                return RedirectToAction(nameof(Create), new { id });
            }

            // resim dosyasını da sil
            if (!string.IsNullOrWhiteSpace(question.ImagePath) && question.ImagePath.StartsWith("/uploads/"))
            {
                var rel = question.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var physical = Path.Combine(_env.WebRootPath, rel);
                if (System.IO.File.Exists(physical))
                {
                    try { System.IO.File.Delete(physical); } catch { }
                }
            }

            _context.Question.Remove(question);
            await _context.SaveChangesAsync();

            TempData["WizardSuccess"] = "Soru silindi ✅";
            return RedirectToAction(nameof(Create), new { id });
        }

        // ===================== DELETE QUIZ (GET) =====================
        [HttpGet]
        [RequireRole("Egitmen")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var teacherId = HttpContext.Session.GetInt32("UserId");
            if (teacherId == null) return RedirectToAction("Index", "Login");

            var quiz = await _context.Quiz.FirstOrDefaultAsync(q => q.Id == id.Value);
            if (quiz == null) return NotFound();

            if (quiz.OwnerTeacherId != teacherId.Value) return Forbid();

            return View(quiz);
        }

        // ===================== DELETE QUIZ (POST) =====================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequireRole("Egitmen")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacherId = HttpContext.Session.GetInt32("UserId");
            if (teacherId == null) return RedirectToAction("Index", "Login");

            var quiz = await _context.Quiz.FirstOrDefaultAsync(q => q.Id == id);
            if (quiz == null) return RedirectToAction(nameof(Index));

            if (quiz.OwnerTeacherId != teacherId.Value) return Forbid();

            // Attempt ids
            var attemptIds = await _context.QuizAttempts
                .Where(a => a.QuizId == id)
                .Select(a => a.Id)
                .ToListAsync();

            // AttemptAnswers + Attempts sil
            if (attemptIds.Count > 0)
            {
                var answers = await _context.AttemptAnswers
                    .Where(x => attemptIds.Contains(x.QuizAttemptId))
                    .ToListAsync();

                if (answers.Count > 0)
                    _context.AttemptAnswers.RemoveRange(answers);

                var attempts = await _context.QuizAttempts
                    .Where(a => a.QuizId == id)
                    .ToListAsync();

                if (attempts.Count > 0)
                    _context.QuizAttempts.RemoveRange(attempts);
            }

            // Questions (+ uploads temizle)
            var questions = await _context.Question
                .Where(q => q.QuizId == id)
                .ToListAsync();

            foreach (var q in questions)
            {
                if (!string.IsNullOrWhiteSpace(q.ImagePath) && q.ImagePath.StartsWith("/uploads/"))
                {
                    var rel = q.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                    var physical = Path.Combine(_env.WebRootPath, rel);
                    if (System.IO.File.Exists(physical))
                    {
                        try { System.IO.File.Delete(physical); } catch { }
                    }
                }
            }

            if (questions.Count > 0)
                _context.Question.RemoveRange(questions);

            _context.Quiz.Remove(quiz);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ===================== SOLVE (GET) =====================
        [HttpGet]
        [RequireRole("Ogrenci")]
        public async Task<IActionResult> Solve(int quizId, int order = 0)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Index", "Login");

            var quizInfo = await _context.Quiz.FirstOrDefaultAsync(q => q.Id == quizId);
            if (quizInfo == null) return NotFound();

            // Kodlu koruması
            if (!quizInfo.IsPublic)
            {
                var unlocked = GetUnlockedQuizIds();
                if (!unlocked.Contains(quizId))
                {
                    TempData["UnlockError"] = "Bu sınav kodlu. Çözmek için önce kod girmelisin.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var activeAttemptId = HttpContext.Session.GetInt32(KEY_ACTIVE_ATTEMPT_ID);
            var activeQuizId = HttpContext.Session.GetInt32(KEY_ACTIVE_QUIZID);

            if (order == 0 || activeAttemptId == null || activeQuizId == null || activeQuizId != quizId)
            {
                var total = await _context.Question.CountAsync(q => q.QuizId == quizId);

                var attempt = new QuizAttempt
                {
                    QuizId = quizId,
                    UserId = userId.Value,
                    StartedAt = DateTime.UtcNow,
                    FinishedAt = null,
                    TotalQuestions = total,
                    CorrectCount = 0
                };

                _context.QuizAttempts.Add(attempt);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetInt32(KEY_ACTIVE_ATTEMPT_ID, attempt.Id);
                HttpContext.Session.SetInt32(KEY_ACTIVE_QUIZID, quizId);

                activeAttemptId = attempt.Id;
            }

            var attemptDb = await _context.QuizAttempts
                .Include(a => a.Answers)
                .FirstOrDefaultAsync(a => a.Id == activeAttemptId.Value);

            if (attemptDb == null) return RedirectToAction(nameof(Index));

            var question = await _context.Question
                .Where(q => q.QuizId == quizId)
                .OrderBy(q => q.QuestionNum)
                .Skip(order)
                .FirstOrDefaultAsync();

            if (question == null)
            {
                await RecalculateAndFinishAttemptAsync(attemptDb.Id);

                attemptDb = await _context.QuizAttempts.FirstAsync(a => a.Id == attemptDb.Id);

                HttpContext.Session.Remove(KEY_ACTIVE_ATTEMPT_ID);
                HttpContext.Session.Remove(KEY_ACTIVE_QUIZID);

                var finishedModel = new SolveQuizViewModel
                {
                    QuizId = quizId,
                    Order = order,
                    CurrentQuestion = null,
                    IsFinished = true,
                    TotalQuestions = attemptDb.TotalQuestions,
                    CorrectCount = attemptDb.CorrectCount,
                    SelectedOption = null
                };

                return View(finishedModel);
            }

            var existing = attemptDb.Answers.FirstOrDefault(x => x.QuestionId == question.Id);

            var model = new SolveQuizViewModel
            {
                QuizId = quizId,
                Order = order,
                CurrentQuestion = question,
                IsFinished = false,
                TotalQuestions = attemptDb.TotalQuestions,
                CorrectCount = attemptDb.CorrectCount,
                SelectedOption = existing?.SelectedOption
            };

            return View(model);
        }

        // ===================== SOLVE (POST) =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Ogrenci")]
        public async Task<IActionResult> Solve(int quizId, int order, string selected)
        {
            var quizInfo = await _context.Quiz.FirstOrDefaultAsync(q => q.Id == quizId);
            if (quizInfo == null) return NotFound();

            if (!quizInfo.IsPublic)
            {
                var unlocked = GetUnlockedQuizIds();
                if (!unlocked.Contains(quizId))
                {
                    TempData["UnlockError"] = "Bu sınav kodlu. Çözmek için önce kod girmelisin.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var attemptId = HttpContext.Session.GetInt32(KEY_ACTIVE_ATTEMPT_ID);
            var activeQuizId = HttpContext.Session.GetInt32(KEY_ACTIVE_QUIZID);

            if (attemptId == null || activeQuizId == null || activeQuizId != quizId)
                return RedirectToAction(nameof(Solve), new { quizId = quizId, order = 0 });

            var question = await _context.Question
                .Where(q => q.QuizId == quizId)
                .OrderBy(q => q.QuestionNum)
                .Skip(order)
                .FirstOrDefaultAsync();

            if (question == null)
                return RedirectToAction(nameof(Solve), new { quizId = quizId, order = order });

            var existing = await _context.AttemptAnswers
                .FirstOrDefaultAsync(a => a.QuizAttemptId == attemptId.Value && a.QuestionId == question.Id);

            if (existing == null)
            {
                _context.AttemptAnswers.Add(new AttemptAnswer
                {
                    QuizAttemptId = attemptId.Value,
                    QuestionId = question.Id,
                    SelectedOption = selected
                });
            }
            else
            {
                existing.SelectedOption = selected;
                _context.AttemptAnswers.Update(existing);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Solve), new { quizId = quizId, order = order + 1 });
        }

        // ===================== SCOREBOARD =====================
        [HttpGet]
        [RequireRole("Egitmen")]
        public async Task<IActionResult> Scoreboard(int quizId)
        {
            var teacherId = HttpContext.Session.GetInt32("UserId");
            if (teacherId == null) return RedirectToAction("Index", "Login");

            var quiz = await _context.Quiz.FirstOrDefaultAsync(q => q.Id == quizId);
            if (quiz == null) return NotFound();
            if (quiz.OwnerTeacherId != teacherId.Value) return Forbid();

            var attempts = await _context.QuizAttempts
                .Include(a => a.User)
                .Where(a => a.QuizId == quizId && a.FinishedAt != null)
                .OrderByDescending(a => a.CorrectCount)
                .ThenBy(a => a.FinishedAt)
                .ToListAsync();

            ViewBag.Quiz = quiz;
            return View(attempts);
        }

        // ===================== HELPERS =====================
        private async Task RecalculateAndFinishAttemptAsync(int attemptId)
        {
            var attempt = await _context.QuizAttempts
                .Include(a => a.Answers)
                .FirstAsync(a => a.Id == attemptId);

            var questions = await _context.Question
                .Where(q => q.QuizId == attempt.QuizId)
                .ToListAsync();

            int correct = 0;
            foreach (var q in questions)
            {
                var ans = attempt.Answers.FirstOrDefault(a => a.QuestionId == q.Id);
                if (ans != null && ans.SelectedOption == q.CorrectOption)
                    correct++;
            }

            attempt.CorrectCount = correct;
            attempt.FinishedAt = DateTime.UtcNow;

            _context.QuizAttempts.Update(attempt);
            await _context.SaveChangesAsync();
        }

        private HashSet<int> GetUnlockedQuizIds()
        {
            var raw = HttpContext.Session.GetString(KEY_UNLOCKED_QUIZ_IDS);
            if (string.IsNullOrWhiteSpace(raw)) return new HashSet<int>();

            var set = new HashSet<int>();
            foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(part, out var id))
                    set.Add(id);
            }
            return set;
        }

        private void SaveUnlockedQuizIds(HashSet<int> ids)
        {
            var raw = string.Join(",", ids.OrderBy(x => x));
            HttpContext.Session.SetString(KEY_UNLOCKED_QUIZ_IDS, raw);
        }
    }
}
