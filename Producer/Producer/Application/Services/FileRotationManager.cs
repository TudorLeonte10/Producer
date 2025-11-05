using System;

namespace Producer.Application.Services
{
    public class FileRotationManager
    {
        private readonly int _maxFileSizeMb;
        private readonly TimeSpan _maxInterval;
        private DateTime _fileStart;
        private FileStream? _currentFileStream;

        public FileRotationManager(int maxFileSizeMb, TimeSpan maxInterval)
        {
            _maxFileSizeMb = maxFileSizeMb;
            _maxInterval = maxInterval;
        }

        public void SetCurrentFile(FileStream fileStream)
        {
            _currentFileStream = fileStream;
            _fileStart = DateTime.UtcNow;
        }

        public bool NeedsRotation()
        {
            if (_currentFileStream == null)
                return true;

            bool sizeExceeded = _currentFileStream.Length >= _maxFileSizeMb * 1024 * 1024;
            bool timeExceeded = DateTime.UtcNow - _fileStart >= _maxInterval;

            return sizeExceeded || timeExceeded;
        }
    }
}
