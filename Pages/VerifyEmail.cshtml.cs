using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LyricalLearning.Models;
using System.ComponentModel.DataAnnotations;

namespace LyricalLearning.Pages
{
    public class VerifyEmailModel : PageModel
    {
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;

        public VerifyEmailModel(UserManager<Users> userManager, SignInManager<Users> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public string Email { get; set; } = "";
        [BindProperty]
        public string Code { get; set; } = "";

        [BindProperty]
        [Required(ErrorMessage = "You must enter the code you received.")]
        public string UserEnteredCode { get; set; } = "";

        public IActionResult OnGet(string? email, string? code) {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code)) {
                return BadRequest("Missing verification info.");
            }
            Email = email;
            Code = code;
            return Page();

        }

        public async Task<IActionResult> OnPostAsync()
        {

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                return BadRequest("Invalid user.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, UserEnteredCode);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToPage("/Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
