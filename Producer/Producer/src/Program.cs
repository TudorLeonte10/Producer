using Microsoft.Extensions.Configuration;
using Producer.src.Models;
using Producer.src.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Producer.src.Utils;


var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var inboxPath = config["Paths:Inbox"] ?? "C:\\Producer\\inbox";
bool useCompression = bool.Parse(config["ProducerSettings:UseCompression"] ?? "false");
int maxFileSizeMb = int.Parse(config["ProducerSettings:MaxFileSizeMb"] ?? "10");
int rotationSeconds = int.Parse(config["ProducerSettings:RotationSeconds"] ?? "30");
int backpressureThreshold = int.Parse(config["ProducerSettings:BackpressureThreshold"] ?? "100");
double faultProbability = double.Parse(config["ProducerSettings:FaultInjectionProbability"] ?? "0.03");

Directory.CreateDirectory(inboxPath);

var writer = new FileWriterService(
    inboxPath,
    useCompression,
    maxFileSizeMb,
    rotationSeconds,
    backpressureThreshold
);

var generator = new TelemetryGenerator();
var vehicleGenerator = new GenerateRandomVehicle();

Console.WriteLine($"Producing telemetry files to: {inboxPath}");
Console.WriteLine("Type 'exit' and press Enter to stop...\n");

bool running = true;

var produceTask = Task.Run(async () =>
{
    while (running)
    {
        try
        {
            string vehicleId = vehicleGenerator.GetRandomVehicle();
            var record = generator.GenerateTelemetry(vehicleId);
            await writer.WriteAsync(record, record.VehicleId);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    await writer.CloseAsync();
    Console.WriteLine("Producer stopped gracefully.");
});


while (true)
{
    string? input = Console.ReadLine();
    if (input?.Trim().ToLower() == "exit")
    {
        running = false;
        break;
    }
}

await produceTask;