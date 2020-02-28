﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LinCms.Application.Contracts.Cms.Permissions;
using LinCms.Core.Data;
using LinCms.Core.Entities;

namespace LinCms.Application.Cms.Permissions
{
    public interface IPermissionService
    {
        Task<bool> CheckPermissionAsync( string permission);
        Task RemovePermissions(RemovePermissionDto permissionDto);

        Task DispatchPermissions(DispatchPermissionsDto permissionDto, List<PermissionDefinition> permissionDefinition);

        Task<List<LinPermission>> GetPermissionByGroupIds(List<long> groupIds);

        List<IDictionary<string, object>> StructuringPermissions(List<LinPermission> permissions);


    }
}