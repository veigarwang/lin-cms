﻿using FreeSql;
using LinCms.Data.Options;
using LinCms.Entities;
using LinCms.IRepositories;
using LinCms.Security;
using Microsoft.Extensions.Options;

namespace LinCms.Repositories
{
    public class FileRepository : AuditBaseRepository<LinFile>, IFileRepository
    {
        private readonly FileStorageOption _fileStorageOption;
        public FileRepository(UnitOfWorkManager unitOfWorkManager, ICurrentUser currentUser, IOptions<FileStorageOption> fileStorageOption) : base(unitOfWorkManager, currentUser)
        {
            _fileStorageOption = fileStorageOption.Value;
        }

        public string GetFileUrl(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (path.StartsWith("http") || path.StartsWith("https"))
            {
                return path;
            }

            LinFile linFile = Where(r => r.Path == path).First();
            if (linFile == null) return path;
            return linFile.Type switch
            {
                1 => _fileStorageOption.LocalFile.Host + path,
                2 => _fileStorageOption.Qiniu.Host + path,
                _ => path,
            };
        }
    }
}