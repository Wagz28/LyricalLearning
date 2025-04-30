namespace Email.API.IMailService;

public interface IMailService {
    Task SendEmailAsync(SendEmailRequest sendEmailRequest);
}