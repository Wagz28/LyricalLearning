namespace Email.API;

public record SendEmailRequest(string Recipient, string Subject, string Body);