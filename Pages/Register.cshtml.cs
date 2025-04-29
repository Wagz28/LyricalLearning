using System.ComponentModel.DataAnnotations;
using System.Net;
using LyricalLearning.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LyricalLearning.Pages;

public class RegisterModel : PageModel
{
    private readonly UserManager<Users> _userManager;
    private readonly SignInManager<Users> _signInManager;

    public RegisterModel(UserManager<Users> userManager, SignInManager<Users> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    [Required(ErrorMessage="Name is required.")]
    public required string Name { get; set; }

    [BindProperty]
    [Required(ErrorMessage="Email is required.")]
    [EmailAddress]
    public required string Email { get; set; }

    [BindProperty]
    [Required(ErrorMessage="Password is required.")]
    [StringLength(40, MinimumLength = 5, ErrorMessage = "The {0} must be at least {2} long.")]
    [DataType(DataType.Password)]
    public required string Password { get; set; }

    [BindProperty]
    [Required(ErrorMessage="Password Confirmation is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public required string ConfirmPassword { get; set; }


    public async Task<IActionResult> OnPostAsync()
    {
        // Console.WriteLine("Form received");
        if (!ModelState.IsValid)
        {
            // Console.WriteLine("Registration failed");
            return Page();
        }

        var user = new Users
        {
            UserName = Email,
            Email = Email,
            FullName = Name
        };
        

        var result = await _userManager.CreateAsync(user, Password);

        if (result.Succeeded)
        {
            // Verify email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            // Email functionality TODO

            return RedirectToPage("/VerifyEmail", new { email = Email, code = token});
        }

        bool duplicateUserAdded = false;
        foreach (var error in result.Errors) {
            if (error.Code == "DuplicateUserName" || error.Code == "DuplicateEmail") {
                if (!duplicateUserAdded) {
                    ModelState.AddModelError(string.Empty, "A user with this email already exists.");
                    duplicateUserAdded = true;
                }
            }
            else {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return Page();
    }
}
