using ECQRS.Commons.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManager.Core.Users;
using UserManager.Shared;

namespace UserManager.Commons.Services
{
    public class PermissionsService : IPermissionsService
    {
        private IAggregateRepository<UserItem> _repository;
        public PermissionsService(IAggregateRepository<UserItem> repository)
        {
            _repository = repository;
        }
        public bool HasPermission(Guid userId, string requiredPermission, params Guid[] ids)
        {
            var userPermissions = _repository.GetById(userId).User.Permissions;
            requiredPermission = requiredPermission.ToLowerInvariant();

            switch (requiredPermission)
            {
                case (Permissions.SysAdmin):
                    return userPermissions.Any(u =>
                        u.Name.ToLowerInvariant() == Permissions.SysAdmin);
                case (Permissions.AppsManager):
                    return userPermissions.Any(u =>
                        u.Name.ToLowerInvariant() == Permissions.SysAdmin ||
                        u.Name.ToLowerInvariant() == Permissions.AppsManager);
                case (Permissions.OrgsManager):
                    return userPermissions.Any(u =>
                        u.Name.ToLowerInvariant() == Permissions.SysAdmin ||
                        u.Name.ToLowerInvariant() == Permissions.OrgsManager);
                case (Permissions.OrgManager):
                    return userPermissions.Any(u =>
                        u.Name.ToLowerInvariant() == Permissions.SysAdmin ||
                        u.Name.ToLowerInvariant() == Permissions.OrgsManager ||
                        (ids.Length >= 1 && u.Name.ToLowerInvariant() == Permissions.OrgManager && u.DataId == ids[0]));
                case (Permissions.GroupManager):
                    return userPermissions.Any(u =>
                        u.Name.ToLowerInvariant() == Permissions.SysAdmin ||
                        u.Name.ToLowerInvariant() == Permissions.OrgsManager ||
                        (ids.Length >= 1 && u.Name.ToLowerInvariant() == Permissions.OrgManager && u.DataId == ids[0]) ||
                        (ids.Length >= 2 && u.Name.ToLowerInvariant() == Permissions.GroupManager && u.DataId == ids[1]));
                default:
                    return false;
            }
        }


        public bool CanAssign(Guid assigningId, Guid assigneeId, string permission, Guid orgId, Guid groupId)
        {
            var assigning = _repository.GetById(assigningId).User;
            var assignee = _repository.GetById(assigneeId).User;
            permission = permission.ToLowerInvariant();

            switch (permission)
            {
                case (Permissions.SysAdmin):
                    return assigning.Permissions.Any(p =>
                        p.Name.ToLowerInvariant() == Permissions.SysAdmin);
                case (Permissions.AppsManager):
                    return assigning.Permissions.Any(p =>
                        p.Name.ToLowerInvariant() == Permissions.SysAdmin ||
                        p.Name.ToLowerInvariant() == Permissions.AppsManager);
                case (Permissions.OrgsManager):
                    return assigning.Permissions.Any(p =>
                        p.Name.ToLowerInvariant() == Permissions.SysAdmin ||
                        p.Name.ToLowerInvariant() == Permissions.OrgsManager);
                case (Permissions.OrgManager):
                    return assigning.Permissions.Any(p =>
                        p.Name.ToLowerInvariant() == Permissions.SysAdmin ||
                        p.Name.ToLowerInvariant() == Permissions.OrgsManager ||
                        (p.Name.ToLowerInvariant() == Permissions.OrgManager && orgId == p.DataId && p.DataId != Guid.Empty));
                case (Permissions.GroupManager):
                    return assigning.Permissions.Any(p =>
                        p.Name.ToLowerInvariant() == Permissions.SysAdmin ||
                        p.Name.ToLowerInvariant() == Permissions.OrgsManager ||
                        (p.Name.ToLowerInvariant() == Permissions.OrgManager && orgId == p.DataId && p.DataId != Guid.Empty) ||
                        (p.Name.ToLowerInvariant() == Permissions.GroupManager && groupId == p.DataId && p.DataId != Guid.Empty));
                default:
                    return false;

            }
        }
    }
}
