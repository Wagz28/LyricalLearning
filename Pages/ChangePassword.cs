using System.ComponentModel.DataAnnotations;

namespace LyricalLearning.Pages;

public class ChangePasswordModel {
    [Required(ErrorMessage="Password is required.")]
    [StringLength(40, MinimumLength = 5, ErrorMessage = "The {0} must be at least {2} long.")]
    [DataType(DataType.Password)]
    [Compare("ConfirmNewPassword", ErrorMessage = "Passwords do not match.")]
    [Display(Name = "New Password.")]
    public required string NewPassword {get; set;}
    
    [Required(ErrorMessage = "New Password Confirmation is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password.")]
    public required string ConfirmNewPassword {get; set;}
}