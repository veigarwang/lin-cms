﻿using IGeekFan.FreeKit.Extras.FreeSql;
using LinCms.Entities;

namespace LinCms.IRepositories
{
    public interface ILogRepository : IAuditBaseRepository<LinLog>
    {
        void Create(LinLog linlog);
    }
}
