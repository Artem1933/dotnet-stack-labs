using System.Text;
using IoTTelemetryLab.Data;
using IoTTelemetryLab.Evaluation;
using IoTTelemetryLab.Models;
using IoTTelemetryLab.Monitoring;
using IoTTelemetryLab.Monitoring.Subscribers;
using IoTTelemetryLab.Processing;

Console.OutputEncoding = Encoding.UTF8;

const string deviceId = "sensor-02";
const SensorType sensorType = SensorType.Pressure;

bool useRandomData = true;

List<TelemetryReading> rawData;

if (useRandomData)
{
    rawData = TelemetryGenerator.GenerateRawBatch(
        deviceId,
        count: 50,
        minValue: 90,
        maxValue: 115);
}
else
{
    double[] values =
    {
        90, 92, 94, 95, 97.99, 98, 100, 105,
        105.01, 110, 111, 112, 115, 116, -999, double.NaN
    };

    var startTime = DateTime.UtcNow;

    rawData = values.Select((value, index) =>
        new TelemetryReading(
            deviceId,
            value,
            startTime.AddSeconds(index * 5))).ToList();
}

var cleaned = TelemetryCleaner.RemoveInvalid(rawData).ToList();
var registry = EvaluatorSetup.CreateDefaultRegistry();

registry.Register(sensorType, new PressureEvaluator());

var classStatuses = cleaned
    .Select(r => registry.Evaluate(sensorType, r.Value))
    .ToList();

registry.Register(sensorType, value => value switch
{
    < 95 or > 110 => SafetyStatus.Critical,
    < 98 or > 105 => SafetyStatus.Warning,
    _ => SafetyStatus.Normal
});

var funcStatuses = cleaned
    .Select(r => registry.Evaluate(sensorType, r.Value))
    .ToList();

bool sameResults = classStatuses.SequenceEqual(funcStatuses);

Console.WriteLine("Лабораторна робота №2");
Console.WriteLine("Варіант 2: тиск, кПа");
Console.WriteLine(
    useRandomData ? "Випадкові дані" : "Контрольний набір");

Console.WriteLine(
    $"Клас і Func дають однакові результати: {sameResults}");

if (!sameResults)
{
    throw new InvalidOperationException(
        "Оцінювачі повернули різні стани.");
}

if (!useRandomData)
{
    Console.WriteLine("\nЗначення | Клас | Func");

    for (int i = 0; i < cleaned.Count; i++)
    {
        Console.WriteLine(
            $"{cleaned[i].Value,8:F2} | " +
            $"{classStatuses[i],8} | {funcStatuses[i]}");
    }
}

var monitor = new TelemetryMonitor(registry, sensorType);
var counter = new CriticalCounterSubscriber();
var recent = new RecentCriticalSubscriber();

monitor.OnStatusEvaluated +=
    ConsoleAlertSubscriber.OnStatusEvaluated;

monitor.OnStatusEvaluated += counter.OnStatusEvaluated;
monitor.OnStatusEvaluated += recent.OnStatusEvaluated;

Console.WriteLine("\nСповіщення:");

foreach (var reading in cleaned)
{
    monitor.Process(reading);
}

int criticalByLinq = TelemetryAnalytics.CountByStatus(
    cleaned,
    sensorType,
    SafetyStatus.Critical,
    registry);

int warningCount = TelemetryAnalytics.CountByStatus(
    cleaned,
    sensorType,
    SafetyStatus.Warning,
    registry);

Console.WriteLine($"\nОтримано показів: {rawData.Count}");
Console.WriteLine($"Після очищення: {cleaned.Count}");

Console.WriteLine(
    $"Вилучено некоректних: {rawData.Count - cleaned.Count}");

Console.WriteLine($"Warning: {warningCount}");
Console.WriteLine($"Critical через подію: {counter.CriticalCount}");
Console.WriteLine($"Critical через LINQ: {criticalByLinq}");
Console.WriteLine($"У журналі: {recent.LastReadings.Count}");

Console.WriteLine(
    "\nОстанні критичні покази (від давнішого до новішого):");

foreach (var reading in recent.LastReadings)
{
    Console.WriteLine(
        $"{reading.Timestamp:HH:mm:ss} UTC | " +
        $"{reading.DeviceId} | {reading.Value:F2} кПа");
}

var expectedRecent = cleaned
    .Where(r =>
        registry.Evaluate(sensorType, r.Value) == SafetyStatus.Critical)
    .TakeLast(5)
    .ToList();

bool journalCorrect =
    recent.LastReadings.SequenceEqual(expectedRecent);

Console.WriteLine(
    $"Журнал містить саме останні 5 або менше: {journalCorrect}");

if (counter.CriticalCount != criticalByLinq || !journalCorrect)
{
    throw new InvalidOperationException(
        "Результати підписників не збігаються з перевіркою.");
}