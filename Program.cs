var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();


app.Run();

// Song object
public class Song
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required string Lyrics { get; set; }
}