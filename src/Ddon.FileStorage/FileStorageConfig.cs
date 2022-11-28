using System;
using System.IO;

namespace Ddon.FileStorage
{
    public class FileStorageConfig
    {
        public static string BasePath = "FileStorage";
        public static string FileStoragePath = Path.Combine(BasePath, "Files");
        public static string FileStorageFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BasePath);
        public static string DatabaseSource = Path.Combine(FileStorageFullPath, "FileStorage.db");
    }
}
