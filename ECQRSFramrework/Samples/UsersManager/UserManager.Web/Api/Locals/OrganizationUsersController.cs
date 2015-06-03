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


using System.Net;
using System.Web;
using ECQRS.Commons.Commands;
using ECQRS.Commons.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using UserManager.Core.Organizations.Commands;
using UserManager.Core.Organizations.ReadModel;
using UserManager.Model.Organizations;
using UserManager.Core.Users.ReadModel;
using UserManager.Core.Users.Commands;
using UserManager.Core.Applications.ReadModel;
using UserManager.Organizations.Commands;

namespace UserManager.Api
{
    public class OrganizationUsersController : ApiController
    {
        private readonly IRepository<OrganizationGroupItem> _groups;
        private readonly IRepository<OrganizationGroupRoleItem> _groupsRoles;
        private readonly IRepository<OrganizationRoleItem> _roles;
        private readonly IRepository<UserListItem> _users;
        private readonly ICommandSender _bus;
        private IRepository<OrganizationUserItem> _orgUsers;

        public OrganizationUsersController(
            IRepository<OrganizationGroupItem> list,
            IRepository<OrganizationGroupRoleItem> groupsRoles,
            IRepository<OrganizationRoleItem> roles,
            IRepository<UserListItem> users,
            IRepository<OrganizationUserItem> orgUsers,
            ICommandSender bus)
        {
            _groups = list;
            _bus = bus;
            _groupsRoles = groupsRoles;
            _roles = roles;
            _users = users;
            _orgUsers = orgUsers;
        }

        // GET: api/Organizations
        [Route("api/OrganizationUsers/list/{organizationId}")]
        public IEnumerable<UserListItem> GetList(Guid? organizationId, string range = null, string filter = null)
        {
            if (organizationId == null) throw new HttpException(400, "Invalid organization Id");
            var parsedRange = AngularApiUtils.ParseRange(range);
            var parsedFilters = AngularApiUtils.ParseFilter(filter);

            var where = _users.Where();
            if (parsedFilters.ContainsKey("EMail")) where = where.Where(a => a.EMail.Contains(parsedFilters["EMail"].ToString()));
            if (parsedFilters.ContainsKey("UserName")) where = where.Where(a => a.UserName.Contains(parsedFilters["UserName"].ToString()));

            var organizationUsers = _orgUsers.Where(u => u.OrganizationId == organizationId.Value).ToList().Select(u => u.UserId).ToList();

            return where
                .Where(u => organizationUsers.Contains(u.Id))
                .Skip(parsedRange.From).Take(parsedRange.Count)
                .ToList();
        }

        // DELETE: api/Organizations/5
        [Route("api/OrganizationUsers/{organizationId}/{userId}")]
        public void Delete(Guid organizationId, Guid userId)
        {
            var item = _orgUsers.Where(u => u.OrganizationId == organizationId && u.UserId == userId).ToList().FirstOrDefault();
            _bus.Send(new OrganizationDeassociateUser
            {
                UserAssociationId = item.Id,
                UserId = userId,
                OrganizationId = organizationId
            });
        }
    }
}
