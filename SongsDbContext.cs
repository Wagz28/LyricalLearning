using Microsoft.EntityFrameworkCore;
public class SongsDbContext : DbContext
{
    public SongsDbContext(DbContextOptions<SongsDbContext> options)
        : base(options) { }

    public DbSet<Word> Words { get; set; }
    public DbSet<Sentence> Sentences { get; set; }
    public DbSet<SentenceWord> SentenceWords { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Word>().ToTable("Words");
        modelBuilder.Entity<Sentence>().ToTable("Sentences");
        modelBuilder.Entity<SentenceWord>()
            .ToTable("sentence_words")
            .HasKey(sw => new { sw.Song_Id, sw.Group_Id, sw.Sentence_Id, sw.Word_Id });

    }
}
