namespace GuildApp.Models;

// Все сущности расчётов в одном файле для простоты

public class HuntCalculation
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public double CurrentPoints { get; set; }
    public double TargetPoints { get; set; }
    public int Attempts { get; set; }
    public double Result { get; set; }
}

public class WorldFameCalculation
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public double CurrentPoints { get; set; }
    public double TargetPoints { get; set; }
    public int Attempts { get; set; }
    public double Result { get; set; }
}

public class ClosenessCalculation
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public double CurrentPoints { get; set; }
    public double TargetPoints { get; set; }
    public int Attempts { get; set; }
    public double Result { get; set; }
}

public class AssistantCalculation
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public double CurrentPoints { get; set; }
    public double TargetPoints { get; set; }
    public int Attempts { get; set; }
    public double Result { get; set; }
}

public class PowerCalculation
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public double CurrentPoints { get; set; }
    public double TargetPoints { get; set; }
    public int Attempts { get; set; }
    public double Result { get; set; }
}

public class BanquetCalculation
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public double CurrentPoints { get; set; }
    public double TargetPoints { get; set; }
    public int Attempts { get; set; }
    public double Result { get; set; }
}
