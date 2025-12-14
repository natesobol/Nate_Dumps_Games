using System.Collections.Concurrent;
using System.IO;
using Microsoft.Extensions.FileProviders;
using TriviaWhip.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});
builder.Services.AddSingleton<ConcurrentBag<LeaderboardEntry>>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "Files")),
    RequestPath = "/Files"
});

app.MapGet("/api/leaderboard", (ConcurrentBag<LeaderboardEntry> store) =>
{
    var ordered = store.OrderByDescending(x => x.Score).Take(50).ToList();
    return Results.Ok(ordered);
});

app.MapPost("/api/leaderboard", (LeaderboardEntry entry, ConcurrentBag<LeaderboardEntry> store) =>
{
    store.Add(entry with { When = DateTimeOffset.UtcNow });
    return Results.Accepted($"/api/leaderboard/{entry.Player}");
});

app.MapPost("/api/mail/send", (FeedbackRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Body))
    {
        return Results.BadRequest("Message body is required.");
    }

    return Results.Ok(new { accepted = true, request.Email, request.Subject });
});

app.MapFallbackToFile("index.html");

app.Run();
