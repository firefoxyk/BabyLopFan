using GuildApp.Data;
using GuildApp.Models;
using GuildApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Подключаем базу данных SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Настраиваем Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Настраиваем редирект для неавторизованных пользователей
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});

// Регистрируем калькуляторы
builder.Services.AddScoped<IHuntCalculator, HuntCalculator>();
builder.Services.AddScoped<IWorldFameCalculator, WorldFameCalculator>();
builder.Services.AddScoped<IClosenessCalculator, ClosenessCalculator>();
builder.Services.AddScoped<IAssistantCalculator, AssistantCalculator>();
builder.Services.AddScoped<IPowerCalculator, PowerCalculator>();
builder.Services.AddScoped<IBanquetCalculator, BanquetCalculator>();

builder.Services.AddRazorPages();

var app = builder.Build();

// Создаём роли и администратора при первом запуске
await SeedData(app);

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();

// Инициализация начальных данных: роли и первый администратор
static async Task SeedData(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Применяем миграции автоматически
    await db.Database.MigrateAsync();

    // Создаём роли если их нет
    foreach (var role in new[] { "Admin", "Member" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Создаём администратора по умолчанию если его нет
    if (await userManager.FindByNameAsync("admin") == null)
    {
        var admin = new ApplicationUser
        {
            UserName = "admin",
            GuildNickname = "Администратор",
            IsActive = true
        };
        var result = await userManager.CreateAsync(admin, "admin123");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}
