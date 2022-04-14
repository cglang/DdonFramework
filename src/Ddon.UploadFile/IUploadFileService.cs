using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ddon.UploadFile
{
    public interface IUploadFileService
    {
        Task<FileDto> UploadFile(IFormFile file);

        Task<IEnumerable<FileDto>> UploadFiles(IEnumerable<IFormFile> files);
    }
}
