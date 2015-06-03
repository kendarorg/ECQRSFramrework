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
using UserManager.Core.Applications.Commands;
using UserManager.Core.Applications.ReadModel;
using UserManager.Model.Applications;

namespace UserManager.Api
{
    public class ApplicationPermissionsController : ApiController
    {
        private readonly IRepository<ApplicationPermissionItem> _list;
        private readonly ICommandSender _bus;

        public ApplicationPermissionsController(IRepository<ApplicationPermissionItem> list, ICommandSender bus)
        {
            _list = list;
            _bus = bus;
        }

        // GET: api/Applications
        [Route("api/ApplicationPermissions/list/{applicationId}")]
        public IEnumerable<ApplicationPermissionItem> GetList(Guid? applicationId, string range = null, string filter = null)
        {
            if(applicationId==null) throw new HttpException(400,"Invalid application Id");
            var parsedRange = AngularApiUtils.ParseRange(range);
            var parsedFilters = AngularApiUtils.ParseFilter(filter);
            
            var where = _list.Where(a=>a.ApplicationId == applicationId.Value);
            if (parsedFilters.ContainsKey("Code")) where = where.Where(a => a.Code.Contains(parsedFilters["Code"].ToString()));
            if (parsedFilters.ContainsKey("Description")) where = where.Where(a => a.Description.Contains(parsedFilters["Description"].ToString()));

            return where
                .Skip(parsedRange.From).Take(parsedRange.Count);
        }
        
        // GET: api/Applications/5/1
        public ApplicationPermissionItem Get(Guid id)
        {
            var res = _list.Get(id);
            if (res != null) return res;
            return null;
        }

        // POST: api/Applications
        public void Post([FromBody]ApplicationPermissionCreateModel value)
        {
            _bus.Send(new ApplicationPermissionAdd
            {
                Code = value.Code,
                Description = value.Description,
                ApplicationId = value.ApplicationId,
                PermissionId = Guid.NewGuid()
            });
        }
        
        // DELETE: api/Applications/5
        public void Delete(Guid id)
        {
            var item = _list.Get(id);
            _bus.Send(new ApplicationPermissionDelete { ApplicationId = item.ApplicationId, PermissionId = id });
        }
    }
}
