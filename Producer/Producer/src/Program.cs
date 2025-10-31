using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Producer.src.Interfaces;
using Producer.src.Services;
using Producer.src.Utils;
using Producer.src.Models;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        var cfg = context.Configuration;
        var outputDir = cfg["Paths:Inbox"] ?? @"C:\Producer\inbox";
        int maxFileSizeMb = int.Parse(cfg["ProducerSettings:MaxFileSizeMb"] ?? "5");
        int rotationSeconds = int.Parse(cfg["ProducerSettings:RotationSeconds"] ?? "30");
        int backpressureThreshold = int.Parse(cfg["ProducerSettings:BackpressureThreshold"] ?? "100");

        services.AddSingleton<IMetadataWriter, MetadataWriter>();
    })
    .Build();

var cfg = host.Services.GetRequiredService<IConfiguration>();
var metadataWriter = host.Services.GetRequiredService<IMetadataWriter>();
string outputDir = cfg["Paths:Inbox"] ?? @"C:\Producer\inbox";
Directory.CreateDirectory(outputDir);

using var cts = new CancellationTokenSource();
var token = cts.Token;

var vehicleIds = new[] { "V001", "V002", "V003", "V004", "V005", "V006", "V007", "V008", "V009", "V0010" };

Console.WriteLine($"Starting {vehicleIds.Length} producers...");
Console.WriteLine("Type 'exit' and press Enter to stop.\n");

var producerTasks = vehicleIds.Select(vehicleId => Task.Run(async () =>
{
    var generator = new TelemetryGenerator(); 
    var writer = new FileWriterService(
        outputDirectory: outputDir,
        metadataWriter: metadataWriter,
        maxFileSizeMb: 5,
        rotationSeconds: 30,
        backpressureThreshold: 100
    );

    try
    {
        while (!token.IsCancellationRequested)
        {
            var record = generator.GenerateTelemetry(vehicleId);
            await writer.WriteAsync(record, vehicleId, token);
        }
    }
    catch (TaskCanceledException) 
    {
        Console.WriteLine("Task cancelled");
    }
    catch (IOException ex)
    {
        Console.WriteLine($"Error in {vehicleId}: {ex.Message}");
    }
    finally
    {
        await writer.CloseAsync(token);
        Console.WriteLine($"Producer for {vehicleId} stopped gracefully.");
    }
}, token)).ToArray();

while (true)
{
    string? input = Console.ReadLine();
    if (input?.Trim().ToLower() == "exit")
    {
        cts.Cancel();
        break;
    }
}

await Task.WhenAll(producerTasks);

Console.WriteLine("All producers stopped.");
