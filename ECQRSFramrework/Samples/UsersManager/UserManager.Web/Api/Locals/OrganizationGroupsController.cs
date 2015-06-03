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

namespace UserManager.Api
{
    public class OrganizationGroupsController : ApiController
    {
        private readonly IRepository<OrganizationGroupItem> _groups;
        private readonly IRepository<OrganizationGroupRoleItem> _groupsRoles;
        private readonly IRepository<OrganizationRoleItem> _roles;
        private readonly IRepository<UserListItem> _users;
        private readonly ICommandSender _bus;
        private IRepository<ApplicationRoleItem> _applicationRoles;

        public OrganizationGroupsController(
            IRepository<OrganizationGroupItem> list,
            IRepository<OrganizationGroupRoleItem> groupsRoles,
            IRepository<OrganizationRoleItem> roles,
            IRepository<UserListItem> users,
            IRepository<ApplicationRoleItem> applicationRoles,
            ICommandSender bus)
        {
            _groups = list;
            _bus = bus;
            _groupsRoles = groupsRoles;
            _roles = roles;
            _users = users;
            _applicationRoles = applicationRoles;
        }

        // GET: api/Organizations
        [Route("api/OrganizationGroups/list/{organizationId}")]
        public IEnumerable<OrganizationGroupItem> GetList(Guid? organizationId, string range = null, string filter = null)
        {
            if (organizationId == null) throw new HttpException(400, "Invalid organization Id");
            var parsedRange = AngularApiUtils.ParseRange(range);
            var parsedFilters = AngularApiUtils.ParseFilter(filter);

            var where = _groups.Where();
            where = where.Where(a => a.OrganizationId == organizationId.Value);
            if (parsedFilters.ContainsKey("Code")) where = where.Where(a => a.Code.Contains(parsedFilters["Code"].ToString()));
            if (parsedFilters.ContainsKey("Description")) where = where.Where(a => a.Description.Contains(parsedFilters["Description"].ToString()));

            return where
                .Skip(parsedRange.From).Take(parsedRange.Count);
        }

        // GET: api/Organizations/5/1
        public OrganizationGroupItem Get(Guid id)
        {
            var res = _groups.Get(id);
            if (res != null) return res;
            return null;
        }

        // POST: api/Organizations
        public void Post([FromBody]OrganizationGroupCreateModel value)
        {
            _bus.Send(new OrganizationGroupCreate
            {
                Code = value.Code,
                Description = value.Description,
                OrganizationId = value.OrganizationId,
                GroupId = Guid.NewGuid()
            });
        }

        // PUT: api/Organizations
        public void Put([FromBody]OrganizationGroupModifyModel value)
        {
            _bus.Send(new OrganizationGroupModify
            {
                Code = value.Code,
                Description = value.Description,
                OrganizationId = value.OrganizationId,
                GroupId = value.Id
            });
        }

        // DELETE: api/Organizations/5
        public void Delete(Guid id)
        {
            var item = _groups.Get(id);
            _bus.Send(new OrganizationGroupDelete { OrganizationId = item.OrganizationId, GroupId = id });
        }

        /// <summary>
        /// Retrieves all the roles associated with the given organization
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="groupId"></param>
        /// <param name="range"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [Route("api/OrganizationGroups/roles/{organizationId}/{groupId}")]
        public IEnumerable<OrganizationGroupRoleModel> GetList(Guid? organizationId, Guid? groupId, string range = null, string filter = null)
        {
            if (organizationId == null) throw new HttpException(400, "Invalid organization Id");
            if (groupId == null) throw new HttpException(400, "Invalid group Id");
            var parsedRange = AngularApiUtils.ParseRange(range);
            var parsedFilters = AngularApiUtils.ParseFilter(filter);

            //First take all the possible roles,filtered
            var allRoles = _applicationRoles.Where();
            if (parsedFilters.ContainsKey("Code")) allRoles = allRoles.Where(a => a.Code.Contains(parsedFilters["Code"].ToString()));
            if (parsedFilters.ContainsKey("ApplicationName")) allRoles = allRoles.Where(a => a.ApplicationName.Contains(parsedFilters["ApplicationName"].ToString()));
            var allRolesList = allRoles.ToList();

            //Takes all the available roles for the organization
            var availableRoles = _roles.Where(a => a.OrganizationId == organizationId.Value).ToList().Select(r=>r.RoleId);

            //Take only the role instance available
            allRolesList = allRolesList.Where(r => availableRoles.Contains(r.Id)).ToList();

            //Takes the roles used by the current group
            var associatedRoles = _groupsRoles.Where(p=>p.GroupId==groupId.Value && p.OrganizationId==organizationId.Value).ToList();

            return allRolesList
                .Skip(parsedRange.From).Take(parsedRange.Count)
                .Select(r => r.ToOrganizationGroupRoleModel(associatedRoles, organizationId.Value, groupId.Value));
        }

        [HttpPost]
        [Route("api/OrganizationGroups/roles/{organizationId}/{groupId}")]
        public void AddOrganizationGroup(Guid? organizationId, Guid? groupId, OrganizationGroupRoleItem value)
        {
            if (organizationId == null) throw new HttpException(400, "Invalid organization Id");
            if (groupId == null) throw new HttpException(400, "Invalid group Id");

            _bus.Send(new OrganizationGroupRoleAdd
            {
                ApplicationId = value.ApplicationId,
                OrganizationId = organizationId.Value,
                GroupId = groupId.Value,
                RoleId = value.RoleId,
                GroupRoleId = Guid.NewGuid()
            });
        }

        [HttpDelete]
        [Route("api/OrganizationGroups/roles/{organizationId}/{groupId}/{id}")]
        public void DeleteOrganizationGroup(Guid? organizationId, Guid? groupId, Guid? id)
        {
            if (organizationId == null) throw new HttpException(400, "Invalid organization Id");
            if (groupId == null) throw new HttpException(400, "Invalid group Id");
            if (id == null) throw new HttpException(400, "Invalid organization-group Id");


            var item = _groupsRoles.Get(id.Value);
            _bus.Send(new OrganizationGroupRoleDelete
            {
                OrganizationId = organizationId.Value,
                GroupId = groupId.Value,
                RoleId = item.RoleId,
                GroupRoleId = item.Id
            });
        }
    }
}
