using ECQRS.Commons.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManager.Core.Users;
using UserManager.Core.Users.Aggregate;
using UserManager.Shared;

namespace UserManager.Commons.Services
{
    public static class UserExtension
    {
        public static Permission GetPermission(this User item, params string[] permission)
        {
            return item.Permissions.FirstOrDefault(p => permission.Contains(p.Name));
        }
        public static List<Permission> GetPermissions(this User item, params string[] permission)
        {
            return item.Permissions.Where(p => permission.Contains(p.Name)).ToList();
        }
    }

    public class PermissionsService : IPermissionsService
    {
        private User _source;
        private User _target;
        private IOrganizationsService _organizationService;

        public PermissionsService(UserItem source, UserItem target, IOrganizationsService organizationService)
        {
            _source = source.User;
            _target = target.User;
            _organizationService = organizationService;
        }

        public bool CanCreateApplications()
        {
            return _source.GetPermission(Permissions.AppsManager, Permissions.SysAdmin) != null;
        }

        public IEnumerable<Kvp> CanSeeOrganizations()
        {
            var allOrgs = _organizationService.GetOrganizations();
            if (IsGroupsOrSysAdmin())
            {
                return allOrgs;
            }
            return _source.GetPermissions(Permissions.GroupManager, Permissions.OrgManager)
                .Where(p => p.Data.Length >= 1)
                .Distinct()
                .Select(p =>allOrgs.FirstOrDefault(o=>o.Key== p.Data[0]))
                .Where(p=>p!=null);
        }

        private bool IsGroupsOrSysAdmin()
        {
            return _source.GetPermission(Permissions.OrgsManager, Permissions.SysAdmin) != null;
        }

        public IEnumerable<Kvp> CanSeeGroups(Guid organizationId)
        {
            return CanManageGroups(organizationId);
        }



        public IEnumerable<Kvp> CanManageGroups(Guid organizationId)
        {
            var allOrganizationGroups = _organizationService.GetOrganizationGroup(organizationId);
            if (IsGroupsOrSysAdmin())
            {
                return allOrganizationGroups;
            }
            var orgManager = _source.GetPermissions(Permissions.OrgManager)
                .FirstOrDefault(p => p.Data.Length >= 1 && p.Data[0] == organizationId);
            if (orgManager != null)
            {
                return allOrganizationGroups;
            }

            return _source.GetPermissions(Permissions.GroupManager)
                .Where(p => p.Data.Length >= 2 && p.Data[0] == organizationId)
                .Select(p => allOrganizationGroups.FirstOrDefault(o => o.Key == p.Data[1]))
                .Where(p => p != null);
        }

        public bool CanCreateOganizations()
        {
            return _source.GetPermission(Permissions.SysAdmin, Permissions.OrgsManager) != null;
        }

        public IEnumerable<Kvp> CanManageOganizations()
        {
            var allOrgs = _organizationService.GetOrganizations();
            if (IsGroupsOrSysAdmin())
            {
                return allOrgs;
            }

            return _source.GetPermissions(Permissions.OrgManager)
                .Where(p => p.Data.Length >= 1)
                .Distinct()
                .Select(p => allOrgs.FirstOrDefault(o => o.Key == p.Data[0]))
                .Where(p => p != null);
        }

        public bool CanCreateGroups(Guid organizationId)
        {
            if (IsGroupsOrSysAdmin())
            {
                return true;
            }

            return _source.GetPermissions(Permissions.OrgManager)
                .Where(p => p.Data.Length >= 1 && p.Data[0] == organizationId)
                .FirstOrDefault() != null;
        }


        public bool CanAssignGlobalRight(string permission)
        {
            if (_source.Id == _target.Id) return false;
            return _source.GetPermission(permission) != null || _source.GetPermission(Permissions.SysAdmin) != null;
        }

        public bool CanAssignSpecificRight(string permission, Guid[] data)
        {
            if (_source.Id == _target.Id) return false;
            
            if (IsGroupsOrSysAdmin()) return true;
            if (data.Length == 0) return false;
            if( _source.GetPermissions(Permissions.OrgManager)
                   .FirstOrDefault(p => p.Data[0] == data[0]) != null) return true;
            if (data.Length != 2) return false;
            if (_source.GetPermissions(Permissions.GroupManager)
                   .FirstOrDefault(p => p.Data[0] == data[0] && p.Data[1] == data[1]) != null) return true;
            return false;
        }

        public bool CanRemoveGlobalRight(string permission)
        {
            return CanAssignGlobalRight(permission);
        }

        public bool CanRemoveSpecificRight(string permission, Guid[] data)
        {
            return CanAssignSpecificRight(permission, data);
        }

    }
}
