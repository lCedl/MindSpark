using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Services;

namespace backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        
        // Add CORS for frontend
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:5000", "http://127.0.0.1:5000")
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        // Add HttpClient for OpenAI API
        builder.Services.AddHttpClient<IOpenAIService, OpenAIService>();

        builder.Services.AddDbContext<BackendContext>(opt =>
        {
            opt.UseInMemoryDatabase("BackendDatabase");
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUi(options =>
            {
                options.DocumentPath = "/openapi/v1.json";
            });
        }

        app.UseHttpsRedirection();

        // Use CORS
        app.UseCors("AllowFrontend");

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
