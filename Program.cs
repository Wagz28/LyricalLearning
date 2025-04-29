using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using LyricalLearning.data;
using LyricalLearning.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddAzureWebAppDiagnostics();  // Ensure this NuGet package is installed
builder.Logging.AddDebug();  // Add Debug logger as well
builder.Logging.SetMinimumLevel(LogLevel.Trace);  // Set to Trace to capture everything


// Add Razor Pages (or other services)
builder.Services.AddRazorPages();

// Song Data SQL Connection String
builder.Services.AddDbContext<SongsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// User Login SQL Connection String
builder.Services.AddDbContext<UsersDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("UsersString")));

// User Login Conditions
builder.Services.AddIdentity<Users, IdentityRole>(options => 
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 5;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
    .AddEntityFrameworkStores<UsersDbContext>()
    .AddDefaultTokenProviders();

// Define how to handle cookies
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Login"; // redirect unauthenticated users
    options.AccessDeniedPath = "/AccessDenied"; // Redirect from pages that require authentification
    options.Cookie.HttpOnly = true; // Ensure the cookies cannot be sent over client side JS
    options.ExpireTimeSpan = TimeSpan.FromMinutes(1); // timeout for non-persistent cookies (e.g. account when remember me not clicked)
    options.SlidingExpiration = true; // Don't expire while active
});

var app = builder.Build();

app.MapGet("/", context =>
{
    if (context.User.Identity != null && context.User.Identity.IsAuthenticated) {
        context.Response.Redirect("/Index"); // if logged in
    }
    else {
        context.Response.Redirect("/Login"); // if not logged in
    }
    return Task.CompletedTask;
});

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
        .Where(w => wordIds.Contains(w.Id) && w.Ru != null)
        .OrderBy(_ => Guid.NewGuid())
        .ToList();

    var guid = Guid.NewGuid();
    usedWords[guid] = wordList.Select(w => w.Id).ToList();
    return Results.Ok(new
    {
        id = guid,
        title = songTitle,
        words = wordList.Select(w => new
        {
            id = w.Id,
            ru = w.Ru
        }).ToList()
    });
});

app.MapGet("/api/word-answers/{guid}", (SongsDbContext db, Guid guid) =>
{
    if (!usedWords.TryGetValue(guid, out var wordIds))
    {
        return Results.NotFound();
    }

    var wordList = db.Words
        .Where(w => wordIds.Contains(w.Id))
        .Select(w => new
        {
            id = w.Id,
            en = w.En
        })
        .ToList();

    return Results.Ok(new { answers = wordList });
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
        .Where(s => sentenceIds.Contains(s.Id) && s.Ru != null)
        .ToList();

    var guid = Guid.NewGuid();
    usedSentences[guid] = sentenceList.Select(s => s.Id).ToList();

    return Results.Ok(new
    {
        id = guid,
        title = songTitle,
        sentences = sentenceList.Select(s => new
        {
            id = s.Id,
            ru = s.Ru
        }).ToList()
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
app.UseAuthentication();
app.UseAuthorization();

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