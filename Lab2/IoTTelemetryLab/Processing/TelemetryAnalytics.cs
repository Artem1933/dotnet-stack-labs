using IoTTelemetryLab.Models;
using IoTTelemetryLab.Evaluation;

namespace IoTTelemetryLab.Processing;

/// <summary>Обчислює показники очищеної телеметрії.</summary>
public static class TelemetryAnalytics
{
    /// <summary>Знаходить максимальне значення через LINQ.</summary>
    public static double? GetMaximum(
        IEnumerable<TelemetryReading> readings)
    {
        return readings.Select(r => (double?)r.Value).Max();
    }

    /// <summary>Рахує покази заданого стану через реєстр стратегій.</summary>
    public static int CountByStatus(
        IEnumerable<TelemetryReading> readings,
        SensorType type,
        SafetyStatus target,
        SensorEvaluatorRegistry registry)
    {
        return readings.Count(r =>
            registry.Evaluate(type, r.Value) == target);
    }

    /// <summary>Обчислює середні значення повних вікон показів.</summary>
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