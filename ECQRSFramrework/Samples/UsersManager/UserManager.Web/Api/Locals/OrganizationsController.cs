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
using UserManager.Core;
using UserManager.Core.Organizations.Commands;
using UserManager.Core.Organizations.ReadModel;
using UserManager.Model.Organizations;

namespace UserManager.Api
{
    public class OrganizationsController : ApiController
    {
        private IRepository<OrganizationListItem> _list;
        private ICommandSender _bus;

        public OrganizationsController(IRepository<OrganizationListItem> list, ICommandSender bus)
        {
            _list = list;
            _bus = bus;
        }

        // GET: api/Organizations
        public IEnumerable<OrganizationListItem> Get(string range = null, string filter = null)
        {
            var parsedRange = AngularApiUtils.ParseRange(range);
            var parsedFilters = AngularApiUtils.ParseFilter(filter);

            var where = _list.Where(a=> a.Deleted == false);
            if (parsedFilters.ContainsKey("Name")) where = where.Where(a => a.Name.Contains(parsedFilters["Name"].ToString()));

            return where
                .Skip(parsedRange.From).Take(parsedRange.Count);
        }

        // GET: api/Organizations/5
        public OrganizationListItem Get(Guid id)
        {
            var res = _list.Get(id);
            if (res != null) return res;
            return null;
        }

        // POST: api/Organizations
        public void Post([FromBody]OrganizationCreateModel value)
        {
            _bus.SendSync(new OrganizationCreate
            {
                Name = value.Name,
                OrganizationId = Guid.NewGuid()
            });
        }

        // PUT: api/Organizations/5
        public void Put(Guid id, [FromBody]OrganizationModifyModel value)
        {
            _bus.SendSync(new OrganizationModify
            {
                Name = value.Name,
                OrganizationId = value.Id
            });
        }

        // DELETE: api/Organizations/5
        public void Delete(Guid id)
        {
            _bus.SendSync(new OrganizationDelete { OrganizationId = id });
        }
    }
}
