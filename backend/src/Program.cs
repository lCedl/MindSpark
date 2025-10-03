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
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:5000", "http://127.0.0.1:5000")
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        builder.Services.AddHttpClient<IOpenAIService, OpenAIService>();

        builder.Services.AddDbContext<BackendContext>(opt =>
        {
            opt.UseSqlite("Data Source=./sqlite/mindspark.db");
        });

        var app = builder.Build();

        // Ensure database is created and migrations are applied
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BackendContext>();
            try
            {
                context.Database.EnsureCreated();
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while migrating the database.");
            }
        }

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUi(options =>
            {
                options.DocumentPath = "/openapi/v1.json";
            });
        }

        app.UseHttpsRedirection();

        app.UseCors("AllowFrontend");

        app.UseAuthorization();

        app.MapControllers();

        // Add health check endpoint
        app.MapGet("/health", () => "OK");

        app.Run();
    }
}
