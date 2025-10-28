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
        bool useCompression = bool.Parse(cfg["ProducerSettings:UseCompression"] ?? "true");
        int maxFileSizeMb = int.Parse(cfg["ProducerSettings:MaxFileSizeMb"] ?? "5");
        int rotationSeconds = int.Parse(cfg["ProducerSettings:RotationSeconds"] ?? "30");
        int backpressureThreshold = int.Parse(cfg["ProducerSettings:BackpressureThreshold"] ?? "100");

        services.AddSingleton<IMetadataWriter, MetadataWriter>();
        services.AddSingleton<IFileWriterService>(provider =>
            new FileWriterService(
                outputDirectory: outputDir,
                metadataWriter: provider.GetRequiredService<IMetadataWriter>(),
                maxFileSizeMb: maxFileSizeMb,
                rotationSeconds: rotationSeconds,
                backpressureThreshold: backpressureThreshold
            )
        );

        services.AddSingleton<TelemetryGenerator>();
        services.AddSingleton<GenerateRandomVehicle>();
    })
    .Build();

var writer = host.Services.GetRequiredService<IFileWriterService>();
var generator = host.Services.GetRequiredService<TelemetryGenerator>();
var vehicleGenerator = host.Services.GetRequiredService<GenerateRandomVehicle>();

Console.WriteLine("Producer started — generating telemetry data continuously.");

Console.WriteLine("Type 'exit' and press Enter to stop.\n");

bool running = true;

using var cts = new CancellationTokenSource();
var token = cts.Token;

var produceTask = Task.Run(async () =>
{
    try
    {
        while (!token.IsCancellationRequested)
        {
            string vehicleId = vehicleGenerator.GetRandomVehicle();

            var record = generator.GenerateTelemetry(vehicleId);

            await writer.WriteAsync(record, record.VehicleId);
        }
    }
    catch (IOException ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    finally
    {
        await writer.CloseAsync();
        Console.WriteLine("Producer stopped gracefully.");
    }

});

while (true)
{
    string? input = Console.ReadLine();
    if (input?.Trim().ToLower() == "exit")
    {
        cts.Cancel();
        break;
    }
}

await produceTask;
