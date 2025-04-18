using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices.Swift;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddAzureWebAppDiagnostics();  // Ensure this NuGet package is installed
builder.Logging.AddDebug();  // Add Debug logger as well
builder.Logging.SetMinimumLevel(LogLevel.Trace);  // Set to Trace to capture everything


// Add Razor Pages (or other services)
builder.Services.AddRazorPages();

builder.Services.AddDbContext<SongsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

// Remember which parts of the song are served with unique session IDs
var usedWords = new Dictionary<Guid, List<int>>();
var usedSentences = new Dictionary<Guid, List<int>>();
var usedParagraphs = new Dictionary<Guid, List<int>>();

// Route for loading song names
app.MapGet("/api/songs", (SongsDbContext db) =>
{
    var songs = db.SentenceWords
        .GroupBy(sw => new { sw.Song_Id, sw.Song_Name })
        .Select(g => new
        {
            Id = g.Key.Song_Id,
            Title = g.Key.Song_Name
        })
        .OrderBy(s => s.Id)
        .ToList();

    return Results.Ok(songs);
});

// Route for loading song words
app.MapGet("/api/words/{song_id}", (SongsDbContext db, int song_id) =>
{
    var songTitle = db.SentenceWords
        .Where(sw => sw.Song_Id == song_id)
        .Select(sw => sw.Song_Name)
        .FirstOrDefault();
    
    var wordIds = db.SentenceWords
        .Where(sw => sw.Song_Id == song_id)
        .Select(sw => sw.Word_Id)
        .Distinct()
        .OrderBy(_ => Guid.NewGuid())
        .Take(10)
        .ToList();

    var wordList = db.Words
        .Where(w => wordIds.Contains(w.Id) && w.En != null)
        .ToList();

    var guid = Guid.NewGuid();
    usedWords[guid] = wordList.Select(w => w.Id).ToList();

    return Results.Ok(new
    {
        id = guid,
        title = songTitle,
        words = wordList.Select(w => w.En)
    });
});

// Route for loading song sentences
app.MapGet("/api/sentences/{song_id}", (SongsDbContext db, int song_id) =>
{
    var songTitle = db.SentenceWords
    .Where(sw => sw.Song_Id == song_id)
    .Select(sw => sw.Song_Name)
    .FirstOrDefault();

    // Random rand = new Random();  
    // int skipper = rand.Next(0, db.SentenceWords.Count()); 

    var sentenceIds = db.SentenceWords
        .Where(sw => sw.Song_Id == song_id)
        .Select(sw => sw.Sentence_Id)
        .Distinct()
        .OrderBy(_ => Guid.NewGuid())
        .Take(3)
        .ToList();

    var sentenceList = db.Sentences
        .Where(s => sentenceIds.Contains(s.Id) && s.En != null)
        .ToList();

    var guid = Guid.NewGuid();
    usedSentences[guid] = sentenceList.Select(s => s.Id).ToList();

    return Results.Ok(new
    {
        id = guid,
        title = songTitle,
        sentences = sentenceList.Select(s => s.En)
    });
});

// Route for loading song paragraph
app.MapGet("/api/paragraph/{song_id}", (SongsDbContext db, int song_id) =>
{
    var songTitle = db.SentenceWords
    .Where(sw => sw.Song_Id == song_id)
    .Select(sw => sw.Song_Name)
    .FirstOrDefault();
    
    var groupId = db.SentenceWords
        .Where(sw => sw.Song_Id == song_id)
        .Select(sw => sw.Group_Id)
        .Distinct()
        .OrderBy(_ => Guid.NewGuid())
        .FirstOrDefault();

    var sentenceList = db.SentenceWords
        .Where(sw => sw.Group_Id == groupId && sw.Song_Id == song_id && sw.Word_Pst == 1)
        .OrderBy(sw => sw.Sentence_Pst)
        .Select(sw => sw.Sentence_Id)
        .ToList();
    
    var paragraphSentences = new List<string> {};
    foreach(var id in sentenceList) {
        paragraphSentences.Add(db.Sentences.Where(s => s.Id == id && s.Ru != null).Select(s => s.Ru).FirstOrDefault() ?? "[missing]");
    }

    var guid = Guid.NewGuid();
    usedParagraphs[guid] = sentenceList;

    return Results.Ok(new
    {
        id = guid,
        title = songTitle,
        paragraph = string.Join("\n", paragraphSentences)
    });
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
    public int Id { get; set; }
    public string Title { get; set; } = "";
}