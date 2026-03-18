namespace GuildApp.Services;

// Все интерфейсы и реализации калькуляторов в одном файле для простоты

public interface IHuntCalculator
{
    double Calculate(double currentPoints, double targetPoints, int attempts);
}

public interface IWorldFameCalculator
{
    double Calculate(double currentPoints, double targetPoints, int attempts);
}

public interface IClosenessCalculator
{
    double Calculate(double currentPoints, double targetPoints, int attempts);
}

public interface IAssistantCalculator
{
    double Calculate(double currentPoints, double targetPoints, int attempts);
}

public interface IPowerCalculator
{
    double Calculate(double currentPoints, double targetPoints, int attempts);
}

public interface IBanquetCalculator
{
    double Calculate(double currentPoints, double targetPoints, int attempts);
}

// Заглушки — реальные формулы будут реализованы позже

public class HuntCalculator : IHuntCalculator
{
    public double Calculate(double currentPoints, double targetPoints, int attempts)
    {
        // TODO: реализовать формулу расчёта охоты
        if (attempts <= 0) return 0;
        return Math.Round((targetPoints - currentPoints) / attempts, 2);
    }
}

public class WorldFameCalculator : IWorldFameCalculator
{
    public double Calculate(double currentPoints, double targetPoints, int attempts)
    {
        // TODO: реализовать формулу расчёта всемирной славы
        if (attempts <= 0) return 0;
        return Math.Round((targetPoints - currentPoints) / attempts, 2);
    }
}

public class ClosenessCalculator : IClosenessCalculator
{
    public double Calculate(double currentPoints, double targetPoints, int attempts)
    {
        // TODO: реализовать формулу расчёта близости
        if (attempts <= 0) return 0;
        return Math.Round((targetPoints - currentPoints) / attempts, 2);
    }
}

public class AssistantCalculator : IAssistantCalculator
{
    public double Calculate(double currentPoints, double targetPoints, int attempts)
    {
        // TODO: реализовать формулу расчёта помощника
        if (attempts <= 0) return 0;
        return Math.Round((targetPoints - currentPoints) / attempts, 2);
    }
}

public class PowerCalculator : IPowerCalculator
{
    public double Calculate(double currentPoints, double targetPoints, int attempts)
    {
        // TODO: реализовать формулу расчёта власти
        if (attempts <= 0) return 0;
        return Math.Round((targetPoints - currentPoints) / attempts, 2);
    }
}

public class BanquetCalculator : IBanquetCalculator
{
    public double Calculate(double currentPoints, double targetPoints, int attempts)
    {
        // TODO: реализовать формулу расчёта банкетов
        if (attempts <= 0) return 0;
        return Math.Round((targetPoints - currentPoints) / attempts, 2);
    }
}
