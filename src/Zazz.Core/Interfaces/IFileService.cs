﻿using System.IO;
using System.Threading.Tasks;

namespace Zazz.Core.Interfaces
{
    public interface IFileService
    {
        string RemoveFileNameFromPath(string path);

        void CreateDirIfNotExists(string path);

        Task SaveFileAsync(string path, Stream data);

        Task SaveFileAsync(string path, byte[] data);

        void RemoveFile(string path);
    }
}