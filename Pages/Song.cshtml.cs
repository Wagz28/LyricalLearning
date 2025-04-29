using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LyricalLearning.Pages;

[Authorize]
public class SongModel : PageModel
{
    public void OnGet(int id)
    {
    }
}
