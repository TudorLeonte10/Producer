using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Producer.Application.Config;
using Producer.Application.Interfaces;
using Producer.Infrastructure.FileSystem;
using Producer.Workers;
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

            services.AddSingleton<IMetadataWriter, MetadataWriter>();

            services.AddHostedService<ProducerManager>();
        }
    }
}
