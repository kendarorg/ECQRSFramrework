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
using UserManager.Core.Applications.Aggregate;
using UserManager.Core.Applications.Events;

namespace UserManager.Core.Applications
{
    public class ApplicationItem : AggregateRoot
    {
        #region Common

        private Application _application;

        public override Guid Id
        {
            get { return _application.Id; }
        }

        private void CheckDeleted()
        {
            if (_application.Deleted)
            {
                throw new AggregateNotFoundException();
            }
        }

        #endregion Common

        #region Constructors

        public ApplicationItem()
        {
            _application = new Application();
            // used to create in repository ... many ways to avoid this, eg making private constructor
        }

        public ApplicationItem(Guid commandId,Guid id, string name)
        {
            _application = new Application();
            ApplyChange(new ApplicationCreated
            {
                CorrelationId = commandId,
                ApplicationId = id,
                Name = name
            });
        }

        #endregion Constructors

        #region CRUD

        public void Apply(ApplicationCreated e)
        {
            _application.Id = e.ApplicationId;
            _application.Name = e.Name;
        }
        
        public void Modify(string name)
        {
            CheckDeleted();
            ApplyChange(new ApplicationModified
            {
                CorrelationId = LastCommand,
                ApplicationId = Id,
                Name = name
            });
        }

        public void Apply(ApplicationModified e)
        {
            _application.Name = e.Name;
        }

        public void Delete()
        {
            CheckDeleted();
            ApplyChange(new ApplicationDeleted
            {
                CorrelationId = LastCommand,
                ApplicationId = Id
            });
        }

        public void Apply(ApplicationDeleted e)
        {
            _application.Deleted = true;
        }

        #endregion CRUD

        #region Permissions

        public void AddPermission(Guid permissionId, string code, string description)
        {
            CheckDeleted();
            Check(_application.HasPermission(permissionId,code), new AggregateException("Duplicated permission " + code));
            ApplyChange(new ApplicationPermssionAdded
            {
                CorrelationId = LastCommand,
                ApplicationId = Id,
                PermissionId = permissionId,
                Code = code,
                Description = description
            });
        }

        public void Apply(ApplicationPermssionAdded e)
        {
            _application.AddPermission(e.PermissionId, e.Code, e.Description);
        }

        public void DeletePermission(Guid permissionId)
        {
            CheckDeleted();
            Check(!_application.HasPermission(permissionId), new AggregateException("Missing permission " + permissionId));
            ApplyChange(new ApplicationPermissionDeleted
            {
                CorrelationId = LastCommand,
                ApplicationId = Id,
                PermissionId = permissionId
            });
        }

        public void Apply(ApplicationPermissionDeleted e)
        {
            _application.DeletePermission(e.PermissionId);
        }

        #endregion Permissions

        #region Role

        public void AddRole(Guid roleId, string code, string description)
        {
            CheckDeleted();
            Check(_application.HasRole(roleId), new AggregateException("Duplicated role " + code));
            ApplyChange(new ApplicationRoleCreated
            {
                CorrelationId = LastCommand,
                ApplicationId = Id,
                RoleId = roleId,
                Code = code,
                Description = description,
                ApplicationName = _application.Name
            });
        }

        public void Apply(ApplicationRoleCreated e)
        {
            _application.AddRole(e.RoleId, e.Code, e.Description);
        }

        public void ModifyRole(Guid roleId, string code, string description)
        {
            CheckDeleted();
            Check(!_application.HasRole(roleId), new AggregateException("Missing role " + code));
            ApplyChange(new ApplicationRoleModified
            {   
                CorrelationId = LastCommand,
                ApplicationId = Id,
                RoleId = roleId,
                Code = code,
                Description = description
            });
        }

        public void Apply(ApplicationRoleModified e)
        {
            _application.ModifyRole(e.RoleId, e.Code, e.Description);
        }

        public void DeleteRole(Guid roleId)
        {
            CheckDeleted();
            Check(!_application.HasRole(roleId), new AggregateException("Missing role " + roleId));
            ApplyChange(new ApplicationRoleDeleted
            {
                CorrelationId = LastCommand,
                ApplicationId = Id,
                RoleId= roleId
            });
        }

        public void Apply(ApplicationRoleDeleted e)
        {
            _application.DeleteRole(e.RoleId);
        }

        #endregion Role

        #region RolePermissions

        public void AddPermissionRole(Guid roleId, Guid permissionId)
        {
            CheckDeleted();
            Check(!_application.HasRole(roleId), new AggregateException("Missing role " + roleId));
            Check(!_application.HasPermission(permissionId), new AggregateException("Missing permission " + permissionId));
            Check(_application.HasRolePermission(roleId, permissionId), new AggregateException("Permission " + permissionId + " already in role " + roleId));
            ApplyChange(new ApplicationRolePermssionAdded
            {
                RolePermissionId = Guid.NewGuid(),
                CorrelationId = LastCommand,
                RoleId = roleId,
                ApplicationId = Id,
                PermissionId = permissionId
            });
        }

        public void Apply(ApplicationRolePermssionAdded e)
        {
            _application.AddPermissionRole(e.RoleId, e.PermissionId);
        }

        public void DeletePermissionRole( Guid roleId, Guid permissionId)
        {
            CheckDeleted();
            Check(!_application.HasRole(roleId), new AggregateException("Missing role " + roleId));
            Check(!_application.HasPermission(permissionId), new AggregateException("Missing permission " + permissionId));
            Check(!_application.HasRolePermission(roleId,permissionId), new AggregateException("Missing permission " + permissionId+ " in role "+roleId));
            ApplyChange(new ApplicationRolePermissionDeleted
            {
                CorrelationId = LastCommand,
                ApplicationId = Id,
                RoleId = roleId,
                PermissionId = permissionId
            });
        }

        public void Apply(ApplicationRolePermissionDeleted e)
        {
            _application.DeletePermissionRole(e.RoleId,e.PermissionId);
        }

        #endregion RolePermissions
    }
}