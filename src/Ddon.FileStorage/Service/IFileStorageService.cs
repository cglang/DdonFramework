using Ddon.FileStorage.DataBase;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace Ddon.FileStorage.Service
{
    public interface IFileStorageService
    {
        Task<FileEntity> SaveFileAsync(IFormFile file);

        Task<FileEntity> SaveFileAsync(Stream file, string? filename);
    }
}
