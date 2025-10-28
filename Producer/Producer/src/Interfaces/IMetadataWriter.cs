﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer.src.Interfaces
{
    public interface IMetadataWriter
    {
        Task WriteMetadataAsync(string finalPath, int recordCount, bool useCompression);
    }
}
