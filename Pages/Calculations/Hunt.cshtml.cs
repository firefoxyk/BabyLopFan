using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GuildApp.Pages.Calculations;

[Authorize]
public class HuntModel : PageModel
{
    // -------------------------------------------------------
    // Статический справочник локаций — БД не используется
    // -------------------------------------------------------
    public static readonly List<LocationInfo> AllLocations = new()
    {
        new("Предместья",        5,      56_600,      "Кролик"),
        new("Пригород",          10,     277_700,     "Кролик"),
        new("Окраина",           23,     638_800,     "Оленина"),
        new("Равнины",           47,     1_300_000,   "Баранина, Ягнёнок, Рёбрышки"),
        new("Прерии",            70,     1_900_000,   "Грудинка, говядина"),
        new("Болота",            103,    2_800_000,   "Оленина"),
        new("Холмы",             160,    4_400_000,   "Баранина, Ягнёнок, Рёбрышки"),
        new("Джунгли",           418,    11_600_000,  ""),
        new("Горы",              1_100,  30_500_000,  "Грудинка, говядина"),
        new("Лес",               1_700,  47_200_000,  ""),
        new("Пустыня",           2_410,  67_500_000,  ""),
        new("Снежные горы",      2_410,  67_500_000,  ""),
        new("Север",             2_410,  67_500_000,  ""),
        new("Пустошь",           3_000,  112_200_000, ""),
        new("Вулканы",           3_000,  112_200_000, ""),
        new("Высокогорье",       3_000,  112_200_000, ""),
        new("Ледники",           3_500,  180_500_000, ""),
        new("Кустарники",        56,     1_300_000,   "Курица"),
        new("Горный лес",        142,    3_300_000,   "Свинина"),
        new("Чаща",              270,    6_200_000,   ""),
        new("Снежный хребет",    660,    15_200_000,  ""),
        new("Снежный пик",       1_680,  38_800_000,  ""),
        new("Ядовитое болото",   4_080,  94_400_000,  ""),
        new("Обветренные скалы", 8_400,  194_400_000, ""),
        new("Туманный лес",      14_400, 333_300_000, ""),
        new("Песчаное море",     23_280, 538_800_000, ""),
        new("Огненная гора",     36_720, 850_000_000, ""),
    };

    // CSS-классы цветов для особых локаций
    public static readonly Dictionary<string, string> LocationCssClass = new()
    {
        { "Кустарники",        "loc-kustarn"   },
        { "Горный лес",        "loc-gorn-les"  },
        { "Чаща",              "loc-chasha"    },
        { "Снежный хребет",    "loc-sn-hrebet" },
        { "Снежный пик",       "loc-sn-pik"    },
        { "Ядовитое болото",   "loc-yad"       },
        { "Обветренные скалы", "loc-obvet"     },
        { "Туманный лес",      "loc-tuman"     },
        { "Песчаное море",     "loc-pesch"     },
        { "Огненная гора",     "loc-ogni"      },
    };

    // -------------------------------------------------------
    // Данные формы
    // -------------------------------------------------------

    // Текущее влияние игрока
    [BindProperty]
    public long MyInfluence { get; set; }

    // Активная колонка для автоподбора (по умолчанию Avg1514)
    [BindProperty]
    public string ActiveColumn { get; set; } = "Avg1514";

    // Имена локаций, отмеченных чекбоксами
    [BindProperty]
    public List<string> SelectedLocations { get; set; } = new();

    // -------------------------------------------------------
    // Результат — строки таблицы (все локации с расчётами)
    // -------------------------------------------------------
    public List<HuntRow> Rows { get; set; } = new();

    // -------------------------------------------------------
    // Итоги по отмеченным строкам
    // -------------------------------------------------------
    public long TotalCoins { get; set; }
    public long TotalCrit15 { get; set; }
    public long TotalAvg1514 { get; set; }
    public long TotalCrit14 { get; set; }
    public long TotalAvg1413 { get; set; }
    public long TotalCrit13 { get; set; }

    // -------------------------------------------------------
    // GET: открываем страницу с пустым состоянием
    // -------------------------------------------------------
    public void OnGet()
    {
        BuildRows(new HashSet<string>());
    }

    // -------------------------------------------------------
    // POST "Подобрать автоматически":
    // запускаем жадный алгоритм, обновляем чекбоксы
    // -------------------------------------------------------
    public IActionResult OnPostAutoSelect()
    {
        // Строим все строки с расчётами
        BuildRows(new HashSet<string>());

        // Жадный автоподбор по активной колонке
        SelectedLocations = AutoSelect(Rows, MyInfluence, ActiveColumn);

        // Перестраиваем строки с учётом нового выбора
        BuildRows(SelectedLocations.ToHashSet());
        CalcTotals();
        return Page();
    }

    // -------------------------------------------------------
    // POST "Пересчитать по моему выбору":
    // берём чекбоксы как есть, только пересчитываем итоги
    // -------------------------------------------------------
    public IActionResult OnPostRecalc()
    {
        BuildRows(SelectedLocations.ToHashSet());
        CalcTotals();
        return Page();
    }

