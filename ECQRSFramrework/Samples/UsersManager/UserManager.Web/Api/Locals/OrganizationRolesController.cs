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
using UserManager.Core.Applications.ReadModel;

namespace UserManager.Api
{
    public class OrganizationRolesController : ApiController
    {
        private readonly IRepository<OrganizationRoleItem> _organizationRoles;
        private readonly ICommandSender _bus;
        private IRepository<ApplicationRoleItem> _applicationRoles;

        public OrganizationRolesController(IRepository<OrganizationRoleItem> organizationRoles, IRepository<ApplicationRoleItem> applicationRoles, ICommandSender bus)
        {
            _organizationRoles = organizationRoles;
            _applicationRoles = applicationRoles;
            _bus = bus;
        }

        // GET: api/Organizations
        [Route("api/OrganizationRoles/list/{organizationId}")]
        public IEnumerable<OrganizationRoleModel> GetList(Guid? organizationId, string range = null, string filter = null)
        {
            if(organizationId==null) throw new HttpException(400,"Invalid organization Id");
            var parsedRange = AngularApiUtils.ParseRange(range);
            var parsedFilters = AngularApiUtils.ParseFilter(filter);

            var organizationRoles =  _organizationRoles.Where(a => a.OrganizationId == organizationId.Value).ToList();

            var where = _applicationRoles.Where();
            if (parsedFilters.ContainsKey("Code")) where = where.Where(a => a.Code.Contains(parsedFilters["Code"].ToString()));
            if (parsedFilters.ContainsKey("ApplicationName")) where = where.Where(a => a.ApplicationName.Contains(parsedFilters["ApplicationName"].ToString()));

            return where
                .Skip(parsedRange.From).Take(parsedRange.Count)
                .ToList()
                .Select(i=>i.ToOrganizationRoleModel(organizationRoles));
        }
        
        // GET: api/Organizations/5/1
        public OrganizationRoleItem Get(Guid id)
        {
            var res = _organizationRoles.Get(id);
            if (res != null) return res;
            return null;
        }

        // POST: api/Organizations

        [HttpPost]
        [Route("api/OrganizationRoles/{organizationId}")]
        public void Post(Guid organizationId, [FromBody]OrganizationRoleAddModel value)
        {
            _bus.Send(new OrganizationRoleAdd
            {
                ApplicationName = value.ApplicationName,
                RoleCode = value.Code,
                OrganizationId = organizationId,
                ApplicationId = value.ApplicationId,
                RoleId = value.RoleId
            });
        }

        // DELETE: api/Organizations/5
        [HttpDelete]
        [Route("api/OrganizationRoles/{organizationId}/{id}")]
        public void Delete(Guid organizationId, Guid id)
        {
            var item = _organizationRoles.Get(id);
            _bus.Send(new OrganizationRoleDelete { OrganizationId = item.OrganizationId, RoleId = id });
        }
    }
}