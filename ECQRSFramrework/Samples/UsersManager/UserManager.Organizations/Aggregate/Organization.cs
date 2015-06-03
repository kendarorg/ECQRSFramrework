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

namespace UserManager.Core.Organizations.Aggregate
{
    public class Organization
    {
        public Organization()
        {
            Roles = new Dictionary<Guid, Role>();
            Groups = new Dictionary<Guid, Group>();
        }

        public bool IsDeleted { get; set; }
        public Guid Id { get; set; }
        public String Name { get; set; }
        public Dictionary<Guid, Role> Roles { get; set; }
        public Dictionary<Guid, Group> Groups { get; set; }

        internal bool HasRole(Guid roleId)
        {
            return Roles.ContainsKey(roleId);
        }

        internal void AddRole(Guid applicationId,Guid roleId)
        {
            Roles.Add(roleId, new Role
            {
                ApplicationId = applicationId,
                Id = roleId
            });
        }

        internal void DeleteRole(Guid applicationId,Guid roleId)
        {
            Roles.Remove(roleId);
            foreach (var group in Groups.Values)
            {
                if (group.Roles.Contains(roleId))
                {
                    group.Roles.Remove(roleId);
                }
            }
        }

        internal bool HasGroup(Guid groupId, string code = null)
        {
            return Groups.ContainsKey(groupId) ||
                (!string.IsNullOrWhiteSpace(code) && Groups.Values.Any(p => p.Code == code));
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

        internal bool HasGroupRole(Guid groupId, Guid roleId)
        {
            return Groups[groupId].Roles.Contains(roleId);
        }

        internal void AddRoleGroup(Guid groupId, Guid roleId)
        {
            Groups[groupId].Roles.Add(roleId);
        }

        internal void DeleteRoleGroup(Guid groupId, Guid roleId)
        {
            Groups[groupId].Roles.Remove(roleId);
        }
    }
}
