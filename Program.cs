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
app.MapGet("/api/songs", () => 
{
    var songList = new List<Song>();

    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!; // Configure connection to default DB in Azure setup
    using var connection = new SqlConnection(connectionString); // Connect
    connection.Open(); // Open the DB

    string sql = "SELECT Id, Title, Lyrics FROM Songs"; // Construct the query

    using var command = new SqlCommand(sql, connection); // Convert to SQL Query
    using var reader = command.ExecuteReader(); // Create reader for result returned by query

    while (reader.Read())
    {
        songList.Add(new Song
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Lyrics = reader.GetString(2)
        });
    }

    return Results.Ok(songList);
});

// Route for loading song lyrics
app.MapGet("/api/song/{id}", (int id) =>
{
    Song? song = null;

    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
    using var connection = new SqlConnection(connectionString);
    connection.Open();

    string sql = "SELECT Id, Title, Lyrics FROM Songs WHERE Id = @Id";

    using var command = new SqlCommand(sql, connection);
    command.Parameters.AddWithValue("@Id", id); //  Insert id value in secure way (mitigates SQL injection vunerability)

    using var reader = command.ExecuteReader();

    if (reader.Read())
    {
        song = new Song
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Lyrics = reader.GetString(2)
        };
    }

    return song is not null ? Results.Ok(song) : Results.NotFound();
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