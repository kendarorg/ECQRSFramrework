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
using UserManager.Commons.Roles;
using UserManager.Core.Applications;
using UserManager.Core.Applications.Commands;
using UserManager.Core.Organizations.Commands;
using UserManager.Core.Organizations.ReadModel;
using UserManager.Organizations.Commands;

namespace UserManager.Commons.Applications
{
    public class ApplicationsCommandHandler : IECQRSService, ICommandHandler
    {
        private  ICommandSender _sender;
        private IRepository<OrganizationRoleItem> _organizationRoles;

        public ApplicationsCommandHandler(ICommandSender sender, IRepository<OrganizationRoleItem> organizationRoles)
        {
            _sender = sender;
            _organizationRoles = organizationRoles;
        }

        public void Handle(DeleteCommonApplication message)
        {
            var allInvolvedOrganizations = _organizationRoles.Where(r => r.ApplicationId == message.ApplicationId).ToList();

            foreach (var role in allInvolvedOrganizations)
            {
                //For the organization
                _sender.Send(new OrganizationRoleDelete
                {
                    CorrelationId = message.CorrelationId,
                    ApplicationId = message.ApplicationId,
                    OrganizationId = role.OrganizationId,
                    RoleId = role.RoleId
                });
            }

            //For the application
            _sender.Send(new ApplicationDelete
            {
                CorrelationId = message.CorrelationId,
                ApplicationId = message.ApplicationId
            });
        }

        public void Handle(DeleteCommonRole message)
        {
            var allInvolvedOrganizations = _organizationRoles.Where(r => r.RoleId == message.RoleId).ToList();

            foreach (var role in allInvolvedOrganizations)
            {
                //For the organization
                _sender.Send(new OrganizationRoleDelete
                {
                    CorrelationId = message.CorrelationId,
                    ApplicationId = message.ApplicationId,
                    OrganizationId = role.OrganizationId,
                    RoleId = message.RoleId
                });
            }

            //For the application
            _sender.Send(new ApplicationRoleDelete
            {
                CorrelationId = message.CorrelationId,
                ApplicationId = message.ApplicationId,
                RoleId = message.RoleId
            });
        }


    }
}
