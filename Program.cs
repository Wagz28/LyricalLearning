using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddAzureWebAppDiagnostics();  // Ensure this NuGet package is installed
builder.Logging.AddDebug();  // Add Debug logger as well
builder.Logging.SetMinimumLevel(LogLevel.Trace);  // Set to Trace to capture everything


// Add Razor Pages (or other services)
builder.Services.AddRazorPages();

var app = builder.Build();

var songs = new List<Song>
{
    new Song { Id = 1, Title = "Song One", Lyrics = "This is the lyrics for Song One." },
    new Song { Id = 2, Title = "Song Two", Lyrics = "This is the lyrics for Song Two." },
    new Song { Id = 3, Title = "Song Three", Lyrics = "This is the lyrics for Song Three." },
    new Song { Id = 4, Title = "Song Four", Lyrics = "This is the lyrics for Song Four." }
};

// Route for loading song names
app.MapGet("/api/songs", () => Results.Ok(songs));

// Route for loading song lyrics
app.MapGet("/api/song/{id}", (int id) =>
{
    var song = songs.FirstOrDefault(s => s.Id == id);
    if (song == null) return Results.NotFound();
    return Results.Ok(song);
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();

// Log that the app has started
var logger = app.Logger;
logger.LogInformation("✅ App started logging.");

// Test DB connection with error handling
try
{
    // Retrieve connection string from configuration
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
    using SqlConnection connection = new(connectionString);
    connection.Open();
    logger.LogInformation("✅ Database connection successful!");
}
catch (SqlException sqlEx)
{
    logger.LogError(sqlEx, "❌ Database connection failed!");
}
catch (Exception ex)
{
    logger.LogError(ex, "❌ Unexpected error occurred.");
}

app.Run();

// Song object
public class Song
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required string Lyrics { get; set; }
}
