using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace SoruDeneme.Models
{
    public class Quiz
    {
        public int Id { get; set; }

        [Required]
        public string QuizName { get; set; } = "";

        public string? Genre { get; set; }

        public int? TimeLimitMinutes { get; set; }

        public bool IsPublic { get; set; } = true;

        public string? AccessCode { get; set; }

        public int? OwnerTeacherId { get; set; }

        [ValidateNever]
        public AppUser? OwnerTeacher { get; set; }

        [ValidateNever]
        public List<Question> Questions { get; set; } = new();
    }
}
