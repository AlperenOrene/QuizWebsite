namespace SoruDeneme.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int QuestionNum { get; set; }
        public string? ChoiceA { get; set; }
        public string? ChoiceB { get; set; }
        public string? ChoiceC { get; set; }
        public string CorrectOption { get; set; }
        public int QuizId { get; set; }
        public Quiz? Quiz { get; set; }

    }
}
