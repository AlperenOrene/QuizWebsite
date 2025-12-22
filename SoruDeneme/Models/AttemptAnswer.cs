namespace SoruDeneme.Models
{
    public class AttemptAnswer
    {
        public int Id { get; set; }

        public int QuizAttemptId { get; set; }
        public QuizAttempt? QuizAttempt { get; set; }

        public int QuestionId { get; set; }
        public Question? Question { get; set; }

        // "A" / "B" / "C"
        public string SelectedOption { get; set; } = "";
    }
}
