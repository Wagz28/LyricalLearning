using System.ComponentModel.DataAnnotations;

namespace LyricalLearning.Pages;

public class VerifyEmailModel {
    [Required(ErrorMessage="Email is required.")]
    [EmailAddress]
    public required string Email {get; set;}
}