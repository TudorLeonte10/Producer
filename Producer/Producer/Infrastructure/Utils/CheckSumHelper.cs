using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Producer.Infrastructure.Utils
{
    public class CheckSumHelper
    {
        public static string ComputeSHA256(string filepath)
        {
            using var sha256 = SHA256.Create();
            using var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, 128 * 1024, useAsync: false);
            var hash = sha256.ComputeHash(fs);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
