using Producer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer.Application.Interfaces
{
    public interface IFileWriterService
    {
        Task WriteAsync(TelemetryRecord record, string vehicleId, CancellationToken ct = default);
        Task CloseAsync(CancellationToken ct = default);
    }
}
