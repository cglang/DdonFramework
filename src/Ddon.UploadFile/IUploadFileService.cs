using Microsoft.AspNetCore.Http;

namespace Ddon.UploadFile
{
    public interface IUploadFileService
    {
        Task<FileDto> UploadFile(IFormFile file);

        Task<IEnumerable<FileDto>> UploadFiles(IEnumerable<IFormFile> files);
    }
}
