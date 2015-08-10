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
using UserManager.Commons.ReadModel;

namespace UserManager.Api
{
    [Authorize]
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

        // GET: api/Organizations  TODO HERE COME TROUBLES
        [Route("api/OrganizationRoles/list/{organizationId}")]
        public IEnumerable<OrganizationRoleModel> GetList(Guid? organizationId, string range = null, string filter = null)
        {
            if (organizationId == null) throw new HttpException(400, "Invalid organization Id");
            var parsedRange = AngularApiUtils.ParseRange(range);
            var parsedFilters = AngularApiUtils.ParseFilter(filter);

            var organizationRoles = _organizationRoles.Where(a => 
                a.OrganizationId == organizationId.Value && 
                a.Deleted == false).ToList();

            var where = _applicationRoles.Where(a => a.Deleted == false);
            if (parsedFilters.ContainsKey("Code"))
            {
                var item = parsedFilters["Code"].ToString();
                where = where.Where(a => a.Code.Contains(item));
            }
            if (parsedFilters.ContainsKey("ApplicationName"))
            {
                var item = parsedFilters["ApplicationName"].ToString();
                where = where.Where(a => a.ApplicationName.Contains(item));
            }

            var result = where
                .DoSkip(parsedRange.From).DoTake(parsedRange.Count)
                .ToList();
            var result2 = result
                .Select(i => i.ToOrganizationRoleModel(organizationRoles)).ToList();
            return result2;
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
            _bus.SendSync(new OrganizationRoleAdd
            {
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
            _bus.SendSync(new OrganizationRoleDelete { OrganizationId = item.OrganizationId, RoleId = item.RoleId });
        }
    }
}
