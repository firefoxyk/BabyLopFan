using GuildApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GuildApp.Pages.Admin;

[Authorize(Roles = "Admin")]
public class UsersModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // Вспомогательный класс для отображения пользователя с его ролью
    public class UserRow
    {
        public string Id { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string GuildNickname { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public List<UserRow> Users { get; set; } = new();
    public string? StatusMessage { get; set; }

    [BindProperty] public string NewUsername { get; set; } = string.Empty;
    [BindProperty] public string NewGuildNickname { get; set; } = string.Empty;
    [BindProperty] public string NewPassword { get; set; } = string.Empty;
    [BindProperty] public string NewRole { get; set; } = "Member";

    public async Task OnGetAsync()
    {
        await LoadUsers();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        // Создаём нового пользователя с выбранной ролью
        var user = new ApplicationUser
        {
            UserName = NewUsername,
            GuildNickname = NewGuildNickname,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            await LoadUsers();
            return Page();
        }

        await _userManager.AddToRoleAsync(user, NewRole);
        StatusMessage = $"Пользователь «{NewUsername}» успешно создан.";
        await LoadUsers();
        return Page();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(string userId)
    {
        // Деактивируем пользователя — он больше не сможет войти
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.IsActive = false;
            await _userManager.UpdateAsync(user);
            StatusMessage = $"Пользователь «{user.UserName}» деактивирован.";
        }
        await LoadUsers();
        return Page();
    }

    public async Task<IActionResult> OnPostActivateAsync(string userId)
    {
        // Восстанавливаем доступ пользователя
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.IsActive = true;
            await _userManager.UpdateAsync(user);
            StatusMessage = $"Пользователь «{user.UserName}» активирован.";
        }
        await LoadUsers();
        return Page();
    }

    private async Task LoadUsers()
    {
        // Загружаем всех пользователей и их роли
        var users = _userManager.Users.OrderBy(u => u.UserName).ToList();
        Users = new List<UserRow>();

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            Users.Add(new UserRow
            {
                Id = u.Id,
                UserName = u.UserName,
                GuildNickname = u.GuildNickname,
                Role = roles.FirstOrDefault() ?? "—",
                IsActive = u.IsActive
            });
        }
    }
}
