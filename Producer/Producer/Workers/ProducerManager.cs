using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Producer.Application.Config;
using Producer.Application.Enums;
using Producer.Application.Factories;
using Producer.Application.Interfaces;
using Producer.Application.Services;

public class ProducerManager : IHostedService
{
    private readonly ProducerSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMetadataWriter _metadataWriter;
    private CancellationTokenSource? _cts;
    private Task? _runningTask;

    public ProducerManager(
        IServiceProvider serviceProvider,
        IOptions<ProducerSettings> options,
        IMetadataWriter metadataWriter)
    {
        _serviceProvider = serviceProvider;
        _metadataWriter = metadataWriter;
        _settings = options.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        var vehicleIds = _settings.VehicleIds.Length > 0
            ? _settings.VehicleIds
            : new[] { "V001" };

        Console.WriteLine($"Starting {vehicleIds.Length} producers...");
        Console.WriteLine("Press Ctrl+C to stop.\n");

        var types = new[]
        {
            TelemetryStrategyType.Constant,
            TelemetryStrategyType.Random,
            TelemetryStrategyType.Burst
        };

        _runningTask = Task.WhenAll(vehicleIds.Select((id, index) =>
        {
            var strategy = TelemetryStrategyFactory.Create(types[index % types.Length]);
            return RunProducer(id, strategy, token);
        }));

        return Task.CompletedTask;
    }

    private async Task RunProducer(string vehicleId, ITelemetryStrategy strategy, CancellationToken token)
    {
        var generator = _serviceProvider.GetRequiredService<TelemetryGenerator>();
        var writer = _serviceProvider.GetRequiredService<IFileWriterService>();

        Console.WriteLine($"[{vehicleId}] using {strategy.Name} strategy");

        try
        {
            while (!token.IsCancellationRequested)
            {
                var record = generator.GenerateTelemetry(vehicleId);
                await writer.WriteAsync(record, vehicleId, token);
                await Task.Delay(strategy.GetNextInterval(), token);
            }
        }
        catch (TaskCanceledException) { }
        finally
        {
            await writer.CloseAsync(token);
            Console.WriteLine($"Producer for {vehicleId} stopped gracefully.");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts?.Cancel();
        if (_runningTask != null)
            await _runningTask;
        Console.WriteLine("All producers stopped.");
    }
}
