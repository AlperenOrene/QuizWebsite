using SoruDeneme.Models;

public class SolveQuizViewModel
{
    public int QuizId { get; set; }
    public int Order { get; set; }

    public Question? CurrentQuestion { get; set; }
    public bool IsFinished { get; set; }

    public int TotalQuestions { get; set; }
    public int CorrectCount { get; set; }

    // ✅ Geri gelince hangi seçenek seçilmişti göstermek için
    public string? SelectedOption { get; set; }
}
