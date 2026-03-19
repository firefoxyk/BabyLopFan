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
public class ClosenessModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IClosenessCalculator _calculator;
    private readonly UserManager<ApplicationUser> _userManager;

    private const long EstateBonus = 256500;

    public ClosenessModel(
        ApplicationDbContext db,
        IClosenessCalculator calculator,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _calculator = calculator;
        _userManager = userManager;
    }

    // Старые свойства и история оставлены, чтобы не ломать текущую страницу/модель проекта
    [BindProperty] public double CurrentPoints { get; set; }
    [BindProperty] public double TargetPoints { get; set; }
    [BindProperty] public int Attempts { get; set; }
    [BindProperty] public double? Result { get; set; }

    public string? SavedMessage { get; set; }
    public List<ClosenessCalculation> History { get; set; } = new();

    // Первая таблица
    [BindProperty] public int WhiteJadeCount { get; set; }
    [BindProperty] public int AgateJadeCount { get; set; }
    [BindProperty] public int HotanJadeCount { get; set; }
    [BindProperty] public int MangoNectarCount { get; set; }
    [BindProperty] public int PromoItemsCount { get; set; }
    [BindProperty] public int ClosenessBagsCount { get; set; }
    [BindProperty] public int RiceBallCount { get; set; }
    [BindProperty] public int FairyPotCount { get; set; }

    // Вторая таблица
    [BindProperty] public bool HasFullPass { get; set; }
    [BindProperty] public bool HasMonthPass { get; set; }
    [BindProperty] public int PenglaiFriendsCount { get; set; }
    [BindProperty] public bool IsShenLinghuaOpened { get; set; }
    [BindProperty] public int PhysicalPowerBallsCount { get; set; }

    // Первая таблица: значения по строкам
    public long WhiteJadeValue => WhiteJadeCount * 10L;
    public long AgateJadeValue => AgateJadeCount * 50L;
    public long HotanJadeValue => HotanJadeCount * 100L;
    public long MangoNectarValue => MangoNectarCount * 5L;
    public long PromoItemsValue => PromoItemsCount * 2000L;

    public long BagsMinValue => ClosenessBagsCount * 10L;
    public long BagsMaxValue => ClosenessBagsCount * 500L;

    public long RiceMinValue => RiceBallCount * 50L;
    public long RiceMaxValue => RiceBallCount * 2000L;

    public long PotMinValue => FairyPotCount * 0L;
    public long PotMaxValue => FairyPotCount * 200L;

    // Первая таблица: итоги
    public long MinTotal =>
        WhiteJadeValue +
        AgateJadeValue +
        HotanJadeValue +
        MangoNectarValue +
        PromoItemsValue +
        BagsMinValue +
        RiceMinValue +
        PotMinValue;

    public long MaxTotal =>
        WhiteJadeValue +
        AgateJadeValue +
        HotanJadeValue +
        MangoNectarValue +
        PromoItemsValue +
        BagsMaxValue +
        RiceMaxValue +
        PotMaxValue;

    public long AverageTotal => (MinTotal + MaxTotal) / 2L;

    public long MinWithEstate => MinTotal + EstateBonus;
    public long MaxWithEstate => MaxTotal + EstateBonus;
    public long AverageWithEstate => AverageTotal + EstateBonus;

    // Вторая таблица: параметры
    public int FullPassBonus => HasFullPass ? 10 : 0;
    public int MonthPassBonus => HasMonthPass ? 5 : 0;
    public int AllCloseFriendsCount => IsShenLinghuaOpened ? 13 : 12;
    public int AllEventsCount => IsShenLinghuaOpened ? 56 : 52;

    // Вторая таблица: расчеты
    public long RegularCityChancePercent =>
        (long)Math.Floor((double)AllCloseFriendsCount / AllEventsCount * 100d) + FullPassBonus + MonthPassBonus;

    public long PenglaiChancePercent =>
        (long)Math.Floor((double)PenglaiFriendsCount / AllEventsCount * 100d) + FullPassBonus + MonthPassBonus;

    public long RegularCityCloseness =>
        (long)Math.Floor((PhysicalPowerBallsCount * 3d * RegularCityChancePercent / 100d) * 50d);

    public long PenglaiCloseness =>
        (long)Math.Floor((PhysicalPowerBallsCount * 3d * PenglaiChancePercent / 100d) * 100d);

    public long PhysicalPowerTotalCloseness => RegularCityCloseness + PenglaiCloseness;

    public async Task OnGetAsync()
    {
        SetDefaults();
        await LoadHistory();
    }

    public async Task<IActionResult> OnPostCalculateAsync()
    {
        NormalizeValues();
        await LoadHistory();
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        NormalizeValues();
        await LoadHistory();
        return Page();
    }

    private void SetDefaults()
    {
        WhiteJadeCount = 0;
        AgateJadeCount = 0;
        HotanJadeCount = 0;
        MangoNectarCount = 0;
        PromoItemsCount = 0;
        ClosenessBagsCount = 0;
        RiceBallCount = 0;
        FairyPotCount = 0;

        HasFullPass = false;
        HasMonthPass = false;
        PenglaiFriendsCount = 0;
        IsShenLinghuaOpened = false;
        PhysicalPowerBallsCount = 0;
    }

    private void NormalizeValues()
    {
        WhiteJadeCount = EnsureNotNegative(WhiteJadeCount);
        AgateJadeCount = EnsureNotNegative(AgateJadeCount);
        HotanJadeCount = EnsureNotNegative(HotanJadeCount);
        MangoNectarCount = EnsureNotNegative(MangoNectarCount);
        PromoItemsCount = EnsureNotNegative(PromoItemsCount);
        ClosenessBagsCount = EnsureNotNegative(ClosenessBagsCount);
        RiceBallCount = EnsureNotNegative(RiceBallCount);
        FairyPotCount = EnsureNotNegative(FairyPotCount);

        PenglaiFriendsCount = EnsureNotNegative(PenglaiFriendsCount);
        PhysicalPowerBallsCount = EnsureNotNegative(PhysicalPowerBallsCount);
    }

    private static int EnsureNotNegative(int value)
    {
        return value < 0 ? 0 : value;
    }

    private async Task LoadHistory()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            History = new List<ClosenessCalculation>();
            return;
        }

        History = await _db.ClosenessCalculations
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(20)
            .ToListAsync();
    }
}