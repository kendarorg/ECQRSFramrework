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
using ECQRS.Commons.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using UserManager.Core;
using UserManager.Core.Applications.Commands;
using UserManager.Core.Applications.ReadModel;
using UserManager.Core.Organizations.Commands;
using UserManager.Core.Organizations.ReadModel;
using UserManager.Core.Users.Commands;

namespace UserManager.Api
{
    public class MaintenanceController : ApiController
    {
        private ICommandSender _bus;
        private IHashService _hs;
        private IRepository<ApplicationListItem> _apps;
        private IRepository<OrganizationListItem> _orgs;

        public MaintenanceController(ICommandSender bus,IHashService hs,IRepository<ApplicationListItem> apps,IRepository<OrganizationListItem> orgs)
        {
            _bus = bus;
            _hs = hs;
            _apps = apps;
            _orgs = orgs;
        }

        [HttpGet]
        [Route("api/Maintenance/InitializeDb/LoadUsers")]
        public void LoadUsers()
        {
            var users = 21;
            SetupUsers(users);
        }

        [HttpGet]
        [Route("api/Maintenance/InitializeDb/LoadApplications")]
        public void LoadApplications()
        {
            var applications = 5;
            SetupApplications(applications);
        }

        [HttpGet]
        [Route("api/Maintenance/InitializeDb/LoadOrganizations")]
        public void LoadOrganizations()
        {
            var organizations = 5;
            SetupOrganizations(organizations);
        }

        private void SetupOrganizations(int organizations)
        {
            var orgs = new List<Guid>();
            for (int i = 0; i < organizations; i++)
            {
                var orgId = Guid.NewGuid();
                orgs.Add(orgId);
                _bus.Send(new OrganizationCreate
                {
                    OrganizationId = orgId,
                    Name = "org" + i
                });
            }
        }


        [HttpGet]
        [Route("api/Maintenance/InitializeDb/LoadOrganizationGroups")]
        public void LoadOrganizationGroups()
        {
            var orgs = _orgs.GetAll().ToList();

            foreach (var organization in orgs)
            {
                var rnd = new Random();


                var permissions = 6;
                for (int j = 0; j < permissions; j++)
                {
                    _bus.Send(new OrganizationGroupCreate
                    {
                        GroupId = Guid.NewGuid(),
                        OrganizationId = organization.Id,
                        Code = organization.Name + "_g" + j,
                        Description = "Group g" + j + " for " + organization.Name
                    });
                }
            }

        }


        [HttpGet]
        [Route("api/Maintenance/InitializeDb/LoadRolesAndPermissions")]
        public void LoadRolesAndPermissions()
        {
            var apps = _apps.GetAll().ToList();

            foreach (var application in apps)
            {
                var rnd = new Random();
               
                
                var permissions = 6;
                for (int j = 0; j < permissions; j++)
                {
                    _bus.Send(new ApplicationPermissionAdd
                    {                        
                        PermissionId = Guid.NewGuid(),
                        ApplicationId = application.Id,
                        Code = application.Name + "_p" + j,
                        Description = "Permission p" + j + " for " + application.Name
                    });
                }

                var roles = 3;
                for (int j = 0; j < roles; j++)
                {
                    _bus.Send(new ApplicationRoleCreate
                    {                        
                        RoleId = Guid.NewGuid(),
                        ApplicationId = application.Id,
                        Code = application.Name +"_r" + j,
                        Description = "Role r" + j + " for " + application.Name
                    });
                }
            }
            
        }

        private void SetupApplications(int applications)
        {
            var apps = new List<Guid>();
            for (int i = 0; i < applications; i++)
            {
                var aapId = Guid.NewGuid();
                apps.Add(aapId);
                _bus.Send(new ApplicationCreate
                {
                    ApplicationId = aapId,
                    Name = "app" + i
                });
            }
            
        }

        private void SetupUsers(int users)
        {
            for (int i = 0; i < users; i++)
            {
                var mail = (int)i % 10;
                _bus.Send(new UserCreate
                {
                    UserName = "user" + i,
                    FirstName = "First " + i,
                    EMail = i + "@m" + mail + ".com",
                    UserId = Guid.NewGuid(),
                    LastName = "Last " + i,
                    HashedPassword = _hs.CalculateHash("password")
                });
            }
        }
    }
}