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
using UserManager.Core.Applications.Commands;

namespace UserManager.Core.Applications
{
    public class ApplicationCommandHandler : IECQRSService, ICommandHandler
    {
        private readonly IAggregateRepository<ApplicationItem> _repository;

        public ApplicationCommandHandler(IAggregateRepository<ApplicationItem> repository)
        {
            _repository = repository;
        }

        public void Handle(ApplicationCreate message)
        {
            var item = new ApplicationItem(
                message.CorrelationId,
                message.ApplicationId, 
                message.Name);
            _repository.Save(item, -1);
        }

        public void Handle(ApplicationDelete message)
        {
            var item = _repository.GetById(message.ApplicationId);
            item.SetLastCommand(message);
            item.Delete();
            _repository.Save(item, -1);
        }

        public void Handle(ApplicationModify message)
        {
            var item = _repository.GetById(message.ApplicationId);
            item.SetLastCommand(message);
            item.Modify(
                message.Name);
            _repository.Save(item, -1);
        }
        
        public void Handle(ApplicationPermissionAdd message)
        {
            var item = _repository.GetById(message.ApplicationId);
            item.SetLastCommand(message);
            item.AddPermission(message.PermissionId,message.Code,message.Description);
            _repository.Save(item, -1);
        }

        public void Handle(ApplicationPermissionDelete message)
        {
            var item = _repository.GetById(message.ApplicationId);
            item.SetLastCommand(message);
            item.DeletePermission(message.PermissionId);
            _repository.Save(item, -1);
        }


        public void Handle(ApplicationRoleCreate message)
        {
            var item = _repository.GetById(message.ApplicationId);
            item.SetLastCommand(message);
            item.AddRole(message.RoleId, message.Code, message.Description);
            _repository.Save(item, -1);
        }

        public void Handle(ApplicationRoleModify message)
        {
            var item = _repository.GetById(message.ApplicationId);
            item.SetLastCommand(message);
            item.ModifyRole(
                message.RoleId,
                message.Code,
                message.Description);
            _repository.Save(item, -1);
        }

        public void Handle(ApplicationRoleDelete message)
        {
            var item = _repository.GetById(message.ApplicationId);
            item.SetLastCommand(message);
            item.DeleteRole(message.RoleId);
            _repository.Save(item, -1);
        }


        public void Handle(ApplicationRolePermissionAdd message)
        {
            var item = _repository.GetById(message.ApplicationId);
            item.SetLastCommand(message);
            item.AddPermissionRole(message.RolePermissionId,message.RoleId,message.PermissionId, message.Code);
            _repository.Save(item, -1);
        }

        public void Handle(ApplicationRolePermissionDelete message)
        {
            var item = _repository.GetById(message.ApplicationId);
            item.SetLastCommand(message);
            item.DeletePermissionRole(message.RolePermissionId,message.RoleId,message.PermissionId);
            _repository.Save(item, -1);
        }
    }
}
