namespace Backend.Models;

public class Quiz
{
    public int Id { get; set; }
    public string Topic { get; set; } = string.Empty;
    public int NumberOfQuestions { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Question> Questions { get; set; } = new();
    public List<QuizResult> Results { get; set; } = new();
}

public class Question
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
    public string QuestionText { get; set; } = string.Empty;
    public List<Answer> Answers { get; set; } = new();
    public int CorrectAnswerId { get; set; }
}

public class Answer
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public string AnswerText { get; set; } = string.Empty;
}

public class QuizResult
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
    public string PlayerName { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime CompletedAt { get; set; }
}

// DTOs for API responses
public class QuizRequest
{
    public string Topic { get; set; } = string.Empty;
    public int NumberOfQuestions { get; set; }
}

public class QuizResponse
{
    public int Id { get; set; }
    public string Topic { get; set; } = string.Empty;
    public int NumberOfQuestions { get; set; }
    public List<QuestionResponse> Questions { get; set; } = new();
}

public class QuestionResponse
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public List<AnswerResponse> Answers { get; set; } = new();
}

public class AnswerResponse
{
    public int Id { get; set; }
    public string AnswerText { get; set; } = string.Empty;
}

public class QuizSubmission
{
    public int QuizId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public List<AnswerSubmission> Answers { get; set; } = new();
}

public class AnswerSubmission
{
    public int QuestionId { get; set; }
    public int SelectedAnswerId { get; set; }
}

public class QuizResultResponse
{
    public int Id { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public double Percentage { get; set; }
    public DateTime CompletedAt { get; set; }
} 