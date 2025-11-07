using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Producer.Application.Config;
using Producer.Application.Interfaces;
using Producer.Application.Services;
using Producer.Infrastructure.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer.Startup
{
    public static class Startup
    {
        public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.Configure<ProducerSettings>(
                context.Configuration.GetSection("ProducerSettings"));

            services.Configure<TelemetrySettings>(
                context.Configuration.GetSection("TelemetrySettings"));

            services.AddSingleton<IMetadataWriter, MetadataWriter>();
            services.AddTransient<IFileWriterService, FileWriterCoordinator>();
            services.AddTransient<TelemetryGenerator>();
            services.AddHostedService<ProducerManager>();

        }
    }
}
