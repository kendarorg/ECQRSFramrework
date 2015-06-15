using ECQRS.Commons.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManager.Shared
{
    public class Permissions
    {
        public const string SysAdmin = "sysadmin";
        public const string OrgsManager = "orgsmanager";
        public const string AppsManager = "appsmanager";
        public const string OrgManager = "orgmanager";
        public const string GroupManager = "groupmanager";
    }

    public interface IPermissionsService : IECQRSService
    {
        bool HasPermission(
                   Guid userId,
                   string requiredPermission,
                   params Guid[] ids);

        bool CanAssign(Guid assigning, Guid assignee, string permission, Guid orgId, Guid groupId);
    }
}
