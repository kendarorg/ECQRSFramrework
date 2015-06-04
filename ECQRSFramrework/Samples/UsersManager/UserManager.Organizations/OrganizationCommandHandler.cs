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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManager.Core.Organizations.Commands;
using UserManager.Organizations.Commands;

namespace UserManager.Core.Organizations
{
    public class OrganizationCommandHandler : IECQRSService, ICommandHandler
    {
        private readonly IAggregateRepository<OrganizationItem> _repository;

        public OrganizationCommandHandler(IAggregateRepository<OrganizationItem> repository)
        {
            _repository = repository;
        }

        public void Handle(OrganizationCreate message)
        {
            var item = new OrganizationItem(
                message.CorrelationId,
                message.OrganizationId, 
                message.Name);
            _repository.Save(item, -1);
        }

        public void Handle(OrganizationModify message)
        {
            var item = _repository.GetById(message.OrganizationId);
            item.SetLastCommand(message);
            item.Modify(
                message.Name);
            _repository.Save(item, -1);
        }

        public void Handle(OrganizationDelete message)
        {
            var item = _repository.GetById(message.OrganizationId);
            item.SetLastCommand(message);
            item.Delete();
            _repository.Save(item, -1);
        }
        
        public void Handle(OrganizationRoleAdd message)
        {
            var item = _repository.GetById(message.OrganizationId);
            item.SetLastCommand(message);
            item.AddRole(message.ApplicationId, message.RoleId);
            _repository.Save(item, -1);
        }

        public void Handle(OrganizationRoleDelete message)
        {
            var item = _repository.GetById(message.OrganizationId);
            item.SetLastCommand(message);
            item.DeleteRole(message.ApplicationId, message.RoleId);
            _repository.Save(item, -1);
        }


        public void Handle(OrganizationGroupCreate message)
        {
            var item = _repository.GetById(message.OrganizationId);
            item.SetLastCommand(message);
            item.AddGroup(message.GroupId, message.Code, message.Description);
            _repository.Save(item, -1);
        }

        public void Handle(OrganizationGroupModify message)
        {
            var item = _repository.GetById(message.OrganizationId);
            item.SetLastCommand(message);
            item.ModifyGroup(
                message.GroupId,
                message.Code,
                message.Description);
            _repository.Save(item, -1);
        }

        public void Handle(OrganizationGroupDelete message)
        {
            var item = _repository.GetById(message.OrganizationId);
            item.SetLastCommand(message);
            item.DeleteGroup(message.GroupId);
            _repository.Save(item, -1);
        }


        public void Handle(OrganizationGroupRoleAdd message)
        {
            var item = _repository.GetById(message.OrganizationId);
            item.SetLastCommand(message);
            item.AddRoleGroup(message.ApplicationId, message.GroupId,message.RoleId);
            _repository.Save(item, -1);
        }

        public void Handle(OrganizationGroupRoleDelete message)
        {
            var item = _repository.GetById(message.OrganizationId);
            item.SetLastCommand(message);
            item.DeleteRoleGroup( message.GroupId, message.RoleId);
            _repository.Save(item, -1);
        }

        public void Handle(OrganizationUserAssociate message)
        {
            var item = _repository.GetById(message.OrganizationId);
            item.SetLastCommand(message);
            item.UserAssociate(message.UserId);
            _repository.Save(item, -1);
        }

        public void Handle(OrganizationUserDeassociate message)
        {
            var item = _repository.GetById(message.OrganizationId);
            item.SetLastCommand(message);
            item.UserDeassociate(message.UserId);
            _repository.Save(item, -1);
        }
    }
}
