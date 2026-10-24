using System.Text;
using IoTTelemetryLab.Data;
using IoTTelemetryLab.Models;
using IoTTelemetryLab.Processing;

Console.OutputEncoding = Encoding.UTF8;

const string deviceId = "sensor-02";
const SensorType sensorType = SensorType.Vibration;

var rawData = TelemetryGenerator.GenerateRawBatch(
    deviceId,
    count: 50,
    minValue: 0,
    maxValue: 8);

var cleaned = TelemetryCleaner.RemoveInvalid(rawData).ToList();

double? maximum = TelemetryAnalytics.GetMaximum(cleaned);

int warningCount = TelemetryAnalytics.CountByStatus(
    cleaned,
    sensorType,
    SafetyStatus.Warning);

Console.WriteLine("Лабораторна робота №1");
Console.WriteLine("Варіант 2: вібрація");
Console.WriteLine($"Датчик: {deviceId}");
Console.WriteLine();

Console.WriteLine($"Отримано показів: {rawData.Count}");
Console.WriteLine($"Після очищення: {cleaned.Count}");
Console.WriteLine($"Вилучено некоректних: {rawData.Count - cleaned.Count}");

Console.WriteLine(maximum.HasValue
    ? $"Максимальна вібрація за період: {maximum.Value:F2} мм/с"
    : "Максимальна вібрація за період: немає коректних показів");

Console.WriteLine($"Кількість Warning-показів: {warningCount}");