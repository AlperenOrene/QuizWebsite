using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SoruDeneme.Data;
using SoruDeneme.Models;

namespace SoruDeneme.Controllers
{
    public class QuizsController : Controller
    {
        private readonly SoruDenemeContext _context;

        public QuizsController(SoruDenemeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Quiz.ToListAsync());
        }

        // GET: Quizs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quiz
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        // GET: Quizs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Quizs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,QuizName,Genre")] Quiz quiz)
        {
            if (ModelState.IsValid)
            {
                _context.Add(quiz);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(quiz);
        }

        // GET: Quizs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quiz.FindAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }
            return View(quiz);
        }

        // POST: Quizs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,QuizName,Genre")] Quiz quiz)
        {
            if (id != quiz.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(quiz);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuizExists(quiz.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(quiz);
        }

        // GET: Quizs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quiz
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        // POST: Quizs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quiz = await _context.Quiz.FindAsync(id);
            if (quiz != null)
            {
                _context.Quiz.Remove(quiz);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Solve(int id, int order = 0, string selected = null)
        {
            string result = null;
            int quizId = id;
            if (selected != null)
            {
                var prevQuestion = await _context.Question
                    .Where(q => q.QuizId == quizId)
                    .OrderBy(q => q.QuestionNum)
                    .Skip(order - 1)
                    .FirstOrDefaultAsync();

                if (prevQuestion != null)
                    result = selected == prevQuestion.CorrectOption ? "Doğru" : "Yanlış";
            }

            var question = await _context.Question
                .Where(q => q.QuizId == quizId)
                .OrderBy(q => q.QuestionNum)
                .Skip(order)
                .FirstOrDefaultAsync();

            var model = new SolveQuizViewModel
            {
                QuizId = quizId,
                Order = order,
                CurrentQuestion = question,
                IsFinished = question == null
            };

            return View(model);
        }
        public async Task<IActionResult> GetQuestion(int quizId, int order)
        {
            var question = await _context.Question
                .Where(q => q.QuizId == quizId)
                .OrderBy(q => q.QuestionNum)
                .Skip(order)
                .FirstOrDefaultAsync();

            if (question == null)
            {
                return Json(new { finished = true });
            }

            return Json(new
            {
                finished = false,
                id = question.Id,
                text = question.Text,
                a = question.ChoiceA,
                b = question.ChoiceB,
                c = question.ChoiceC,
                correct = question.CorrectOption
            });
        }
        [HttpPost]
        public IActionResult CheckAnswer(int questionId, string answer)
        {
            var question = _context.Question.Find(questionId);

            bool correct = question.CorrectOption == answer;

            return Json(correct);
        }

        private bool QuizExists(int id)
        {
            return _context.Quiz.Any(e => e.Id == id);
        }
    }
}
