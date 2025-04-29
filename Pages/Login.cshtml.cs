using System.ComponentModel.DataAnnotations;
using LyricalLearning.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LyricalLearning.Pages;
public class LoginModel : PageModel {
    private readonly SignInManager<Users> _signInManager;
    public LoginModel(SignInManager<Users> signInManager)
    {
        _signInManager = signInManager;
    }

    [BindProperty]
    [Required(ErrorMessage="Email is required.")]
    [EmailAddress]
    public required string Email {get; set;}
    
    [BindProperty]
    [Required(ErrorMessage="Password is required.")]
    [DataType(DataType.Password)]
    public required string Password {get; set;}
    
    [BindProperty]
    [Display(Name = "Remember Me?")]
    public bool RememberMe {get; set;}

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(Email, Password, RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return RedirectToPage("/Index");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }
    }

}