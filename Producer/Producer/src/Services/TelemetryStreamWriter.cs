using Producer.src.Models;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Producer.src.Services
{
    public class TelemetryStreamWriter
    {
        private FileStream? _baseStream;
        private Stream? _stream;
        private StreamWriter? _writer;
        private readonly bool _useCompression;

        public int RecordCount { get; private set; } = 0;
        public bool UseCompression => _useCompression;
        public FileStream? BaseStream => _baseStream;

        public TelemetryStreamWriter(bool useCompression)
        {
            _useCompression = useCompression;
        }

        public async Task OpenAsync(string tmpPath)
        {
            _baseStream = new FileStream(tmpPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 128 * 1024, useAsync: true);
            _stream = _useCompression
                ? new GZipStream(_baseStream, CompressionLevel.Fastest, leaveOpen: false)
                : _baseStream;

            _writer = new StreamWriter(_stream, new UTF8Encoding(false), 128 * 1024)
            {
                AutoFlush = false
            };
            RecordCount = 0;
        }

        public async Task WriteRecordAsync(TelemetryRecord record)
        {
            var json = JsonSerializer.Serialize(record);
            await _writer!.WriteLineAsync(json);
            RecordCount++;
        }

        public async Task CloseAsync()
        {
            if (_writer != null)
            {
                await _writer.FlushAsync();
                _stream!.Flush();
                _baseStream!.Flush(true);
                _writer.Dispose();
                _stream.Dispose();
                _baseStream.Dispose();
            }
        }
    }

}
