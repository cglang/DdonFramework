using System.Collections.Generic;
using Ddon.FileStorage.DataBase;

namespace Ddon.FileStorage
{
    public class FileDto
    {
        public string? Id { get; set; }

        public string? Url { get; set; }

        public static FileDto Build(FileEntity fileEntity)
        {
            return new FileDto
            {
                Id = fileEntity.Id,
                Url = fileEntity.Path
            };
        }

        public static IEnumerable<FileDto> Build(IEnumerable<FileEntity> filesEntity)
        {
            List<FileDto> filesDto = new();
            foreach (var fileEntity in filesEntity)
            {
                filesDto.Add(Build(fileEntity));
            }
            return filesDto;
        }
    }
}
