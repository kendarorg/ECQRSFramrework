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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManager.Core.Applications.Aggregate
{
    public class Application
    {
        public Application()
        {
            Permissions = new Dictionary<Guid, Permission>();
            Roles = new Dictionary<Guid, Role>();
        }

        public bool IsDeleted { get; set; }
        public Guid Id { get; set; }
        public String Name { get; set; }
        public Dictionary<Guid, Permission> Permissions { get; set; }
        public Dictionary<Guid, Role> Roles { get; set; }

        internal bool HasPermission(Guid permissionId, string code = null)
        {
            return Permissions.ContainsKey(permissionId) || 
                (!string.IsNullOrWhiteSpace(code) && Permissions.Values.Any(p=>p.Code == code));
        }

        internal void AddPermission(Guid permissionId, string code, string description)
        {
            Permissions.Add(permissionId, new Permission
            {
                Id = permissionId,
                Code = code,
                Description = description
            });
        }

        internal void DeletePermission(Guid permissionId)
        {
            Permissions.Remove(permissionId);
            foreach (var role in Roles.Values)
            {
                if (role.Permissions.Contains(permissionId))
                {
                    role.Permissions.Remove(permissionId);
                }
            }
        }

        internal bool HasRole(Guid roleId, string code = null)
        {
            return Roles.ContainsKey(roleId) ||
                (!string.IsNullOrWhiteSpace(code) && Roles.Values.Any(p => p.Code == code));
        }

        internal void AddRole(Guid roleId, string code, string description)
        {
            Roles.Add(roleId, new Role
            {
                Id = roleId,
                Code = code,
                Description = description
            });
        }

        internal void ModifyRole(Guid roleId, string code, string description)
        {
            Roles[roleId].Code = code;
            Roles[roleId].Description = description;
        }

        internal void DeleteRole(Guid roleId)
        {
            Roles.Remove(roleId);
        }

        internal bool HasRolePermission(Guid roleId, Guid permissionId)
        {
            return Roles[roleId].Permissions.Contains(permissionId);
        }

        internal void AddPermissionRole(Guid roleId, Guid permissionId)
        {
            Roles[roleId].Permissions.Add(permissionId);
        }

        internal void DeletePermissionRole(Guid roleId, Guid permissionId)
        {
            Roles[roleId].Permissions.Remove(permissionId);
        }
    }
}
