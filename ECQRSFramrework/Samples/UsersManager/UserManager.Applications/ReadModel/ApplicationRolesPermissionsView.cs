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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManager.Core.Applications.Events;

namespace UserManager.Core.Applications.ReadModel
{
    public class ApplicationRolePermissionItem : IEntity
    {
        [AutoGen(false)]
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }
    }

    public class ApplicationRolesPermissionsView : IEventHandler, IECQRSService
    {
        private readonly IRepository<ApplicationRolePermissionItem> _repository;

        public ApplicationRolesPermissionsView(IRepository<ApplicationRolePermissionItem> repository)
        {
            _repository = repository;
        }

        public void Handle(ApplicationRolePermssionAdded message)
        {
            _repository.Save(new ApplicationRolePermissionItem
            {
                ApplicationId = message.ApplicationId,
                PermissionId = message.PermissionId,
                RoleId = message.RoleId,
                Id = message.RolePermissionId
            });
        }

        public void Handle(ApplicationRolePermissionDeleted message)
        {
            _repository.Delete(message.RolePermissionId);
        }

        public void Handle(ApplicationPermissionDeleted message)
        {
            _repository.DeleteWhere(
                x => x.ApplicationId == message.ApplicationId && x.PermissionId == message.PermissionId);

        }

        public void Handle(ApplicationRoleDeleted message)
        {
            _repository.DeleteWhere(
                x => x.ApplicationId == message.ApplicationId && x.RoleId == message.RoleId);
        }

        public void Handle(ApplicationDeleted message)
        {
            _repository.DeleteWhere(
                x => x.ApplicationId == message.ApplicationId);
        }
    }
}
