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
using UserManager.Core.Applications.Events;

namespace UserManager.Core.Applications.ReadModel
{
    public class ApplicationDetailItem : IEntity
    {
        [AutoGen(false)]
        public Guid Id { get; set; }
        public string Name { get; set; }
        [DbType("TEXT")]
        public string Permissions { get; set; }
        [DbType("TEXT")]
        public string Roles { get; set; }
    }

    public class ApplicationDetailView : IEventHandler, IECQRSService
    {
        public class ApplicationPermission
        {
            public Guid Id { get; set; }
        }

        public class ApplicationRole
        {
            public ApplicationRole()
            {
                Permissions = new List<ApplicationPermission>();
            }
            public Guid Id { get; set; }
            public List<ApplicationPermission> Permissions { get; set; }
        }

        private readonly IRepository<ApplicationDetailItem> _repository;

        public ApplicationDetailView(IRepository<ApplicationDetailItem> repository)
        {
            _repository = repository;
        }

        public void Handle(ApplicationCreated message)
        {
            _repository.Save(new ApplicationDetailItem
            {
                Id = message.ApplicationId,
                Name = message.Name
            });
        }

        public void Handle(ApplicationDeleted message)
        {
            _repository.Delete(message.ApplicationId);
        }

        public void Handle(ApplicationModified message)
        {
            var item = _repository.Get(message.ApplicationId);
            item.Name = message.Name;

            _repository.Update(item);
        }

        public void Handle(ApplicationPermssionAdded message)
        {
            var item = _repository.Get(message.ApplicationId);
            var permissions = new List<ApplicationPermission>();
            if (!string.IsNullOrWhiteSpace(item.Permissions))
            {
                permissions = JsonConvert.DeserializeObject<List<ApplicationPermission>>(item.Permissions);
            }
            if (permissions.Any(p => p.Id == message.PermissionId))
            {
                throw new DuplicateEntityException(
                    message.ApplicationId + "." + message.PermissionId,
                    "ApplicationDetailItem.Permission");
            }
            permissions.Add(new ApplicationPermission
            {
                Id = message.PermissionId
            });
            item.Permissions = JsonConvert.SerializeObject(permissions);

            _repository.Update(item);
        }

        public void Handle(ApplicationPermissionDeleted message)
        {
            var item = _repository.Get(message.ApplicationId);

            if (!string.IsNullOrWhiteSpace(item.Permissions))
            {
                var permissions = JsonConvert.DeserializeObject<List<ApplicationPermission>>(item.Permissions);
                permissions = permissions.Where(p => p.Id != message.PermissionId).ToList();
                item.Permissions = JsonConvert.SerializeObject(permissions);
            }


            if (!string.IsNullOrWhiteSpace(item.Roles))
            {
                var roles = JsonConvert.DeserializeObject<List<ApplicationRole>>(item.Permissions);
                foreach (var role in roles)
                {
                    role.Permissions = role.Permissions.Where(a => a.Id != message.PermissionId).ToList();
                }
                item.Roles = JsonConvert.SerializeObject(roles);
            }


            _repository.Update(item);
        }


        public void Handle(ApplicationRoleCreated message)
        {
            var item = _repository.Get(message.ApplicationId);
            var roles = new List<ApplicationRole>();
            if (!string.IsNullOrWhiteSpace(item.Roles))
            {
                roles = JsonConvert.DeserializeObject<List<ApplicationRole>>(item.Roles);
            }
            if (roles.Any(p => p.Id == message.RoleId))
            {
                return;
            }
            roles.Add(new ApplicationRole
            {
                Id = message.RoleId
            });
            item.Roles = JsonConvert.SerializeObject(roles);

            _repository.Update(item);
        }

        public void Handle(ApplicationRoleDeleted message)
        {
            var item = _repository.Get(message.ApplicationId);
            var roles = new List<ApplicationRole>();
            if (!string.IsNullOrWhiteSpace(item.Roles))
            {
                roles = JsonConvert.DeserializeObject<List<ApplicationRole>>(item.Roles);
            }
            roles = roles.Where(p => p.Id != message.RoleId).ToList();
            item.Roles = JsonConvert.SerializeObject(roles);

            _repository.Update(item);
        }

        public void Handle(ApplicationRolePermissionDeleted message)
        {
            var item = _repository.Get(message.ApplicationId);
            if (!string.IsNullOrWhiteSpace(item.Roles))
            {
                var roles = JsonConvert.DeserializeObject<List<ApplicationRole>>(item.Permissions);
                var role = roles.FirstOrDefault(r => r.Id == message.RoleId);
                if (role == null) return;

                role.Permissions = role.Permissions.Where(a => a.Id != message.PermissionId).ToList();
                item.Roles = JsonConvert.SerializeObject(roles);
            }
            _repository.Update(item);
        }

        public void Handle(ApplicationRolePermssionAdded message)
        {
            var item = _repository.Get(message.ApplicationId);
            if (!string.IsNullOrWhiteSpace(item.Roles))
            {
                var roles = JsonConvert.DeserializeObject<List<ApplicationRole>>(item.Permissions);
                var role = roles.FirstOrDefault(r => r.Id == message.RoleId);
                if (role == null) return;
                if (role.Permissions.Any(a => a.Id != message.PermissionId)) return;

                role.Permissions.Add(new ApplicationPermission{
                  Id =message.PermissionId,
                });
                item.Roles = JsonConvert.SerializeObject(roles);
            }
            _repository.Update(item);
        }
    }
}
