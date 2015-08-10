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

    /// <summary>
    /// Create=> 
    ///         Create Update Delete on the item and its children. 
    ///         Can add new users for the item and its children
    /// Manage=> 
    ///         Create Update Delete on the children
    ///         Can add new users for the children
    /// All users with admin rights can create user with the same admin rights
    /// </summary>
    public interface IPermissionsService 
    {
        bool CanCreateApplications();
        bool CanCreateOganizations();
        bool CanCreateGroups(Guid organizationId);

        IEnumerable<Kvp> CanSeeOrganizations();
        IEnumerable<Kvp> CanSeeGroups(Guid organizationId);
        
        IEnumerable<Kvp> CanManageOganizations();
        IEnumerable<Kvp> CanManageGroups(Guid organizationId);

        bool CanAssignGlobalRight(string permission);
        bool CanRemoveGlobalRight(string permission);

        bool CanAssignSpecificRight(string p, Guid[] guid);
        bool CanRemoveSpecificRight(string p, Guid[] guid);
    }
}
