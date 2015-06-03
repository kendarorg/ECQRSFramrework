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


using ECQRS.Commons.Events;
using ECQRS.Commons.Interfaces;
using ECQRS.Commons.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManager.Core.Organizations.Events;

namespace UserManager.Core.Organizations.ReadModel
{
    public class OrganizationGroupItem : IEntity
    {
        public OrganizationGroupItem()
        {
            Roles = string.Empty;
        }
        [AutoGen(false)]
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Roles { get; set; }

        internal void AddRole(Guid roleId)
        {
            var deserializedRoles = JsonConvert.DeserializeObject<List<Guid>>(Roles) ?? new List<Guid>();
            if (deserializedRoles.Contains(roleId)) return;
            deserializedRoles.Add(roleId);
            Roles = JsonConvert.SerializeObject(deserializedRoles);
        }

        internal void DeleteRole(Guid roleId)
        {
            var deserializedRoles = JsonConvert.DeserializeObject<List<Guid>>(Roles) ?? new List<Guid>();
            if (!deserializedRoles.Contains(roleId)) return;
            deserializedRoles = deserializedRoles.Where(r => r != roleId).ToList();
            Roles = JsonConvert.SerializeObject(deserializedRoles);
        }
    }

    public class OrganizationGroupsView : IEventHandler, IECQRSService
    {
        private readonly IRepository<OrganizationGroupItem> _repository;
        private readonly IRepository<OrganizationGroupRoleItem> _groupRoles;

        public OrganizationGroupsView(IRepository<OrganizationGroupItem> repository, IRepository<OrganizationGroupRoleItem> groupRoles)
        {
            _repository = repository;
            _groupRoles = groupRoles;
        }

        public void Handle(OrganizationGroupCreated message)
        {
            _repository.Save(new OrganizationGroupItem
            {
                OrganizationId = message.OrganizationId,
                Id = message.GroupId,
                Code = message.Code,
                Description = message.Description,
                OrganizationName = message.OrganizationName
            });
        }

        public void Handle(OrganizationGroupModified message)
        {
            var item = _repository.Get(message.GroupId);
            item.Code = message.Code;
            item.Description = message.Description;
            _repository.Update(item);
        }

        public void Handle(OrganizationModified message)
        {
            _repository.UpdateWhere(new
            {
                OrganizationName = message.Name
            }, x => x.OrganizationId == message.OrganizationId);
        }

        public void Handle(OrganizationDeleted message)
        {
            _repository.DeleteWhere(x => x.OrganizationId == message.OrganizationId);
        }

        public void Handle(OrganizationGroupDeleted message)
        {
            _repository.Delete(message.GroupId);
        }



        public void Handle(OrganizationGroupRoleAdded message)
        {
            var group = _repository.Get(message.GroupId);
            group.AddRole(message.RoleId);
            _repository.Update(group);
        }

        public void Handle(OrganizationGroupRoleDeleted message)
        {
            var group = _repository.Get(message.GroupId);
            group.DeleteRole(message.RoleId);
            _repository.Update(group);
        }

        public void Handle(OrganizationRoleDeleted message)
        {
            var roleId = message.RoleId.ToString();
            var groups = _repository.Where(g => g.Roles.Contains(roleId)).ToList();
            foreach (var group in groups)
            {
                group.DeleteRole(message.RoleId);
                _repository.Update(group);
            }
        }


    }
}
