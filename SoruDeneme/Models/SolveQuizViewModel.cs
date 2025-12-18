using SoruDeneme.Models;

public class SolveQuizViewModel
{
    public int QuizId { get; set; }
    public int Order { get; set; }

    public Question? CurrentQuestion { get; set; }

    public bool IsFinished { get; set; }
}