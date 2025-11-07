using Producer.Application.Interfaces;
using Producer.Domain.Entities;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Producer.Infrastructure.FileSystem
{
    public class TelemetryStreamWriter : ITelemetryStreamWriter
    {
        private FileStream _baseStream;
        private Stream _stream;
        private StreamWriter _writer;
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
            try
            {
                _baseStream = new FileStream(
                    tmpPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 128 * 1024,
                    useAsync: true
                );

                _stream = _useCompression
                    ? new GZipStream(_baseStream, CompressionLevel.Fastest, leaveOpen: false)
                    : _baseStream;

                _writer = new StreamWriter(
                    _stream,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                    bufferSize: 128 * 1024
                )
                {
                    AutoFlush = false
                };

                RecordCount = 0;
                Console.WriteLine($"Opened stream for {Path.GetFileName(tmpPath)} (compression={_useCompression})");
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"Failed to open file {tmpPath}: {ioEx.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($" Cannot open {tmpPath}: {ex.Message}");
                throw;
            }
        }

        public async Task WriteRecordAsync(TelemetryRecord record)
        {
            if (_writer == null)
                throw new InvalidOperationException("Writer not initialized — file not opened.");

            try
            {
                string json = JsonSerializer.Serialize(record);
                await _writer.WriteLineAsync(json);
                RecordCount++;

            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine($"Attempted to write to a closed stream.");
                throw;
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"Failed to write record for {record.VehicleId}: {ioEx.Message}");
                throw;
            }
        }

        public async Task CloseAsync()
        {
            await _writer.FlushAsync();
            _stream?.Flush();
            _baseStream?.Flush(true);

            _writer.Dispose();
            _stream?.Dispose();
            _baseStream?.Dispose();

        }
    }
}
