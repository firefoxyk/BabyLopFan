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

    private const long MinutesPerDay = 24 * 60;
    private const long CircleDays = 47;

    public ClosenessModel(
        ApplicationDbContext db,
        IClosenessCalculator calculator,
        UserManager<ApplicationUser> userManager)
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
    public List<ClosenessCalculation> History { get; set; } = new();

    [BindProperty] public bool ShowDeepFeelingsResults { get; set; }

    public class DeepFeelingCell
    {
        public bool IsHidden { get; set; }
        public bool IsPassed { get; set; }
        public long? Value { get; set; }

        public string DisplayText
        {
            get
            {
                if (IsHidden)
                {
                    return string.Empty;
                }

                if (IsPassed)
                {
                    return "Х";
                }

                return (Value ?? 0).ToString("N0").Replace(",", " ");
            }
        }

        public string CssClass
        {
            get
            {
                if (IsHidden)
                {
                    return "deep-hidden";
                }

                if (IsPassed)
                {
                    return "deep-x";
                }

                if (!Value.HasValue)
                {
                    return "deep-neutral";
                }

                if (Value.Value > 0)
                {
                    return "deep-positive";
                }

                if (Value.Value < 0)
                {
                    return "deep-negative";
                }

                return "deep-neutral";
            }
        }
    }

    public class DeepFeelingRow
    {
        public int LevelNumber { get; set; }
        public List<DeepFeelingCell> Cells { get; set; } = new();

        public bool ShouldRender => Cells.Any(x => !x.IsHidden);
    }

    public class DeepFeelingSectionResult
    {
        public List<DeepFeelingRow> Rows { get; set; } = new();
        public List<int> TotalLevels { get; set; } = new() { 0, 0, 0, 0 };
        public List<long> ClosenessValues { get; set; } = new() { 0, 0, 0, 0 };
    }

    public DeepFeelingSectionResult TheaterDeepResult { get; set; } = new();
    public DeepFeelingSectionResult AntiqueDeepResult { get; set; } = new();

    public List<long> LevelCosts { get; } = new()
    {
        333000,
        700000,
        1200000,
        1800000,
        2800000,
        3700000,
        5000000,
        6100000,
        7900000,
        9300000,
        10500000,
        12000000,
        15300000,
        19800000,
        24400000,
        28900000,
        34000000,
        39600000,
        45100000
    };

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

    // Третья таблица - Театр
    [BindProperty] public long TheaterWhipProductionPerMinute { get; set; }
    [BindProperty] public long TheaterWhipCurrentAmount { get; set; }
    [BindProperty] public long TheaterWhipDaysCount { get; set; }

    [BindProperty] public long TheaterFanProductionPerMinute { get; set; }
    [BindProperty] public long TheaterFanCurrentAmount { get; set; }
    [BindProperty] public long TheaterFanDaysCount { get; set; }

    [BindProperty] public long TheaterHatProductionPerMinute { get; set; }
    [BindProperty] public long TheaterHatCurrentAmount { get; set; }
    [BindProperty] public long TheaterHatDaysCount { get; set; }

    [BindProperty] public long TheaterMaskProductionPerMinute { get; set; }
    [BindProperty] public long TheaterMaskCurrentAmount { get; set; }
    [BindProperty] public long TheaterMaskDaysCount { get; set; }

    // Третья таблица - Антикварная лавка
    [BindProperty] public long AntiqueEmbroideryProductionPerMinute { get; set; }
    [BindProperty] public long AntiqueEmbroideryCurrentAmount { get; set; }
    [BindProperty] public long AntiqueEmbroideryDaysCount { get; set; }

    [BindProperty] public long AntiquePorcelainProductionPerMinute { get; set; }
    [BindProperty] public long AntiquePorcelainCurrentAmount { get; set; }
    [BindProperty] public long AntiquePorcelainDaysCount { get; set; }

    [BindProperty] public long AntiqueScrollProductionPerMinute { get; set; }
    [BindProperty] public long AntiqueScrollCurrentAmount { get; set; }
    [BindProperty] public long AntiqueScrollDaysCount { get; set; }

    [BindProperty] public long AntiqueJadeDecorationProductionPerMinute { get; set; }
    [BindProperty] public long AntiqueJadeDecorationCurrentAmount { get; set; }
    [BindProperty] public long AntiqueJadeDecorationDaysCount { get; set; }

    // Расходы глубоких чувств - Театр
    [BindProperty] public int TheaterWhipDeepLevel { get; set; }
    [BindProperty] public int TheaterFanDeepLevel { get; set; }
    [BindProperty] public int TheaterHatDeepLevel { get; set; }
    [BindProperty] public int TheaterMaskDeepLevel { get; set; }

    [BindProperty] public long TheaterWhipSpendResource { get; set; }
    [BindProperty] public long TheaterFanSpendResource { get; set; }
    [BindProperty] public long TheaterHatSpendResource { get; set; }
    [BindProperty] public long TheaterMaskSpendResource { get; set; }

    [BindProperty] public long TheaterWhipBonusLevel { get; set; }
    [BindProperty] public long TheaterFanBonusLevel { get; set; }
    [BindProperty] public long TheaterHatBonusLevel { get; set; }
    [BindProperty] public long TheaterMaskBonusLevel { get; set; }

    // Расходы глубоких чувств - Антикварная лавка
    [BindProperty] public int AntiqueEmbroideryDeepLevel { get; set; }
    [BindProperty] public int AntiquePorcelainDeepLevel { get; set; }
    [BindProperty] public int AntiqueScrollDeepLevel { get; set; }
    [BindProperty] public int AntiqueJadeDecorationDeepLevel { get; set; }

    [BindProperty] public long AntiqueEmbroideryBonusLevel { get; set; }
    [BindProperty] public long AntiquePorcelainBonusLevel { get; set; }
    [BindProperty] public long AntiqueScrollBonusLevel { get; set; }
    [BindProperty] public long AntiqueJadeDecorationBonusLevel { get; set; }

    [BindProperty] public long AntiqueEmbroiderySpendResource { get; set; }
    [BindProperty] public long AntiquePorcelainSpendResource { get; set; }
    [BindProperty] public long AntiqueScrollSpendResource { get; set; }
    [BindProperty] public long AntiqueJadeDecorationSpendResource { get; set; }

    // Первая таблица
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

    public long EstateClosenessTotal =>
        TheaterDeepResult.ClosenessValues[0] +
        TheaterDeepResult.ClosenessValues[1] +
        TheaterDeepResult.ClosenessValues[2] +
        TheaterDeepResult.ClosenessValues[3] +
        AntiqueDeepResult.ClosenessValues[0] +
        AntiqueDeepResult.ClosenessValues[1] +
        AntiqueDeepResult.ClosenessValues[2] +
        AntiqueDeepResult.ClosenessValues[3];

    public long MinWithEstate => MinTotal + EstateClosenessTotal;
    public long MaxWithEstate => MaxTotal + EstateClosenessTotal;
    public long AverageWithEstate => AverageTotal + EstateClosenessTotal;

    // Вторая таблица
    public int FullPassBonus => HasFullPass ? 10 : 0;
    public int MonthPassBonus => HasMonthPass ? 5 : 0;
    public int AllCloseFriendsCount => IsShenLinghuaOpened ? 13 : 12;
    public int AllEventsCount => IsShenLinghuaOpened ? 56 : 52;

    public long RegularCityChancePercent =>
        (long)Math.Floor((double)AllCloseFriendsCount / AllEventsCount * 100d) + FullPassBonus + MonthPassBonus;

    public long PenglaiChancePercent =>
        (long)Math.Floor((double)PenglaiFriendsCount / AllEventsCount * 100d) + FullPassBonus + MonthPassBonus;

    public long RegularCityCloseness =>
        (long)Math.Floor((PhysicalPowerBallsCount * 3d * RegularCityChancePercent / 100d) * 50d);

    public long PenglaiCloseness =>
        (long)Math.Floor((PhysicalPowerBallsCount * 3d * PenglaiChancePercent / 100d) * 100d);

    public long PhysicalPowerTotalCloseness => RegularCityCloseness + PenglaiCloseness;

    // Третья таблица - Театр
    public long TheaterWhipPerDay => CalcPerDay(TheaterWhipProductionPerMinute);
    public long TheaterWhipFor47Days => CalcFor47Days(TheaterWhipPerDay);
    public long TheaterWhipForNDays => CalcForNDays(TheaterWhipDaysCount, TheaterWhipPerDay);
    public long TheaterWhipCurrentPlusDay => CalcCurrentPlus(TheaterWhipCurrentAmount, TheaterWhipPerDay);
    public long TheaterWhipCurrentPlus47Days => CalcCurrentPlus(TheaterWhipCurrentAmount, TheaterWhipFor47Days);
    public long TheaterWhipCurrentPlusNDays => CalcCurrentPlus(TheaterWhipCurrentAmount, TheaterWhipForNDays);

    public long TheaterFanPerDay => CalcPerDay(TheaterFanProductionPerMinute);
    public long TheaterFanFor47Days => CalcFor47Days(TheaterFanPerDay);
    public long TheaterFanForNDays => CalcForNDays(TheaterFanDaysCount, TheaterFanPerDay);
    public long TheaterFanCurrentPlusDay => CalcCurrentPlus(TheaterFanCurrentAmount, TheaterFanPerDay);
    public long TheaterFanCurrentPlus47Days => CalcCurrentPlus(TheaterFanCurrentAmount, TheaterFanFor47Days);
    public long TheaterFanCurrentPlusNDays => CalcCurrentPlus(TheaterFanCurrentAmount, TheaterFanForNDays);

    public long TheaterHatPerDay => CalcPerDay(TheaterHatProductionPerMinute);
    public long TheaterHatFor47Days => CalcFor47Days(TheaterHatPerDay);
    public long TheaterHatForNDays => CalcForNDays(TheaterHatDaysCount, TheaterHatPerDay);
    public long TheaterHatCurrentPlusDay => CalcCurrentPlus(TheaterHatCurrentAmount, TheaterHatPerDay);
    public long TheaterHatCurrentPlus47Days => CalcCurrentPlus(TheaterHatCurrentAmount, TheaterHatFor47Days);
    public long TheaterHatCurrentPlusNDays => CalcCurrentPlus(TheaterHatCurrentAmount, TheaterHatForNDays);

    public long TheaterMaskPerDay => CalcPerDay(TheaterMaskProductionPerMinute);
    public long TheaterMaskFor47Days => CalcFor47Days(TheaterMaskPerDay);
    public long TheaterMaskForNDays => CalcForNDays(TheaterMaskDaysCount, TheaterMaskPerDay);
    public long TheaterMaskCurrentPlusDay => CalcCurrentPlus(TheaterMaskCurrentAmount, TheaterMaskPerDay);
    public long TheaterMaskCurrentPlus47Days => CalcCurrentPlus(TheaterMaskCurrentAmount, TheaterMaskFor47Days);
    public long TheaterMaskCurrentPlusNDays => CalcCurrentPlus(TheaterMaskCurrentAmount, TheaterMaskForNDays);

    // Третья таблица - Антикварная лавка
    public long AntiqueEmbroideryPerDay => CalcPerDay(AntiqueEmbroideryProductionPerMinute);
    public long AntiqueEmbroideryFor47Days => CalcFor47Days(AntiqueEmbroideryPerDay);
    public long AntiqueEmbroideryForNDays => CalcForNDays(AntiqueEmbroideryDaysCount, AntiqueEmbroideryPerDay);
    public long AntiqueEmbroideryCurrentPlusDay => CalcCurrentPlus(AntiqueEmbroideryCurrentAmount, AntiqueEmbroideryPerDay);
    public long AntiqueEmbroideryCurrentPlus47Days => CalcCurrentPlus(AntiqueEmbroideryCurrentAmount, AntiqueEmbroideryFor47Days);
    public long AntiqueEmbroideryCurrentPlusNDays => CalcCurrentPlus(AntiqueEmbroideryCurrentAmount, AntiqueEmbroideryForNDays);

    public long AntiquePorcelainPerDay => CalcPerDay(AntiquePorcelainProductionPerMinute);
    public long AntiquePorcelainFor47Days => CalcFor47Days(AntiquePorcelainPerDay);
    public long AntiquePorcelainForNDays => CalcForNDays(AntiquePorcelainDaysCount, AntiquePorcelainPerDay);
    public long AntiquePorcelainCurrentPlusDay => CalcCurrentPlus(AntiquePorcelainCurrentAmount, AntiquePorcelainPerDay);
    public long AntiquePorcelainCurrentPlus47Days => CalcCurrentPlus(AntiquePorcelainCurrentAmount, AntiquePorcelainFor47Days);
    public long AntiquePorcelainCurrentPlusNDays => CalcCurrentPlus(AntiquePorcelainCurrentAmount, AntiquePorcelainForNDays);

    public long AntiqueScrollPerDay => CalcPerDay(AntiqueScrollProductionPerMinute);
    public long AntiqueScrollFor47Days => CalcFor47Days(AntiqueScrollPerDay);
    public long AntiqueScrollForNDays => CalcForNDays(AntiqueScrollDaysCount, AntiqueScrollPerDay);
    public long AntiqueScrollCurrentPlusDay => CalcCurrentPlus(AntiqueScrollCurrentAmount, AntiqueScrollPerDay);
    public long AntiqueScrollCurrentPlus47Days => CalcCurrentPlus(AntiqueScrollCurrentAmount, AntiqueScrollFor47Days);
    public long AntiqueScrollCurrentPlusNDays => CalcCurrentPlus(AntiqueScrollCurrentAmount, AntiqueScrollForNDays);

    public long AntiqueJadeDecorationPerDay => CalcPerDay(AntiqueJadeDecorationProductionPerMinute);
    public long AntiqueJadeDecorationFor47Days => CalcFor47Days(AntiqueJadeDecorationPerDay);
    public long AntiqueJadeDecorationForNDays => CalcForNDays(AntiqueJadeDecorationDaysCount, AntiqueJadeDecorationPerDay);
    public long AntiqueJadeDecorationCurrentPlusDay => CalcCurrentPlus(AntiqueJadeDecorationCurrentAmount, AntiqueJadeDecorationPerDay);
    public long AntiqueJadeDecorationCurrentPlus47Days => CalcCurrentPlus(AntiqueJadeDecorationCurrentAmount, AntiqueJadeDecorationFor47Days);
    public long AntiqueJadeDecorationCurrentPlusNDays => CalcCurrentPlus(AntiqueJadeDecorationCurrentAmount, AntiqueJadeDecorationForNDays);

    public async Task OnGetAsync()
    {
        SetDefaults();
        await LoadHistory();
    }

    public async Task<IActionResult> OnPostCalculateClosenessAsync()
    {
        NormalizeValues();
        RebuildDeepFeelingsIfNeeded();
        await LoadHistory();
        return Page();
    }

    public async Task<IActionResult> OnPostCalculatePhysicalAsync()
    {
        NormalizeValues();
        RebuildDeepFeelingsIfNeeded();
        await LoadHistory();
        return Page();
    }

    public async Task<IActionResult> OnPostCalculateEstateBuildingAsync()
    {
        NormalizeValues();
        RebuildDeepFeelingsIfNeeded();
        await LoadHistory();
        return Page();
    }

    public async Task<IActionResult> OnPostCalculateDeepFeelingsAsync()
    {
        NormalizeValues();
        ShowDeepFeelingsResults = true;
        RebuildDeepFeelingsIfNeeded();
        await LoadHistory();
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        var userId = _userManager.GetUserId(User)!;
        Result = _calculator.Calculate(CurrentPoints, TargetPoints, Attempts);

        _db.ClosenessCalculations.Add(new ClosenessCalculation
        {
            UserId = userId,
            CurrentPoints = CurrentPoints,
            TargetPoints = TargetPoints,
            Attempts = Attempts,
            Result = Result ?? 0
        });

        await _db.SaveChangesAsync();
        SavedMessage = "Результат сохранён.";
        NormalizeValues();
        await LoadHistory();
        return Page();
    }

    private void RebuildDeepFeelingsIfNeeded()
    {
        if (!ShowDeepFeelingsResults && !HasDeepFeelingsInput())
        {
            return;
        }

        TheaterDeepResult = BuildDeepFeelingSection(
            new List<int>
            {
                TheaterWhipDeepLevel,
                TheaterFanDeepLevel,
                TheaterHatDeepLevel,
                TheaterMaskDeepLevel
            },
            new List<long>
            {
                TheaterWhipSpendResource,
                TheaterFanSpendResource,
                TheaterHatSpendResource,
                TheaterMaskSpendResource
            },
            new List<long>
            {
                TheaterWhipBonusLevel,
                TheaterFanBonusLevel,
                TheaterHatBonusLevel,
                TheaterMaskBonusLevel
            });

        AntiqueDeepResult = BuildDeepFeelingSection(
            new List<int>
            {
                AntiqueEmbroideryDeepLevel,
                AntiquePorcelainDeepLevel,
                AntiqueScrollDeepLevel,
                AntiqueJadeDecorationDeepLevel
            },
            new List<long>
            {
                AntiqueEmbroiderySpendResource,
                AntiquePorcelainSpendResource,
                AntiqueScrollSpendResource,
                AntiqueJadeDecorationSpendResource
            },
            new List<long>
            {
                AntiqueEmbroideryBonusLevel,
                AntiquePorcelainBonusLevel,
                AntiqueScrollBonusLevel,
                AntiqueJadeDecorationBonusLevel
            });

        ShowDeepFeelingsResults = true;
    }

    private bool HasDeepFeelingsInput()
    {
        return
            TheaterWhipDeepLevel > 0 || TheaterFanDeepLevel > 0 || TheaterHatDeepLevel > 0 || TheaterMaskDeepLevel > 0 ||
            AntiqueEmbroideryDeepLevel > 0 || AntiquePorcelainDeepLevel > 0 || AntiqueScrollDeepLevel > 0 || AntiqueJadeDecorationDeepLevel > 0 ||
            TheaterWhipSpendResource > 0 || TheaterFanSpendResource > 0 || TheaterHatSpendResource > 0 || TheaterMaskSpendResource > 0 ||
            AntiqueEmbroiderySpendResource > 0 || AntiquePorcelainSpendResource > 0 || AntiqueScrollSpendResource > 0 || AntiqueJadeDecorationSpendResource > 0 ||
            TheaterWhipBonusLevel > 0 || TheaterFanBonusLevel > 0 || TheaterHatBonusLevel > 0 || TheaterMaskBonusLevel > 0 ||
            AntiqueEmbroideryBonusLevel > 0 || AntiquePorcelainBonusLevel > 0 || AntiqueScrollBonusLevel > 0 || AntiqueJadeDecorationBonusLevel > 0;
    }

    private DeepFeelingSectionResult BuildDeepFeelingSection(
        List<int> currentLevels,
        List<long> spendResources,
        List<long> closeFriendsCounts)
    {
        var result = new DeepFeelingSectionResult();
        var negativeCounts = new int[4];
        var hideColumn = new bool[4];

        for (var level = 1; level <= LevelCosts.Count; level++)
        {
            var row = new DeepFeelingRow
            {
                LevelNumber = level
            };

            for (var column = 0; column < 4; column++)
            {
                if (hideColumn[column])
                {
                    row.Cells.Add(new DeepFeelingCell
                    {
                        IsHidden = true
                    });
                    continue;
                }

                var currentLevel = currentLevels[column];
                var spendResource = spendResources[column];

                if (level <= currentLevel)
                {
                    row.Cells.Add(new DeepFeelingCell
                    {
                        IsPassed = true
                    });
                    continue;
                }

                var value = CalcDeepFeelingValue(currentLevel, spendResource, level);

                if (value > 0)
                {
                    result.TotalLevels[column]++;
                }

                if (value < 0)
                {
                    negativeCounts[column]++;

                    row.Cells.Add(new DeepFeelingCell
                    {
                        Value = value
                    });

                    if (negativeCounts[column] >= 2)
                    {
                        hideColumn[column] = true;
                    }

                    continue;
                }

                row.Cells.Add(new DeepFeelingCell
                {
                    Value = value
                });
            }

            if (row.ShouldRender)
            {
                result.Rows.Add(row);
            }
        }

        for (var column = 0; column < 4; column++)
        {
            result.ClosenessValues[column] = 1500L * result.TotalLevels[column] * closeFriendsCounts[column];
        }

        return result;
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

        TheaterWhipProductionPerMinute = 0;
        TheaterWhipCurrentAmount = 0;
        TheaterWhipDaysCount = 0;

        TheaterFanProductionPerMinute = 0;
        TheaterFanCurrentAmount = 0;
        TheaterFanDaysCount = 0;

        TheaterHatProductionPerMinute = 0;
        TheaterHatCurrentAmount = 0;
        TheaterHatDaysCount = 0;

        TheaterMaskProductionPerMinute = 0;
        TheaterMaskCurrentAmount = 0;
        TheaterMaskDaysCount = 0;

        AntiqueEmbroideryProductionPerMinute = 0;
        AntiqueEmbroideryCurrentAmount = 0;
        AntiqueEmbroideryDaysCount = 0;

        AntiquePorcelainProductionPerMinute = 0;
        AntiquePorcelainCurrentAmount = 0;
        AntiquePorcelainDaysCount = 0;

        AntiqueScrollProductionPerMinute = 0;
        AntiqueScrollCurrentAmount = 0;
        AntiqueScrollDaysCount = 0;

        AntiqueJadeDecorationProductionPerMinute = 0;
        AntiqueJadeDecorationCurrentAmount = 0;
        AntiqueJadeDecorationDaysCount = 0;

        TheaterWhipDeepLevel = 0;
        TheaterFanDeepLevel = 0;
        TheaterHatDeepLevel = 0;
        TheaterMaskDeepLevel = 0;

        TheaterWhipSpendResource = 0;
        TheaterFanSpendResource = 0;
        TheaterHatSpendResource = 0;
        TheaterMaskSpendResource = 0;

        TheaterWhipBonusLevel = 0;
        TheaterFanBonusLevel = 0;
        TheaterHatBonusLevel = 0;
        TheaterMaskBonusLevel = 0;

        AntiqueEmbroideryDeepLevel = 0;
        AntiquePorcelainDeepLevel = 0;
        AntiqueScrollDeepLevel = 0;
        AntiqueJadeDecorationDeepLevel = 0;

        AntiqueEmbroideryBonusLevel = 0;
        AntiquePorcelainBonusLevel = 0;
        AntiqueScrollBonusLevel = 0;
        AntiqueJadeDecorationBonusLevel = 0;

        AntiqueEmbroiderySpendResource = 0;
        AntiquePorcelainSpendResource = 0;
        AntiqueScrollSpendResource = 0;
        AntiqueJadeDecorationSpendResource = 0;

        ShowDeepFeelingsResults = false;
        TheaterDeepResult = new DeepFeelingSectionResult();
        AntiqueDeepResult = new DeepFeelingSectionResult();
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

        TheaterWhipProductionPerMinute = EnsureNotNegative(TheaterWhipProductionPerMinute);
        TheaterWhipCurrentAmount = EnsureNotNegative(TheaterWhipCurrentAmount);
        TheaterWhipDaysCount = EnsureNotNegative(TheaterWhipDaysCount);

        TheaterFanProductionPerMinute = EnsureNotNegative(TheaterFanProductionPerMinute);
        TheaterFanCurrentAmount = EnsureNotNegative(TheaterFanCurrentAmount);
        TheaterFanDaysCount = EnsureNotNegative(TheaterFanDaysCount);

        TheaterHatProductionPerMinute = EnsureNotNegative(TheaterHatProductionPerMinute);
        TheaterHatCurrentAmount = EnsureNotNegative(TheaterHatCurrentAmount);
        TheaterHatDaysCount = EnsureNotNegative(TheaterHatDaysCount);

        TheaterMaskProductionPerMinute = EnsureNotNegative(TheaterMaskProductionPerMinute);
        TheaterMaskCurrentAmount = EnsureNotNegative(TheaterMaskCurrentAmount);
        TheaterMaskDaysCount = EnsureNotNegative(TheaterMaskDaysCount);

        AntiqueEmbroideryProductionPerMinute = EnsureNotNegative(AntiqueEmbroideryProductionPerMinute);
        AntiqueEmbroideryCurrentAmount = EnsureNotNegative(AntiqueEmbroideryCurrentAmount);
        AntiqueEmbroideryDaysCount = EnsureNotNegative(AntiqueEmbroideryDaysCount);

        AntiquePorcelainProductionPerMinute = EnsureNotNegative(AntiquePorcelainProductionPerMinute);
        AntiquePorcelainCurrentAmount = EnsureNotNegative(AntiquePorcelainCurrentAmount);
        AntiquePorcelainDaysCount = EnsureNotNegative(AntiquePorcelainDaysCount);

        AntiqueScrollProductionPerMinute = EnsureNotNegative(AntiqueScrollProductionPerMinute);
        AntiqueScrollCurrentAmount = EnsureNotNegative(AntiqueScrollCurrentAmount);
        AntiqueScrollDaysCount = EnsureNotNegative(AntiqueScrollDaysCount);

        AntiqueJadeDecorationProductionPerMinute = EnsureNotNegative(AntiqueJadeDecorationProductionPerMinute);
        AntiqueJadeDecorationCurrentAmount = EnsureNotNegative(AntiqueJadeDecorationCurrentAmount);
        AntiqueJadeDecorationDaysCount = EnsureNotNegative(AntiqueJadeDecorationDaysCount);

        TheaterWhipDeepLevel = EnsureNotNegative(TheaterWhipDeepLevel);
        TheaterFanDeepLevel = EnsureNotNegative(TheaterFanDeepLevel);
        TheaterHatDeepLevel = EnsureNotNegative(TheaterHatDeepLevel);
        TheaterMaskDeepLevel = EnsureNotNegative(TheaterMaskDeepLevel);

        AntiqueEmbroideryDeepLevel = EnsureNotNegative(AntiqueEmbroideryDeepLevel);
        AntiquePorcelainDeepLevel = EnsureNotNegative(AntiquePorcelainDeepLevel);
        AntiqueScrollDeepLevel = EnsureNotNegative(AntiqueScrollDeepLevel);
        AntiqueJadeDecorationDeepLevel = EnsureNotNegative(AntiqueJadeDecorationDeepLevel);

        TheaterWhipSpendResource = EnsureNotNegative(TheaterWhipSpendResource);
        TheaterFanSpendResource = EnsureNotNegative(TheaterFanSpendResource);
        TheaterHatSpendResource = EnsureNotNegative(TheaterHatSpendResource);
        TheaterMaskSpendResource = EnsureNotNegative(TheaterMaskSpendResource);

        TheaterWhipBonusLevel = EnsureNotNegative(TheaterWhipBonusLevel);
        TheaterFanBonusLevel = EnsureNotNegative(TheaterFanBonusLevel);
        TheaterHatBonusLevel = EnsureNotNegative(TheaterHatBonusLevel);
        TheaterMaskBonusLevel = EnsureNotNegative(TheaterMaskBonusLevel);

        AntiqueEmbroideryBonusLevel = EnsureNotNegative(AntiqueEmbroideryBonusLevel);
        AntiquePorcelainBonusLevel = EnsureNotNegative(AntiquePorcelainBonusLevel);
        AntiqueScrollBonusLevel = EnsureNotNegative(AntiqueScrollBonusLevel);
        AntiqueJadeDecorationBonusLevel = EnsureNotNegative(AntiqueJadeDecorationBonusLevel);

        AntiqueEmbroiderySpendResource = EnsureNotNegative(AntiqueEmbroiderySpendResource);
        AntiquePorcelainSpendResource = EnsureNotNegative(AntiquePorcelainSpendResource);
        AntiqueScrollSpendResource = EnsureNotNegative(AntiqueScrollSpendResource);
        AntiqueJadeDecorationSpendResource = EnsureNotNegative(AntiqueJadeDecorationSpendResource);
    }

    private static int EnsureNotNegative(int value)
    {
        return value < 0 ? 0 : value;
    }

    private static long EnsureNotNegative(long value)
    {
        return value < 0 ? 0 : value;
    }

    private static long CalcPerDay(long productionPerMinute)
    {
        return productionPerMinute * MinutesPerDay;
    }

    private static long CalcFor47Days(long perDay)
    {
        return perDay * CircleDays;
    }

    private static long CalcForNDays(long daysCount, long perDay)
    {
        return daysCount * perDay;
    }

    private static long CalcCurrentPlus(long currentAmount, long resourceAmount)
    {
        return currentAmount + resourceAmount;
    }

    private long CalcDeepFeelingValue(int currentLevel, long spendResource, int targetLevel)
    {
        long totalCost = 0;

        for (int i = currentLevel; i < targetLevel; i++)
        {
            totalCost += LevelCosts[i];
        }

        return CalcDeepFeelingResult(spendResource, totalCost);
    }

    private static long CalcDeepFeelingResult(long resourceForSpend, long totalCost)
    {
        return resourceForSpend - totalCost;
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