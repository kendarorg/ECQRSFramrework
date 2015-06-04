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
using UserManager.Commons.Applications;
using UserManager.Core;
using UserManager.Core.Applications.Commands;
using UserManager.Core.Applications.ReadModel;
using UserManager.Model.Applications;

namespace UserManager.Api
{
    public class ApplicationsController : ApiController
    {
        private IRepository<ApplicationListItem> _list;
        private ICommandSender _bus;

        public ApplicationsController(IRepository<ApplicationListItem> list, ICommandSender bus)
        {
            _list = list;
            _bus = bus;
        }

        // GET: api/Applications
        public IEnumerable<ApplicationListItem> Get(string range = null, string filter = null)
        {
            var parsedRange = AngularApiUtils.ParseRange(range);
            var parsedFilters = AngularApiUtils.ParseFilter(filter);

            var where = _list.Where();
            if (parsedFilters.ContainsKey("Name")) where = where.Where(a => a.Name.Contains(parsedFilters["Name"].ToString()));

            return where
                .Skip(parsedRange.From).Take(parsedRange.Count);
        }

        // GET: api/Applications/5
        public ApplicationListItem Get(Guid id)
        {
            var res = _list.Get(id);
            if (res != null) return res;
            return null;
        }

        // POST: api/Applications
        public void Post([FromBody]ApplicationCreateModel value)
        {
            _bus.Send(new ApplicationCreate
            {
                Name = value.Name,
                ApplicationId = Guid.NewGuid()
            });
        }

        // PUT: api/Applications/5
        public void Put(Guid id, [FromBody]ApplicationModifyModel value)
        {
            _bus.Send(new ApplicationModify
            {
                Name = value.Name,
                ApplicationId = value.Id
            });
        }

        // DELETE: api/Applications/5
        public void Delete(Guid id)
        {
            _bus.Send(new DeleteCommonApplication { ApplicationId = id });
        }
    }
}
