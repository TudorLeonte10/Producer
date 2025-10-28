using Producer.src.Interfaces;
using Producer.src.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Producer.src.Services
{
    public class MetadataWriter : IMetadataWriter
    {
        public async Task WriteMetadataAsync(string finalPath, int recordCount, bool useCompression)
        {
            var sha = CheckSumHelper.ComputeSHA256(finalPath);
            var metaData = new
            {
                Version = "1.0",
                CreatedUTC = DateTime.UtcNow,
                RecordCount = recordCount,
                Sha256 = sha,
                Encoding = "utf-8",
                Compression = useCompression ? "gzip" : "none"
            };

            var metaPath = finalPath + ".meta.json";
            var metaJson = JsonSerializer.Serialize(metaData, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(metaPath, metaJson, Encoding.UTF8);
        }

    }
}
