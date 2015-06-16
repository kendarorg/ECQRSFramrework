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
using System.Net;
using System.Net.Http;
using System.Web.Http;
using UserManager.Commons.ReadModel;
using UserManager.Core;
using UserManager.Core.Organizations.ReadModel;
using UserManager.Core.Users.Commands;
using UserManager.Core.Users.ReadModel;
using UserManager.Model.Users;

namespace UserManager.Api
{
    public class UsersOrganizationsController : ApiController
    {
        private IRepository<OrganizationListItem> _organizations;
        private ICommandSender _bus;
        private IRepository<OrganizationUserItem> _orgUsers;

        public UsersOrganizationsController(IRepository<OrganizationListItem> organizations,IRepository<OrganizationUserItem> orgUsers, ICommandSender bus)
        {
            _organizations = organizations;
            _orgUsers = orgUsers;
            _bus = bus;
        }

        // GET: api/Users
        [Route("api/UserOrganizations/list/{userId}")]
        public IEnumerable<UserOrganizationModel> Get(Guid userId,string range = null, string filter = null)
        {
            var parsedRange = AngularApiUtils.ParseRange(range);
            var parsedFilters = AngularApiUtils.ParseFilter(filter);

            var where = _organizations.Where(a=> a.Deleted == false);
            if (parsedFilters.ContainsKey("Name")) where = where.Where(a => a.Name.Contains(parsedFilters["Name"].ToString()));

            var realOrgUsers = _orgUsers.Where(u => u.UserId == userId && u.Deleted == false).ToList();

            return where
                .Skip(parsedRange.From).Take(parsedRange.Count)
                .ToList()
                .Select(o => o.ToUserOrganizationModel(realOrgUsers, userId));
        }

        // POST: api/Users
        [HttpPost]
        [Route("api/UserOrganizations/{userId}")]
        public void Post(Guid userId, [FromBody]UserOrganizationModel value)
        {
            _bus.SendSync(new UserOrganizationAssociate
            {
                UserId = value.UserId,
                OrganizationId = value.OrganizationId
            });
        }

        // DELETE: api/Users/5
        [HttpDelete]
        [Route("api/UserOrganizations/{userId}/{organizationId}")]
        public void Delete(Guid userId, Guid organizationId)
        {
            _bus.SendSync(new UserOrganizationDeassociate
            {
                UserId = userId,
                OrganizationId = organizationId
            });
        }
    }
}
