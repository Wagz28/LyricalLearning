using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LyricalLearning.Models;
using Microsoft.AspNetCore.Authorization;

namespace LyricalLearning.Pages
{
    [Authorize]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<Users> _signInManager;

        public LogoutModel(SignInManager<Users> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Login");
        }
    }
}
