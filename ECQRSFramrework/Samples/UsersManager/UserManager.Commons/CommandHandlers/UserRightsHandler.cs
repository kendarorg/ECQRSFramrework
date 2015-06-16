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
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManager.Core.Applications;
using UserManager.Core.Applications.ReadModel;
using UserManager.Core.Organizations;
using UserManager.Core.Organizations.ReadModel;
using UserManager.Core.Users;
using UserManager.Core.Users.Commands;
using UserManager.Shared;

namespace UserManager.Commons.CommandHandlers
{
    public class UserRightsHandler : IECQRSService, ICommandHandler
    {
        private readonly IAggregateRepository<UserItem> _repository;
        private IAggregateRepository<OrganizationItem> _organizations;
        private IRepository<OrganizationListItem> _organizationList;
        private IRepository<OrganizationGroupItem> _groups;
        private IAggregateRepository<ApplicationItem> _applications;
        private IRepository<ApplicationListItem> _applicationList;
        private IRepository<ApplicationRoleItem> _roles;
        private IPermissionsService _permissionsService;

        public UserRightsHandler(
            IPermissionsService permissionsService,
            IAggregateRepository<UserItem> repository,
            IAggregateRepository<ApplicationItem> applications, IRepository<ApplicationListItem> applicationList, IRepository<ApplicationRoleItem> roles,
            IAggregateRepository<OrganizationItem> organizations, IRepository<OrganizationListItem> organizationList, IRepository<OrganizationGroupItem> groups)
        {
            _repository = repository;
            _organizations = organizations;
            _organizationList = organizationList;
            _groups = groups;
            _applications = applications;
            _applicationList = applicationList;
            _roles = roles;
            _permissionsService = permissionsService;
        }

        public void Handle(UserRightAssign message)
        {
            var appSecret = Guid.Parse(ConfigurationManager.AppSettings["AppSecret"]);
            UserItem assigning = null;
            try
            {
                assigning = _repository.GetById(message.Assigning);
            }
            catch
            {
                Console.WriteLine("User setup warning.");
            }
            var assignee = _repository.GetById(message.Assignee);

            if (assigning == null && message.Assigning == appSecret)
            {
                assignee.AssignRight(message.Assigning, message.Permission, message.OrganizationId, message.GroupId);
                _repository.Save(assignee, -1);
            }
            else if (_permissionsService.CanAssign(assigning.Id, assignee.Id, message.Permission, message.OrganizationId, message.GroupId))
            {
                assignee.AssignRight(assigning.Id, message.Permission, message.OrganizationId, message.GroupId);
                _repository.Save(assignee, -1);
            }
        }

        public void Handle(UserRightRemove message)
        {
            var assigning = _repository.GetById(message.Assigning);
            var assignee = _repository.GetById(message.Assignee);

            if (_permissionsService.CanAssign(assigning.Id, assignee.Id, message.Permission, message.OrganizationId, message.GroupId))
            {
                assignee.RemoveRight(assigning.Id, message.Permission, message.OrganizationId, message.GroupId);
                _repository.Save(assignee, -1);
            }
        }
        /*
        public void Handle(UserOrganizationGroupAssociate message)
        {
            var org = _organizations.GetById(message.OrganizationId);
            var orgItem = _organizationList.Get(org.Id);
            var group = _groups.Where(g => g.Id == message.GroupId && g.OrganizationId == orgItem.Id).FirstOrDefault();
            var item = _repository.GetById(message.UserId);
            item.SetLastCommand(message);
            item.AssociateWithOrganizationGroup(org.Id, orgItem.Name, group.Id, group.Code);
            _repository.Save(item, -1);
        }

        public void Handle(UserOrganizationGroupDeassociate message)
        {
            var item = _repository.GetById(message.UserId);
            item.SetLastCommand(message);
            item.DeassociateFromOrganizationGroup(message.OrganizationId, message.GroupId);
            _repository.Save(item, -1);
        }

        public void Handle(UserCreateWithGroup message)
        {

            var orgItem = _organizationList.Get(message.OrganizationId);
            var group = _groups.Where(g => g.Id == message.GroupId && g.OrganizationId == orgItem.Id).FirstOrDefault();

            var item = new UserItem(
               message.CorrelationId,
               message.UserId,
               message.UserName,
               message.EMail,
               message.HashedPassword,
               message.FirstName,
               message.LastName);
            item.SetLastCommand(message);

            item.AssociateWithOrganization(message.OrganizationId, orgItem.Name);
            item.AssociateWithOrganizationGroup(message.OrganizationId, orgItem.Name, group.Id, group.Code);

            _repository.Save(item, -1);
        }*/
    }
}
