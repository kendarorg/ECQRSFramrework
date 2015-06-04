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
using ECQRS.Commons.Interfaces;
using ECQRS.Commons.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManager.Core.Organizations.ReadModel;
using UserManager.Core.Users.Commands;
using UserManager.Organizations.Commands;

namespace UserManager.Commons.Groups
{
    public class UsersCommandHandler : IECQRSService, ICommandHandler
    {
        private  ICommandSender _sender;
        private IRepository<OrganizationUserItem> _orgUsers;

        public UsersCommandHandler(ICommandSender sender,IRepository<OrganizationUserItem> orgUsers)
        {
            _sender = sender;
            _orgUsers = orgUsers;
        }

        public void Handle(CreateUserWithGroup message)
        {
            _sender.Send(new UserCreate
            {
                UserName = message.UserName,
                FirstName = message.FirstName,
                EMail = message.EMail,
                UserId = message.UserId,
                LastName = message.LastName,
                HashedPassword = message.HashedPassword
            });

            if (message.OrganizationId != Guid.Empty)
            {
                _sender.Send(new OrganizationUserAssociate
                {
                    UserId = message.UserId,
                    OrganizationId = message.OrganizationId
                });
            }
        }

        public void Handle(DeleteCommonUser message)
        {
            var allInvolvedOrganizations = _orgUsers.Where(r => r.UserId == message.UserId).ToList();

            foreach (var role in allInvolvedOrganizations)
            {
                //For the organization
                _sender.Send(new OrganizationUserDeassociate
                {
                    CorrelationId = message.CorrelationId,
                    OrganizationId = role.OrganizationId,
                    UserId = role.UserId
                });
            }

            //For the user
            _sender.Send(new UserDelete
            {
                CorrelationId = message.CorrelationId,
                UserId = message.UserId
            });
        }

        public void Handle(OrganizationUserGroupAssociateCommon message)
        {
            //For the Group
            _sender.Send(new OrganizationUserGroupAssociate
            {
                CorrelationId = message.CorrelationId,
                UserId = message.UserId,
                GroupId = message.GroupId,
                 OrganizationId = message.OrganizationId
            });
        }

        public void Handle(OrganizationUserGroupDissociateCommon message)
        {
            //For the Group
            _sender.Send(new OrganizationUserGroupDissociate
            {
                CorrelationId = message.CorrelationId,
                UserId = message.UserId,
                GroupId = message.GroupId,
                OrganizationId = message.OrganizationId
            });
        }
    }
}
