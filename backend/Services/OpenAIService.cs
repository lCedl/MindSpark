using System.Text.Json;
using Backend.Models;

namespace Backend.Services;

public interface IOpenAIService
{
    Task<Quiz> GenerateQuizAsync(string topic, int numberOfQuestions);
}

public class OpenAIService : IOpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAIService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Quiz> GenerateQuizAsync(string topic, int numberOfQuestions)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var prompt = $@"Generate a quiz about '{topic}' with {numberOfQuestions} multiple choice questions. 
        Each question should have 4 answer options (A, B, C, D) with only one correct answer.
        
        Return the response in the following JSON format:
        {{
            ""questions"": [
                {{
                    ""question"": ""Question text here"",
                    ""answers"": [
                        {{ ""text"": ""Answer A"", ""isCorrect"": true }},
                        {{ ""text"": ""Answer B"", ""isCorrect"": false }},
                        {{ ""text"": ""Answer C"", ""isCorrect"": false }},
                        {{ ""text"": ""Answer D"", ""isCorrect"": false }}
                    ]
                }}
            ]
        }}";

        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                new { role = "system", content = "You are a helpful assistant that creates educational quizzes. Always respond with valid JSON in the exact format requested." },
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 2000
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        // Retry logic for rate limits
        int maxRetries = 3;
        int retryDelayMs = 2000; // Start with 2 seconds

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"OpenAI API Error - Status: {response.StatusCode}, Content: {errorContent}");
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        if (attempt < maxRetries)
                        {
                            _logger.LogWarning($"Rate limit hit, attempt {attempt}/{maxRetries}. Waiting {retryDelayMs}ms before retry.");
                            await Task.Delay(retryDelayMs);
                            retryDelayMs *= 2; // Exponential backoff
                            continue;
                        }
                        else
                        {
                            throw new HttpRequestException($"OpenAI Rate Limit Exceeded (429) - {errorContent}");
                        }
                    }
                    else
                    {
                        throw new HttpRequestException($"OpenAI API Error - Status: {response.StatusCode}, Content: {errorContent}");
                    }
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

                if (openAIResponse?.Choices?.FirstOrDefault()?.Message?.Content == null)
                {
                    throw new Exception("Invalid response from OpenAI API");
                }

                var quizJson = openAIResponse.Choices[0].Message.Content;
                var quizData = JsonSerializer.Deserialize<OpenAIQuizResponse>(quizJson);

                if (quizData?.Questions == null)
                {
                    throw new Exception("Failed to parse quiz data from OpenAI response");
                }

                // Convert to our domain model
                var quiz = new Quiz
                {
                    Topic = topic,
                    NumberOfQuestions = numberOfQuestions,
                    CreatedAt = DateTime.UtcNow,
                    Questions = new List<Question>()
                };

                foreach (var questionData in quizData.Questions)
                {
                    var question = new Question
                    {
                        QuestionText = questionData.Question,
                        Answers = new List<Answer>()
                    };

                    for (int i = 0; i < questionData.Answers.Count; i++)
                    {
                        var answer = new Answer
                        {
                            AnswerText = questionData.Answers[i].Text
                        };
                        question.Answers.Add(answer);

                        if (questionData.Answers[i].IsCorrect)
                        {
                            question.CorrectAnswerId = answer.Id;
                        }
                    }

                    quiz.Questions.Add(question);
                }

                return quiz;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error generating quiz with OpenAI");
                throw; // Re-throw to preserve the original error details
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error generating quiz with OpenAI");
                throw;
            }
        }

        throw new Exception("Failed to generate quiz after all retry attempts");
    }
}

// OpenAI API response models
public class OpenAIResponse
{
    public List<OpenAIChoice> Choices { get; set; } = new();
}

public class OpenAIChoice
{
    public OpenAIMessage Message { get; set; } = new();
}

public class OpenAIMessage
{
    public string Content { get; set; } = string.Empty;
}

public class OpenAIQuizResponse
{
    public List<OpenAIQuestion> Questions { get; set; } = new();
}

public class OpenAIQuestion
{
    public string Question { get; set; } = string.Empty;
    public List<OpenAIAnswer> Answers { get; set; } = new();
}

public class OpenAIAnswer
{
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
} 