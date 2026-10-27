using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Processing;

/// <summary>Обчислює аналітичні показники очищеної телеметрії.</summary>
public static class TelemetryAnalytics
{
    /// <summary>
    /// Знаходить максимум за весь період через LINQ Select і Max.
    /// Повертає null, якщо показів немає.
    /// </summary>
    public static double? GetMaximum(
        IEnumerable<TelemetryReading> readings)
    {
        return readings.Select(r => (double?)r.Value).Max();
    }

    /// <summary>
    /// Підраховує покази заданого стану через LINQ Count.
    /// Для варіанта 2 використовується стан Warning.
    /// </summary>
    public static int CountByStatus(
        IEnumerable<TelemetryReading> readings,
        SensorType type,
        SafetyStatus target)
    {
        return readings.Count(r =>
            StatusEvaluator.Evaluate(type, r.Value) == target);
    }

    /// <summary>Обчислює ковзне середнє за повними вікнами показів у часовому порядку.</summary>
    /// <param name="readings">Послідовність показів.</param>
    /// <param name="windowSize">Додатна кількість показів у вікні.</param>
    public static IEnumerable<double> MovingAverage(
        IEnumerable<TelemetryReading> readings,
        int windowSize)
    {
        var values = readings.Select(r => r.Value).ToList();

        for (int i = 0; i <= values.Count - windowSize; i++)
        {
            yield return values
                .Skip(i)
                .Take(windowSize)
                .Average();
        }
    }
}