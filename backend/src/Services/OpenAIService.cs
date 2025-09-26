using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
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

        int maxRetries = 3;
        int retryDelayMs = 2000;

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
                            retryDelayMs *= 2;
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
                _logger.LogInformation($"OpenAI Response: {responseContent}");
                
                var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);
                _logger.LogInformation($"Deserialized OpenAI Response - Choices Count: {openAIResponse?.Choices?.Count ?? 0}");
                
                if (openAIResponse?.Choices?.FirstOrDefault()?.Message?.Content == null)
                {
                    _logger.LogError($"Invalid OpenAI response structure. Response: {responseContent}");
                    throw new Exception($"Invalid response from OpenAI API. Response structure: {responseContent}");
                }

                var quizJson = openAIResponse.Choices[0].Message.Content;
                _logger.LogInformation($"Quiz JSON from OpenAI: {quizJson}");
                
                OpenAIQuizResponse quizData;
                try
                {
                    quizData = JsonSerializer.Deserialize<OpenAIQuizResponse>(quizJson);

                    if (quizData?.Questions == null)
                    {
                        _logger.LogError($"Failed to parse quiz data. Quiz JSON: {quizJson}");
                        throw new Exception($"Failed to parse quiz data from OpenAI response. Quiz JSON: {quizJson}");
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, $"JSON parsing error for quiz data. Quiz JSON: {quizJson}");
                    throw new Exception($"Failed to parse quiz JSON from OpenAI response: {ex.Message}. Quiz JSON: {quizJson}");
                }

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

                    int correctAnswerIndex = -1;
                    for (int i = 0; i < questionData.Answers.Count; i++)
                    {
                        var answer = new Answer
                        {
                            AnswerText = questionData.Answers[i].Text
                        };
                        question.Answers.Add(answer);

                        if (questionData.Answers[i].IsCorrect)
                        {
                            correctAnswerIndex = i;
                        }
                    }

                    question.CorrectAnswerId = correctAnswerIndex;

                    quiz.Questions.Add(question);
                }

                return quiz;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error generating quiz with OpenAI");
                throw;
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

public class OpenAIResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;
    
    [JsonPropertyName("created")]
    public long Created { get; set; }
    
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
    
    [JsonPropertyName("choices")]
    public List<OpenAIChoice> Choices { get; set; } = new();
    
    [JsonPropertyName("usage")]
    public OpenAIUsage Usage { get; set; } = new();
}

public class OpenAIChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }
    
    [JsonPropertyName("message")]
    public OpenAIMessage Message { get; set; } = new();
    
    [JsonPropertyName("logprobs")]
    public object? Logprobs { get; set; }
    
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = string.Empty;
}

public class OpenAIMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
    
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
    
    [JsonPropertyName("refusal")]
    public object? Refusal { get; set; }
    
    [JsonPropertyName("annotations")]
    public List<object> Annotations { get; set; } = new();
}

public class OpenAIUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }
    
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }
    
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

public class OpenAIQuizResponse
{
    [JsonPropertyName("questions")]
    public List<OpenAIQuestion> Questions { get; set; } = new();
}

public class OpenAIQuestion
{
    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;
    
    [JsonPropertyName("answers")]
    public List<OpenAIAnswer> Answers { get; set; } = new();
}

public class OpenAIAnswer
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
    
    [JsonPropertyName("isCorrect")]
    public bool IsCorrect { get; set; }
}
