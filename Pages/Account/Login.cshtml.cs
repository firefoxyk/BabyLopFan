using GuildApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GuildApp.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.FindByNameAsync(Username);

        // Проверяем существование и активность пользователя
        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Неверное имя пользователя или пароль.");
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(user, Password, false, false);
        if (result.Succeeded)
            return RedirectToPage("/Index");

        ModelState.AddModelError(string.Empty, "Неверное имя пользователя или пароль.");
        return Page();
    }
}
