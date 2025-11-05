using Producer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer.Application.Interfaces
{
    public interface ITelemetryStreamWriter
    {
        bool UseCompression { get; }
        FileStream? BaseStream { get; }
        int RecordCount { get; }

        Task OpenAsync(string tmpPath);
        Task WriteRecordAsync(TelemetryRecord record);
        Task CloseAsync();
    }
}
