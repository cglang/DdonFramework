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
        private readonly IFileStorageRepository _repository;

        public FileStorageService(IFileStorageRepository repository)
        {
            _repository = repository;
        }

        public async Task<FileEntity> SaveFileAsync(IFormFile file)
        {
            var fileEntity = BuildEntity(file, file.FileName);

            var aa = Path.GetDirectoryName(fileEntity.FullPath);
            Directory.CreateDirectory(aa!);
            using var newstream = File.OpenWrite(fileEntity.FullPath);
            await file.CopyToAsync(newstream);

            await _repository.AddAsync(fileEntity, true);

            return fileEntity;
        }

        public async Task<IEnumerable<FileEntity>> SaveFileAsync(IEnumerable<IFormFile> files)
        {
            List<FileEntity> filesEntity = new();
            try
            {
                foreach (var file in files)
                {
                    await SaveFileAsync(file);
                }
            }
            catch (ApplicationServiceException e)
            {
                throw e;
            }
            catch
            {
                throw new ApplicationServiceException("文件上传失败");
            }
            finally
            {
                await _repository.SaveChangesAsync();
            }

            return filesEntity;
        }

        public async Task<FileEntity> SaveFileAsync(Stream file, string? filename)
        {
            var fileEntity = BuildEntity(file, filename);

            var aa = Path.GetDirectoryName(fileEntity.FullPath);
            Directory.CreateDirectory(aa!);
            using var newstream = File.OpenWrite(fileEntity.FullPath);
            await file.CopyToAsync(newstream);

            await _repository.AddAsync(fileEntity, true);

            return fileEntity;
        }

        private static FileEntity BuildEntity(object obj, string? filename)
        {
            if (obj is Stream stream)
            {
                if (stream.Length == 0) throw new ApplicationServiceException("文件长度为0");

                if (stream.Length / 1024 / 1024 > 4) throw new ApplicationServiceException("文件超过上限大小");
            }
            else if (obj is IFormFile file)
            {
                if (file.Length == 0) throw new ApplicationServiceException("文件错误");

                if (file.Length / 1024 / 1024 > 4) throw new ApplicationServiceException("文件超过上限大小");
            }
            else
            {
                throw new ApplicationServiceException("参数有误");
            }


            // 文件拓展名
            var ext = Path.GetExtension(filename ?? string.Empty).ToLowerInvariant();

            // 文件存储位置
            var savePath = Path.Combine(FileStorageConfig.FileStoragePath, DateTime.UtcNow.ToString("yyyy-MM"));
            var saveFilename = $"{Guid.NewGuid()}{ext}";

            var path = Path.Combine(savePath, saveFilename);
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, savePath);
            // 文件存储位置全路径名称
            var fullName = Path.Combine(fullPath, saveFilename);

            var fileEntity = new FileEntity
            {
                Id = Guid.NewGuid().ToString(),
                Name = Path.GetFileName(filename),
                Path = path,
                FullPath = fullName,
                Extension = ext,
            };

            return fileEntity;
        }

    }
}
