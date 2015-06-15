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


using ECQRS.Commons.Commands;
using ECQRS.Commons.Domain;
using ECQRS.Commons.Interfaces;
using ECQRS.Commons.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManager.Core.Applications;
using UserManager.Core.Applications.ReadModel;
using UserManager.Core.Organizations;
using UserManager.Core.Organizations.Commands;

namespace UserManager.Commons.CommandHandlers
{
    public class OrganizationCommonHandler : IECQRSService, ICommandHandler
    {
        private readonly IAggregateRepository<OrganizationItem> _repository;
        private IAggregateRepository<ApplicationItem> _applications;
        private IRepository<ApplicationListItem> _applicationList;
        private IRepository<ApplicationRoleItem> _roles;

        public OrganizationCommonHandler(IAggregateRepository<OrganizationItem> repository,
            IAggregateRepository<ApplicationItem> applications, IRepository<ApplicationListItem> applicationList,IRepository<ApplicationRoleItem> roles)
        {
            _repository = repository;
            _applications = applications;
            _applicationList = applicationList;
            _roles = roles;
        }
        
        public void Handle(OrganizationRoleAdd message)
        {
            var item = _repository.GetById(message.OrganizationId);
            var app = _applications.GetById(message.ApplicationId);
            var appItem = _applicationList.Get(app.Id);
            var role = _roles.Get(message.RoleId);
            item.SetLastCommand(message);
            item.AddRole(app.Id,appItem.Name, role.Id,role.Code);
            _repository.Save(item, -1);
        }

        public void Handle(OrganizationRoleDelete message)
        {
            var item = _repository.GetById(message.OrganizationId);
            item.SetLastCommand(message);
            item.DeleteRole(message.ApplicationId, message.RoleId);
            _repository.Save(item, -1);
        }

        public void Handle(OrganizationGroupRoleAdd message)
        {
            var item = _repository.GetById(message.OrganizationId);
            var app = _applications.GetById(message.ApplicationId);
            var appItem = _applicationList.Get(app.Id);
            var role = _roles.Get(message.RoleId);
            item.SetLastCommand(message);
            item.AddRoleGroup(app.Id, appItem.Name, message.RoleId, role.Code, message.GroupId);
            _repository.Save(item, -1);
        }

        public void Handle(OrganizationGroupRoleDelete message)
        {
            var item = _repository.GetById(message.OrganizationId);
            item.SetLastCommand(message);
            item.DeleteRoleGroup( message.GroupId, message.RoleId);
            _repository.Save(item, -1);
        }
    }
}
