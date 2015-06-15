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
using UserManager.Organizations.Aggregate;

namespace UserManager.Core.Organizations.Aggregate
{
    public class Organization
    {
        public Organization()
        {
            Groups = new Dictionary<Guid, Group>();
            Roles = new Dictionary<Guid,Role>();
        }

        public bool Deleted { get; set; }
        public Guid Id { get; set; }
        public String Name { get; set; }
        public Dictionary<Guid, Group> Groups { get; set; }
        public Dictionary<Guid, Role> Roles { get; set; }
        
        internal bool HasGroup(Guid groupId)
        {
            return Groups.ContainsKey(groupId);
        }

        internal bool HasRole(Guid userId)
        {
            return Roles.ContainsKey(userId);
        }

        internal bool GroupHasRole(Guid groupId, Guid roleId)
        {
            if (!HasRole(roleId) || !HasGroup(groupId)) return false;
            return Groups[groupId].Roles.Contains(roleId);
        }

        internal void AddGroup(Guid groupId, string code, string description)
        {
            Groups.Add(groupId, new Group
            {
                Id = groupId,
                Code = code,
                Description = description
            });
        }

        internal void ModifyGroup(Guid groupId, string code, string description)
        {
            Groups[groupId].Code = code;
            Groups[groupId].Description = description;
        }

        internal void DeleteGroup(Guid groupId)
        {
            Groups.Remove(groupId);
        }

        internal void AddRole(Guid applicationId,Guid roleId)
        {
            Roles.Add(roleId, new Role
            {
                ApplicationId = applicationId,
                RoleId = roleId
            });
        }

        internal void AddRoleToGroup(Guid groupId, Guid roleId)
        {
            Groups[groupId].AddRole(roleId);
        }

        internal void RemoveRoleFromGroup(Guid groupId, Guid roleId)
        {
            if (!HasGroup(groupId)) return;
            Groups[groupId].RemoveRole(roleId);
        }

        internal void RemoveRole(Guid roleId)
        {
            if (Roles.ContainsKey(roleId))
            {
                Roles.Remove(roleId);
            }
            foreach (var group in Groups.Values)
            {
                group.RemoveRole(roleId);
            }
        }
    }
}
