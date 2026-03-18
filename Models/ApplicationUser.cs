using Microsoft.AspNetCore.Identity;

namespace GuildApp.Models;

public class ApplicationUser : IdentityUser
{
    // Никнейм пользователя в гильдии
    public string GuildNickname { get; set; } = string.Empty;

    // Флаг активности — деактивированные пользователи не могут войти
    public bool IsActive { get; set; } = true;
}
