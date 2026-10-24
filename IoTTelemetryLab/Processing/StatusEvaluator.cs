using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Processing;

public enum SafetyStatus
{
    Normal,
    Warning,
    Critical
}

/// <summary>Визначає рівень небезпеки за типом датчика та значенням показу.</summary>
public static class StatusEvaluator
{
    /// <summary>Оцінює очищений показ через вкладені switch-вирази.</summary>
    public static SafetyStatus Evaluate(
        SensorType type, double value) => type switch
        {
            SensorType.Temperature => value switch
            {
                < 0 or > 80 => SafetyStatus.Critical,
                >= 60 and <= 80 => SafetyStatus.Warning,
                _ => SafetyStatus.Normal
            },

            // За умовою варіанта 2: понад 2.5 мм/с — підвищена вібрація, понад 5.0 — критична.
            SensorType.Vibration => value switch
            {
                > 5.0 => SafetyStatus.Critical,
                > 2.5 and <= 5.0 => SafetyStatus.Warning,
                _ => SafetyStatus.Normal
            },

            SensorType.Voltage => value switch
            {
                < 200 or > 240 => SafetyStatus.Critical,
                < 210 or > 230 => SafetyStatus.Warning,
                _ => SafetyStatus.Normal
            },

            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
}