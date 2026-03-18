using GuildApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GuildApp.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public string GuildNickname { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        // Загружаем никнейм для приветствия
        var user = await _userManager.GetUserAsync(User);
        GuildNickname = user?.GuildNickname ?? User.Identity?.Name ?? "Гость";
    }
}
