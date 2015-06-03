// ===========================================================
// Copyright (c) 2014-2015, Enrico Da Ros/kendar.org
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// ===========================================================


using ECQRS.Commons.Domain;
using ECQRS.Commons.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UserManager.Core.Organizations.Aggregate;
using UserManager.Core.Organizations.Events;
using UserManager.Organizations.Events;

namespace UserManager.Core.Organizations
{
    public class OrganizationItem : AggregateRoot
    {
        private Organization _organization;

        public override Guid Id
        {
            get { return _organization.Id; }
        }

        private void CheckDeleted()
        {
            if (_organization.IsDeleted)
            {
                throw new AggregateNotFoundException();
            }
        }

        public OrganizationItem()
        {
            _organization = new Organization();
            // used to create in repository ... many ways to avoid this, eg making private constructor
        }

        public OrganizationItem(Guid commandId, Guid id, string name)
        {
            _organization = new Organization();
            ApplyChange(new OrganizationCreated
            {
                CorrelationId = commandId,
                OrganizationId = id,
                Name = name
            });
        }

        #region CRUD

        public void Apply(OrganizationCreated e)
        {
            _organization.Id = e.OrganizationId;
            _organization.Name = e.Name;
        }

        public void Delete()
        {
            CheckDeleted();
            ApplyChange(new OrganizationDeleted
            {
                CorrelationId = LastCommand,
                OrganizationId = Id
            });
        }

        public void Apply(OrganizationDeleted e)
        {
            _organization.IsDeleted = true;
        }

        public void Modify(string name)
        {
            CheckDeleted();
            ApplyChange(new OrganizationModified
            {
                CorrelationId = LastCommand,
                OrganizationId = Id,
                Name = name
            });
        }

        public void Apply(OrganizationModified e)
        {
            _organization.Name = e.Name;
        }

        #endregion CRUD

        #region Roles

        public void AddRole(Guid applicationId, Guid roleId, string applicationName, string roleCode)
        {
            CheckDeleted();
            Check(_organization.HasRole(roleId), new AggregateException("Duplicated role " + roleId));
            ApplyChange(new OrganizationRoleAdded
            {
                ApplicationId = applicationId,
                CorrelationId = LastCommand,
                OrganizationId = Id,
                RoleId = roleId
            });
        }

        public void Apply(OrganizationRoleAdded e)
        {
            _organization.AddRole(e.ApplicationId, e.RoleId);
        }

        public void DeleteRole(Guid applicationId, Guid roleId)
        {
            CheckDeleted();
            if(!_organization.HasRole(roleId)) return;
            ApplyChange(new OrganizationRoleDeleted
            {
                CorrelationId = LastCommand,
                ApplicationId = applicationId,
                OrganizationId = Id,
                RoleId = roleId
            });
        }

        public void Apply(OrganizationRoleDeleted e)
        {
            _organization.DeleteRole(e.ApplicationId, e.RoleId);
        }

        #endregion Roles

        #region Group


        public void AddGroup(Guid groupId, string code, string description)
        {
            CheckDeleted();
            Check(_organization.HasGroup(groupId, code), new AggregateException("Duplicated group " + code));
            ApplyChange(new OrganizationGroupCreated
            {
                CorrelationId = LastCommand,
                OrganizationId = Id,
                GroupId = groupId,
                Code = code,
                Description = description,
                OrganizationName = _organization.Name
            });
        }

        public void Apply(OrganizationGroupCreated e)
        {
            _organization.AddGroup(e.GroupId, e.Code, e.Description);
        }


        public void ModifyGroup(Guid groupId, string code, string description)
        {
            CheckDeleted();
            Check(!_organization.HasGroup(groupId, code), new AggregateException("Missing group " + code));
            ApplyChange(new OrganizationGroupModified
            {
                CorrelationId = LastCommand,
                OrganizationId = Id,
                GroupId = groupId,
                Code = code,
                Description = description
            });
        }

        public void Apply(OrganizationGroupModified e)
        {
            _organization.ModifyGroup(e.GroupId, e.Code, e.Description);
        }

        public void DeleteGroup(Guid groupId)
        {
            CheckDeleted();
            Check(!_organization.HasGroup(groupId), new AggregateException("Missing group " + groupId));
            ApplyChange(new OrganizationGroupDeleted
            {
                CorrelationId = LastCommand,
                OrganizationId = Id,
                GroupId = groupId
            });
        }

        public void Apply(OrganizationGroupDeleted e)
        {
            _organization.DeleteGroup(e.GroupId);
        }

        #endregion Group

        #region GroupRoles

        public void AddRoleGroup(Guid applicationId,Guid guid, Guid groupId, Guid roleId, string code)
        {
            CheckDeleted();
            Check(!_organization.HasGroup(groupId, code), new AggregateException("Missing group " + code));
            Check(!_organization.HasRole(roleId), new AggregateException("Missing role " + roleId));
            Check(_organization.HasGroupRole(groupId, roleId), new AggregateException("Role " + roleId + " already in group " + groupId));
            ApplyChange(new OrganizationGroupRoleAdded
            {
                ApplicationId = applicationId,
                CorrelationId = LastCommand,
                GroupRoleId = guid,
                GroupId = groupId,
                OrganizationId = Id,
                RoleId = roleId
            });
        }

        public void Apply(OrganizationGroupRoleAdded e)
        {
            _organization.AddRoleGroup(e.GroupId, e.RoleId);
        }

        public void DeleteRoleGroup(Guid guid, Guid groupId, Guid roleId)
        {
            CheckDeleted();
            Check(!_organization.HasGroup(groupId), new AggregateException("Missing group " + groupId));
            Check(!_organization.HasRole(roleId), new AggregateException("Missing role " + roleId));
            Check(!_organization.HasGroupRole(groupId, roleId), new AggregateException("Missing role " + roleId + " in group " + groupId));
            ApplyChange(new OrganizationGroupRoleDeleted
            {
                CorrelationId = LastCommand,
                GroupRoleId = guid,
                OrganizationId = Id,
                GroupId = groupId,
                RoleId = roleId
            });
        }

        public void Apply(OrganizationGroupRoleDeleted e)
        {
            _organization.DeleteRoleGroup(e.GroupId, e.RoleId);
        }

        #endregion GroupRoles

        public void AssociateUser(Guid userId)
        {
            CheckDeleted();
            ApplyChange(new OrganizationUserAssociated
            {
                OrganizationId = Id,
                UserId = userId
            });
        }

        public void DeassociateUser(Guid userId)
        {
            CheckDeleted();
            ApplyChange(new OrganizationUserDeassociated
            {
                OrganizationId = Id,
                UserId = userId
            });
        }
    }
}