    // -------------------------------------------------------
    // Строим список строк таблицы
    // -------------------------------------------------------
    private void BuildRows(HashSet<string> selected)
    {
        Rows = AllLocations
            .Select(loc => new HuntRow(loc, selected.Contains(loc.Name)))
            .ToList();
    }

    // -------------------------------------------------------
    // Считаем итоги только по отмеченным строкам
    // -------------------------------------------------------
    private void CalcTotals()
    {
        var sel = Rows.Where(r => r.IsSelected).ToList();
        TotalCoins = sel.Sum(r => r.Coins);
        TotalCrit15 = sel.Sum(r => r.Crit15);
        TotalAvg1514 = sel.Sum(r => r.Avg1514);
        TotalCrit14 = sel.Sum(r => r.Crit14);
        TotalAvg1413 = sel.Sum(r => r.Avg1413);
        TotalCrit13 = sel.Sum(r => r.Crit13);
    }

    // -------------------------------------------------------
    // Жадный автоподбор (greedy by efficiency)
    //
    // Алгоритм:
    // 1. Для каждой локации считаем «выгодность»:
    //    монеты / стоимость_по_активной_колонке
    // 2. Сортируем по убыванию выгодности
    // 3. Берём локации по порядку, пока хватает влияния
    // 4. Делаем один проход улучшений: пробуем заменить
    //    каждую выбранную локацию на невыбранную,
    //    если это даёт больше монет без превышения бюджета
    //
    // Почему жадный, а не полный перебор:
    // - локаций 27, влияние до ~850 000 000 → полный рюкзак
    //   по весу невозможен из-за огромного диапазона весов
    // - жадный по эффективности даёт практически хороший
    //   результат для задач такого типа
    // -------------------------------------------------------
    private static List<string> AutoSelect(
        List<HuntRow> rows, long budget, string col)
    {
        if (budget <= 0) return new();

        // Получаем стоимость строки по активной колонке
        long Cost(HuntRow r) => col switch
        {
            "Crit15" => r.Crit15,
            "Avg1514" => r.Avg1514,
            "Crit14" => r.Crit14,
            "Avg1413" => r.Avg1413,
            "Crit13" => r.Crit13,
            _ => r.Avg1514
        };

        // Сортируем по монеты/стоимость убыванию
        var sorted = rows
            .Where(r => Cost(r) > 0 && Cost(r) <= budget)
            .OrderByDescending(r => (double)r.Coins / Cost(r))
            .ToList();

        var chosen = new HashSet<string>();
        long remaining = budget;

        // Шаг 1: жадный набор
        foreach (var row in sorted)
        {
            long cost = Cost(row);
            if (cost <= remaining)
            {
                chosen.Add(row.Name);
                remaining -= cost;
            }
        }

        // Шаг 2: одна итерация локальных улучшений —
        // пробуем заменить выбранное на невыбранное выгоднее
        var notChosen = sorted.Where(r => !chosen.Contains(r.Name)).ToList();
        foreach (var outRow in chosen.ToList())
        {
            var outR = rows.First(r => r.Name == outRow);
            long freed = remaining + Cost(outR);

            foreach (var inR in notChosen.OrderByDescending(r => r.Coins))
            {
                if (Cost(inR) <= freed && inR.Coins > outR.Coins)
                {
                    // Замена выгодна
                    chosen.Remove(outRow);
                    chosen.Add(inR.Name);
                    notChosen.Remove(inR);
                    notChosen.Add(outR);
                    break;
                }
            }
        }

        return chosen.ToList();
    }
}

// -------------------------------------------------------
// Данные одной локации из справочника
// -------------------------------------------------------
public record LocationInfo(
    string Name,
    long Coins,
    long RecInfluence,
    string Meat);

// -------------------------------------------------------
// Строка таблицы с расчётами
// -------------------------------------------------------
public class HuntRow
{
    public string Name { get; }
    public long Coins { get; }
    public long RecInfluence { get; }
    public string Meat { get; }
    public bool IsSelected { get; }

    // Крит 1,5 — минимальное влияние для критического удара ×1,5
    public long Crit15 { get; }
    // Среднее между Крит1,5 и Крит1,4
    public long Avg1514 { get; }
    // Крит 1,4
    public long Crit14 { get; }
    // Среднее между Крит1,4 и Крит1,3
    public long Avg1413 { get; }
    // Крит 1,3
    public long Crit13 { get; }

    public HuntRow(LocationInfo loc, bool isSelected)
    {
        Name = loc.Name;
        Coins = loc.Coins;
        RecInfluence = loc.RecInfluence;
        Meat = loc.Meat;
        IsSelected = isSelected;

        // Ceiling: берём минимальное целое, при котором крит ≥ РекВлияние
        Crit15 = (long)Math.Ceiling(loc.RecInfluence / 1.5);
        Crit14 = (long)Math.Ceiling(loc.RecInfluence / 1.4);
        Crit13 = (long)Math.Ceiling(loc.RecInfluence / 1.3);

        // AwayFromZero: стандартное математическое округление (как в Excel)
        Avg1514 = (long)Math.Round((Crit15 + Crit14) / 2.0, MidpointRounding.AwayFromZero);
        Avg1413 = (long)Math.Round((Crit14 + Crit13) / 2.0, MidpointRounding.AwayFromZero);
    }
}