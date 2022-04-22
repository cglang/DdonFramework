using Ddon.Application.Service;
using Ddon.Core.Services.LazyService;
using Ddon.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Ddon.UploadFile
{
    public class UploadFileService : ApplicationService<Guid>, IUploadFileService
    {
        private readonly UploadFileDbContext _dbContext;

        public UploadFileService(ILazyServiceProvider lazyServiceProvider, UploadFileDbContext dbContext) : base(lazyServiceProvider)
        {
            _dbContext = dbContext;
        }

        public async Task<FileDto> UploadFile(IFormFile file)
        {
            var fileEntity = await SaveFile(file);
            await _dbContext.AddAsync(fileEntity);
            return FileDto.Build(fileEntity);
        }

        public async Task<IEnumerable<FileDto>> UploadFiles(IEnumerable<IFormFile> files)
        {
            List<FileEntity> filesEntity = new();
            try
            {
                foreach (var file in files)
                {
                    var fileEntity = await SaveFile(file);
                    filesEntity.Add(fileEntity);
                }
                await _dbContext.AddAsync(filesEntity);
            }
            catch (ApplicationServiceException e)
            {
                throw e;
            }
            catch
            {
                throw new ApplicationServiceException("文件上传失败");
            }

            return FileDto.Build(filesEntity);
        }

        private static async Task<FileEntity> SaveFile(IFormFile file)
        {
            if (file.Length == 0)
            {
                throw new ApplicationServiceException("文件错误");
            }

            if (file.Length / 1024 / 1024 > 4)
            {
                throw new ApplicationServiceException("文件超过上限大小");
            }

            // 获取文件拓展名
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var path = Path.Combine("Files", $"{new Guid()}.{ext}");
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

            var fileEntity = new FileEntity
            {
                Id = new Guid().ToString(),
                Name = file.FileName,
                Path = path,
                FullPath = fullPath,
                Extension = ext,
            };

            try
            {
                using var stream = File.Create(fileEntity.FullPath);
                await file.CopyToAsync(stream);
                return fileEntity;
            }
            catch
            {
                throw new ApplicationServiceException("文件上传失败");
            }
        }

    }
}
