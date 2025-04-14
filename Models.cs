public class Word
{
    public int Id { get; set; }
    public string? En { get; set; }
    public string? Ru { get; set; }
    public string? De { get; set; }
    public string? Es { get; set; }
    public string? Fr { get; set; }
}

public class Sentence
{
    public int Id { get; set; }
    public string? En { get; set; }
    public string? Ru { get; set; }
    public string? De { get; set; }
    public string? Es { get; set; }
    public string? Fr { get; set; }
}

public class SentenceWord
{
    public int Song_Id { get; set; }
    public int Group_Id { get; set; }
    public int Sentence_Id { get; set; }
    public int Word_Id { get; set; }
    public string Song_Name { get; set; } = "";
    public int Sentence_Pst { get; set; }
    public int Word_Pst { get; set; }
}
