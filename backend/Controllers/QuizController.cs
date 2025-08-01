using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Services;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly BackendContext _context;
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<QuizController> _logger;

    public QuizController(BackendContext context, IOpenAIService openAIService, ILogger<QuizController> logger)
    {
        _context = context;
        _openAIService = openAIService;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<QuizResponse>> GenerateQuiz([FromBody] QuizRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Topic))
            {
                return BadRequest("Topic is required");
            }

            if (request.NumberOfQuestions < 1 || request.NumberOfQuestions > 20)
            {
                return BadRequest("Number of questions must be between 1 and 20");
            }

            // Generate quiz using OpenAI
            var quiz = await _openAIService.GenerateQuizAsync(request.Topic, request.NumberOfQuestions);

            // Save to database
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            // Return response without correct answers
            var response = new QuizResponse
            {
                Id = quiz.Id,
                Topic = quiz.Topic,
                NumberOfQuestions = quiz.NumberOfQuestions,
                Questions = quiz.Questions.Select(q => new QuestionResponse
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Answers = q.Answers.Select(a => new AnswerResponse
                    {
                        Id = a.Id,
                        AnswerText = a.AnswerText
                    }).ToList()
                }).ToList()
            };

            return Ok(response);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "OpenAI API error generating quiz");
            return StatusCode(500, new { error = ex.Message, details = ex.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating quiz");
            return StatusCode(500, new { error = ex.Message, details = ex.ToString() });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QuizResponse>> GetQuiz(int id)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null)
        {
            return NotFound();
        }

        var response = new QuizResponse
        {
            Id = quiz.Id,
            Topic = quiz.Topic,
            NumberOfQuestions = quiz.NumberOfQuestions,
            Questions = quiz.Questions.Select(q => new QuestionResponse
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                Answers = q.Answers.Select(a => new AnswerResponse
                {
                    Id = a.Id,
                    AnswerText = a.AnswerText
                }).ToList()
            }).ToList()
        };

        return Ok(response);
    }

    [HttpPost("submit")]
    public async Task<ActionResult<QuizResultResponse>> SubmitQuiz([FromBody] QuizSubmission submission)
    {
        try
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == submission.QuizId);

            if (quiz == null)
            {
                return NotFound("Quiz not found");
            }

            if (string.IsNullOrWhiteSpace(submission.PlayerName))
            {
                return BadRequest("Player name is required");
            }

            // Calculate score
            int score = 0;
            foreach (var answerSubmission in submission.Answers)
            {
                var question = quiz.Questions.FirstOrDefault(q => q.Id == answerSubmission.QuestionId);
                if (question != null && answerSubmission.SelectedAnswerId == question.CorrectAnswerId)
                {
                    score++;
                }
            }

            // Save result
            var result = new QuizResult
            {
                QuizId = quiz.Id,
                PlayerName = submission.PlayerName,
                Score = score,
                TotalQuestions = quiz.NumberOfQuestions,
                CompletedAt = DateTime.UtcNow
            };

            _context.QuizResults.Add(result);
            await _context.SaveChangesAsync();

            var response = new QuizResultResponse
            {
                Id = result.Id,
                PlayerName = result.PlayerName,
                Score = result.Score,
                TotalQuestions = result.TotalQuestions,
                Percentage = (double)result.Score / result.TotalQuestions * 100,
                CompletedAt = result.CompletedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting quiz");
            return StatusCode(500, "Error submitting quiz. Please try again.");
        }
    }

    [HttpGet("results/{quizId}")]
    public async Task<ActionResult<List<QuizResultResponse>>> GetQuizResults(int quizId)
    {
        var results = await _context.QuizResults
            .Where(r => r.QuizId == quizId)
            .OrderByDescending(r => r.Score)
            .ThenBy(r => r.CompletedAt)
            .ToListAsync();

        var response = results.Select(r => new QuizResultResponse
        {
            Id = r.Id,
            PlayerName = r.PlayerName,
            Score = r.Score,
            TotalQuestions = r.TotalQuestions,
            Percentage = (double)r.Score / r.TotalQuestions * 100,
            CompletedAt = r.CompletedAt
        }).ToList();

        return Ok(response);
    }

    [HttpGet("list")]
    public async Task<ActionResult<List<QuizResponse>>> GetQuizList()
    {
        var quizzes = await _context.Quizzes
            .OrderByDescending(q => q.CreatedAt)
            .Take(10)
            .ToListAsync();

        var response = quizzes.Select(q => new QuizResponse
        {
            Id = q.Id,
            Topic = q.Topic,
            NumberOfQuestions = q.NumberOfQuestions,
            Questions = new List<QuestionResponse>() // Don't include questions in list view
        }).ToList();

        return Ok(response);
    }
} 