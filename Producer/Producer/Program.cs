using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Producer.Startup;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices(Startup.ConfigureServices)
    .RunConsoleAsync();