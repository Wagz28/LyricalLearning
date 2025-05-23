using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using LyricalLearning.data;
using LyricalLearning.Models;
using Microsoft.AspNetCore.Identity;
using Email.API.Options;
using email.API;
using Email.API.IMailService;
using email.API.GmailServices;
using Email.API;

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
    options.UseSqlServer(Environment.GetEnvironmentVariable("SQL_DEFAULT_CONNECTION")));

// User Login SQL Connection String
builder.Services.AddDbContext<UsersDbContext>(options => 
    options.UseSqlServer(Environment.GetEnvironmentVariable("SQL_USERS_CONNECTION")));

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
    options.SignIn.RequireConfirmedEmail = true;
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

// Enforce cookie consent for site usage
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    #if DEBUG
        options.MinimumSameSitePolicy = SameSiteMode.Lax;
    #else
        options.MinimumSameSitePolicy = SameSiteMode.None;
    #endif
});

builder.Services.Configure<GmailOptions>(builder.Configuration.GetSection(GmailOptions.GmailOptionsKey));

builder.Services.AddScoped<IMailService, GmailService>();

builder.Configuration.AddEnvironmentVariables();
builder.Configuration["GmailOptions:Password"] =
    Environment.GetEnvironmentVariable("GMAIL_APP_PASSWORD")
    ?? throw new InvalidOperationException("GMAIL_APP_PASSWORD not set.");

builder.Services.Configure<GmailOptions>(builder.Configuration.GetSection("GmailOptions"));


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


app.MapGet("/api/answers/{mode}/{ids}", (SongsDbContext db, string mode, string ids) =>
{
    // Split the ids string into a list of integers
    var wordIds = ids
        .Split('|', StringSplitOptions.RemoveEmptyEntries)
        .Select(id => int.Parse(id))
        .ToList();

    List<(int Id, string? En)> wordList = new();
    if (mode == "words") {
        wordList = db.Words
            .Where(w => wordIds.Contains(w.Id))
            .Select(w => new { w.Id, w.En }).ToList()
            .Select(w => (w.Id, w.En)).ToList();
    } else if (mode == "sentences" || mode == "paragraph") {
        wordList = db.Sentences
            .Where(p => wordIds.Contains(p.Id))
            .Select(p => new { p.Id, p.En }).ToList()
            .Select(p => (p.Id, p.En)).ToList();
    } else {
        return Results.BadRequest($"Unknown mode: {mode}");
    }

    // Create an ordered list based on the wordIds to match the original order
    var orderedWords = wordIds
        .Select(id => wordList.First(w => w.Id == id).En)
        .ToList();

    // Return the ordered words
    return Results.Ok(new { answers = orderedWords });
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
        .Where(w => wordIds.Contains(w.Id) && w.Og != null)
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
            text = w.Og
        }).ToList()
    });
});


// Route for loading song sentences
app.MapGet("/api/sentences/{song_id}", (SongsDbContext db, int song_id) =>
{
    var songTitle = db.SentenceWords
    .Where(sw => sw.Song_Id == song_id)
    .Select(sw => sw.Song_Name)
    .FirstOrDefault();

    var sentenceIds = db.SentenceWords
        .Where(sw => sw.Song_Id == song_id)
        .Select(sw => sw.Sentence_Id)
        .Distinct()
        .OrderBy(_ => Guid.NewGuid())
        .Take(3)
        .ToList();

    var sentenceList = db.Sentences
        .Where(s => sentenceIds.Contains(s.Id) && s.Og != null)
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
            text = s.Og
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

    var paragraphSentences = sentenceList
        .Select(id => new {
            id = id,
            text = db.Sentences
                .Where(s => s.Id == id && s.Og != null)
                .Select(s => s.Og)
                .FirstOrDefault() ?? "[missing]"
        })
        .ToList();

    var guid = Guid.NewGuid();
    usedParagraphs[guid] = sentenceList;

    return Results.Ok(new
    {
        id = guid,
        title = songTitle,
        paragraph = paragraphSentences
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
app.UseCookiePolicy();
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
    string? connectionString = Environment.GetEnvironmentVariable("SQL_DEFAULT_CONNECTION")
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