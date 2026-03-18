using GuildApp.Data;
using GuildApp.Models;
using GuildApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuildApp.Pages.Calculations;

[Authorize]
public class PowerModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IPowerCalculator _calculator;
    private readonly UserManager<ApplicationUser> _userManager;

    public PowerModel(ApplicationDbContext db, IPowerCalculator calculator, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _calculator = calculator;
        _userManager = userManager;
    }

    [BindProperty] public double CurrentPoints { get; set; }
    [BindProperty] public double TargetPoints { get; set; }
    [BindProperty] public int Attempts { get; set; }
    [BindProperty] public double? Result { get; set; }

    public string? SavedMessage { get; set; }
    public List<PowerCalculation> History { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadHistory();
    }

    public async Task<IActionResult> OnPostCalculateAsync()
    {
        // Считаем результат и показываем на странице без сохранения
        Result = _calculator.Calculate(CurrentPoints, TargetPoints, Attempts);
        await LoadHistory();
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        // Считаем и сохраняем результат в базу данных
        var userId = _userManager.GetUserId(User)!;
        Result = _calculator.Calculate(CurrentPoints, TargetPoints, Attempts);

        _db.PowerCalculations.Add(new PowerCalculation
        {
            UserId = userId,
            CurrentPoints = CurrentPoints,
            TargetPoints = TargetPoints,
            Attempts = Attempts,
            Result = Result.Value
        });
        await _db.SaveChangesAsync();

        SavedMessage = "Результат сохранён.";
        await LoadHistory();
        return Page();
    }

    private async Task LoadHistory()
    {
        var userId = _userManager.GetUserId(User)!;
        History = await _db.PowerCalculations
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(20)
            .ToListAsync();
    }
}
