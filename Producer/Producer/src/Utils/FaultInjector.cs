using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer.src.Utils
{
    using System.Text;

    namespace Producer.src.Utils
    {
        public static class FaultInjector
        {
            private static readonly Random _rnd = new();

            public static void MaybeCorruptFile(string filePath, double probability)
            {
                if (_rnd.NextDouble() > probability)
                    return; 

                var choice = _rnd.Next(3);
                switch (choice)
                {
                    case 0:
                        TruncateFile(filePath);
                        Console.WriteLine($"FaultInjection: Truncated file {Path.GetFileName(filePath)}");
                        break;
                    case 1:
                        CorruptLastBytes(filePath, 100);
                        Console.WriteLine($"FaultInjection: Corrupted last 100 bytes of {Path.GetFileName(filePath)}");
                        break;
                    case 2:
                        DropLastLine(filePath);
                        Console.WriteLine($"FaultInjection: Dropped last line in {Path.GetFileName(filePath)}");
                        break;
                }
            }

            private static void TruncateFile(string filePath)
            {
                var fi = new FileInfo(filePath);
                if (fi.Length > 500)
                {
                    using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Write);
                    fs.SetLength(fi.Length - _rnd.Next(100, 500)); 
                }
            }

            private static void CorruptLastBytes(string filePath, int bytesToCorrupt)
            {
                var fi = new FileInfo(filePath);
                if (fi.Length <= bytesToCorrupt) return;

                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
                fs.Seek(-bytesToCorrupt, SeekOrigin.End);

                byte[] junk = new byte[bytesToCorrupt];
                _rnd.NextBytes(junk);
                fs.Write(junk, 0, junk.Length);
            }

            private static void DropLastLine(string filePath)
            {
                var lines = File.ReadAllLines(filePath).ToList();
                if (lines.Count > 0)
                {
                    lines.RemoveAt(lines.Count - 1);
                    File.WriteAllLines(filePath, lines, Encoding.UTF8);
                }
            }
        }
    }

}
