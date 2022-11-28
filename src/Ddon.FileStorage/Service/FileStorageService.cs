using Ddon.Domain.Exceptions;
using Ddon.FileStorage.DataBase;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Ddon.FileStorage.Service
{
    public class FileStorageService : IFileStorageService
    {
        private readonly FileStorageDbContext _dbContext;

        public FileStorageService(FileStorageDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<FileEntity> SaveFileAsync(IFormFile file)
        {
            var fileEntity = await SaveFile(file);
            await _dbContext.AddAsync(fileEntity);
            return fileEntity;
        }

        public async Task<IEnumerable<FileEntity>> SaveFilesAsync(IEnumerable<IFormFile> files)
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

            return filesEntity;
        }

        public async Task<FileEntity> SaveFileAsync(Stream file, string? filename)
        {
            if (file.Length == 0)
            {
                throw new ApplicationServiceException("文件长度为0");
            }

            if (file.Length / 1024 / 1024 > 4)
            {
                throw new ApplicationServiceException("文件超过上限大小");
            }

            // 获取文件拓展名
            var ext = Path.GetExtension(filename ?? string.Empty).ToLowerInvariant();
            var path = Path.Combine("Files", $"{new Guid()}.{ext}");
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

            var fileEntity = new FileEntity
            {
                Id = new Guid().ToString(),
                Name = filename ?? string.Empty,
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
                throw new ApplicationServiceException("文件保存失败");
            }
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
