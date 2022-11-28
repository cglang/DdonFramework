using Ddon.FileStorage.DataBase;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Ddon.FileStorage.Service
{
    public interface IFileStorageService
    {
        Task<FileEntity> SaveFileAsync(IFormFile file);

        Task<IEnumerable<FileEntity>> SaveFilesAsync(IEnumerable<IFormFile> files);

        Task<FileEntity> SaveFileAsync(Stream file, string? filename);
    }
}
