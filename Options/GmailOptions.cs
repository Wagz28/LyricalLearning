using Microsoft.Identity.Client;

namespace Email.API.Options;

public class GmailOptions {
    public const string GmailOptionsKey = "GmailOptions";

    public required string Host {get; set;}
    public int Port {get; set;}
    public required string Email {get; set;}
    public required string Password {get; set;}
}