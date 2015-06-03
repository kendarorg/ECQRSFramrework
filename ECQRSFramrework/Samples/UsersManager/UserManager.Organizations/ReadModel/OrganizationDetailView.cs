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
using ECQRS.Commons.Exceptions;
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
    public class OrganizationDetailItem : IEntity
    {
        [AutoGen(false)]
        public Guid Id { get; set; }
        public string Name { get; set; }
        [DbType("TEXT")]
        public string Roles { get; set; }
        [DbType("TEXT")]
        public string Groups { get; set; }
    }

    public class OrganizationDetailView : IEventHandler, IECQRSService
    {
        public class OrganizationRole
        {
            public Guid Id { get; set; }
        }

        public class OrganizationGroup
        {
            public OrganizationGroup()
            {
                Roles = new List<OrganizationRole>();
            }
            public Guid Id { get; set; }
            public List<OrganizationRole> Roles { get; set; }
        }

        private readonly IRepository<OrganizationDetailItem> _repository;

        public OrganizationDetailView(IRepository<OrganizationDetailItem> repository)
        {
            _repository = repository;
        }

        public void Handle(OrganizationCreated message)
        {
            _repository.Save(new OrganizationDetailItem
            {
                Id = message.OrganizationId,
                Name = message.Name
            });
        }

        public void Handle(OrganizationDeleted message)
        {
            _repository.Delete(message.OrganizationId);
        }

        public void Handle(OrganizationModified message)
        {
            var item = _repository.Get(message.OrganizationId);
            item.Name = message.Name;

            _repository.Update(item);
        }

        public void Handle(OrganizationRoleAdded message)
        {
            var item = _repository.Get(message.OrganizationId);
            var roles = new List<OrganizationRole>();
            if (!string.IsNullOrWhiteSpace(item.Roles))
            {
                roles = JsonConvert.DeserializeObject<List<OrganizationRole>>(item.Roles);
            }
            if (roles.Any(p => p.Id == message.RoleId))
            {
                throw new DuplicateEntityException(
                    message.OrganizationId + "." + message.RoleId,
                    "OrganizationDetailItem.Role");
            }
            roles.Add(new OrganizationRole
            {
                Id = message.RoleId
            });
            item.Roles = JsonConvert.SerializeObject(roles);

            _repository.Update(item);
        }

        public void Handle(OrganizationRoleDeleted message)
        {
            var item = _repository.Get(message.OrganizationId);

            if (!string.IsNullOrWhiteSpace(item.Roles))
            {
                var roles = JsonConvert.DeserializeObject<List<OrganizationRole>>(item.Roles);
                roles = roles.Where(p => p.Id != message.RoleId).ToList();
                item.Roles = JsonConvert.SerializeObject(roles);
            }


            if (!string.IsNullOrWhiteSpace(item.Groups))
            {
                var groups = JsonConvert.DeserializeObject<List<OrganizationGroup>>(item.Roles);
                foreach (var group in groups)
                {
                    group.Roles = group.Roles.Where(a => a.Id != message.RoleId).ToList();
                }
                item.Groups = JsonConvert.SerializeObject(groups);
            }


            _repository.Update(item);
        }


        public void Handle(OrganizationGroupCreated message)
        {
            var item = _repository.Get(message.OrganizationId);
            var groups = new List<OrganizationGroup>();
            if (!string.IsNullOrWhiteSpace(item.Groups))
            {
                groups = JsonConvert.DeserializeObject<List<OrganizationGroup>>(item.Groups);
            }
            if (groups.Any(p => p.Id == message.GroupId))
            {
                return;
            }
            groups.Add(new OrganizationGroup
            {
                Id = message.GroupId
            });
            item.Groups = JsonConvert.SerializeObject(groups);

            _repository.Update(item);
        }

        public void Handle(OrganizationGroupDeleted message)
        {
            var item = _repository.Get(message.OrganizationId);
            var groups = new List<OrganizationGroup>();
            if (!string.IsNullOrWhiteSpace(item.Groups))
            {
                groups = JsonConvert.DeserializeObject<List<OrganizationGroup>>(item.Groups);
            }
            groups = groups.Where(p => p.Id != message.GroupId).ToList();
            item.Groups = JsonConvert.SerializeObject(groups);

            _repository.Update(item);
        }

        public void Handle(OrganizationGroupRoleDeleted message)
        {
            var item = _repository.Get(message.OrganizationId);
            if (!string.IsNullOrWhiteSpace(item.Groups))
            {
                var groups = JsonConvert.DeserializeObject<List<OrganizationGroup>>(item.Roles);
                var group = groups.FirstOrDefault(r => r.Id == message.GroupId);
                if (group == null) return;

                group.Roles = group.Roles.Where(a => a.Id != message.RoleId).ToList();
                item.Groups = JsonConvert.SerializeObject(groups);
            }
            _repository.Update(item);
        }

        public void Handle(OrganizationGroupRoleAdded message)
        {
            var item = _repository.Get(message.OrganizationId);
            if (!string.IsNullOrWhiteSpace(item.Groups))
            {
                var groups = JsonConvert.DeserializeObject<List<OrganizationGroup>>(item.Roles);
                var group = groups.FirstOrDefault(r => r.Id == message.GroupId);
                if (group == null) return;
                if (group.Roles.Any(a => a.Id != message.RoleId)) return;

                group.Roles.Add(new OrganizationRole{
                  Id =message.RoleId,
                });
                item.Groups = JsonConvert.SerializeObject(groups);
            }
            _repository.Update(item);
        }
    }
}
