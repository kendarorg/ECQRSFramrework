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
        #region Common

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

        #endregion Common

        #region Constructors

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

        #endregion Constructors

        #region CRUD

        public void Apply(OrganizationCreated e)
        {
            _organization.Id = e.OrganizationId;
            _organization.Name = e.Name;
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

        #endregion CRUD

        #region Roles

        public void AddRole(Guid applicationId, Guid roleId)
        {
            CheckDeleted();
            ApplyChange(new OrganizationRoleAdded
            {
                ApplicationId = applicationId,
                CorrelationId = LastCommand,
                OrganizationId = Id,
                RoleId = roleId
            });
        }

        public void DeleteRole(Guid applicationId, Guid roleId)
        {
            CheckDeleted();
            ApplyChange(new OrganizationRoleDeleted
            {
                CorrelationId = LastCommand,
                ApplicationId = applicationId,
                OrganizationId = Id,
                RoleId = roleId
            });
        }

        #endregion Roles

        #region Group

        public void AddGroup(Guid groupId,string code,string description)
        {
            CheckDeleted();
            Check(_organization.HasGroup(groupId), new AggregateException("Duplicated group " + groupId));
            ApplyChange(new OrganizationGroupCreated
            {
                CorrelationId = LastCommand,
                OrganizationId = Id,
                GroupId = groupId,
                Code = code,
                Description = description
            });
        }

        public void Apply(OrganizationGroupCreated e)
        {
            _organization.AddGroup(e.GroupId, e.Code, e.Description);
        }


        public void ModifyGroup(Guid groupId, string code, string description)
        {
            CheckDeleted();
            Check(!_organization.HasGroup(groupId), new AggregateException("Missing group " + groupId));
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

        public void AddRoleGroup(Guid applicationId, Guid roleId, Guid groupId)
        {
            CheckDeleted();
            Check(!_organization.HasGroup(groupId), new AggregateException("Missing group " + groupId));
            ApplyChange(new OrganizationGroupRoleAdded
            {
                ApplicationId = applicationId,
                CorrelationId = LastCommand,
                GroupId = groupId,
                OrganizationId = Id,
                RoleId = roleId
            });
        }

        public void DeleteRoleGroup(Guid groupId, Guid roleId)
        {
            CheckDeleted();
            Check(!_organization.HasGroup(groupId), new AggregateException("Missing group " + groupId));
            ApplyChange(new OrganizationGroupRoleDeleted
            {
                CorrelationId = LastCommand,
                OrganizationId = Id,
                GroupId = groupId,
                RoleId = roleId
            });
        }

        #endregion GroupRoles

        #region OrganizationUsers

        public void UserAssociate(Guid userId)
        {
            CheckDeleted();
            ApplyChange(new OrganizationUserAssociated
            {
                OrganizationId = Id,
                UserId = userId
            });
        }

        public void UserDeassociate(Guid userId)
        {
            CheckDeleted();
            ApplyChange(new OrganizationUserDeassociated
            {
                OrganizationId = Id,
                UserId = userId
            });
        }

        #endregion OrganizationUsers

        #region OrganizationGroupUser

        public void GroupUserAssociate(Guid userId, Guid groupId)
        {
            CheckDeleted();
            ApplyChange(new OrganizationUserAssociated
            {
                OrganizationId = Id,
                UserId = userId
            });
        }

        public void GroupUserDeassociate(Guid userId, Guid groupId)
        {
            CheckDeleted();
            ApplyChange(new OrganizationUserDeassociated
            {
                OrganizationId = Id,
                UserId = userId
            });
        }

        #endregion OrganizationGroupUser
    }
}