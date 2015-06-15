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
using UserManager.Core.Users.Commands;
using UserManager.Core.Users.ReadModel;
using UserManager.Model.Users;

namespace UserManager.Api
{
    public class UsersController : ApiController
    {
        private IRepository<UserListItem> _list;
        private IRepository<UserDetailItem> _detail;
        private ICommandSender _bus;
        private readonly IHashService _hashService;

        public UsersController(IRepository<UserListItem> list, IRepository<UserDetailItem> detail, ICommandSender bus, IHashService hashService)
        {
            _list = list;
            _detail = detail;
            _bus = bus;
            _hashService = hashService;
        }

        // GET: api/Users
        public IEnumerable<UserListItem> Get(string range = null, string filter = null)
        {
            var parsedRange = AngularApiUtils.ParseRange(range);
            var parsedFilters = AngularApiUtils.ParseFilter(filter);

            var where = _list.Where(a=> a.Deleted == false);
            if (parsedFilters.ContainsKey("EMail")) where = where.Where(a => a.EMail.Contains(parsedFilters["EMail"].ToString()));
            if (parsedFilters.ContainsKey("UserName")) where = where.Where(a => a.UserName.Contains(parsedFilters["UserName"].ToString()));

            return where
                .Skip(parsedRange.From).Take(parsedRange.Count);
        }

        // GET: api/Users/5
        public UserDetailItem Get(Guid id)
        {
            var res = _detail.Get(id);
            if (res != null) return res;
            return null;
        }

        // POST: api/Users
        public void Post([FromBody]UserCreateModel value)
        {
            _bus.Send(new UserCreateWithGroup
            {
                UserName = value.UserName,
                FirstName = value.FirstName,
                EMail = value.EMail,
                UserId = Guid.NewGuid(),
                LastName = value.LastName,
                OrganizationId = value.OrganizationId,
                HashedPassword = _hashService.CalculateHash(value.Password)
            });
        }

        // PUT: api/Users/5
        public void Put(Guid id, [FromBody]UserModifyModel value)
        {
            _bus.Send(new UserModify
            {
                UserName = value.UserName,
                FirstName = value.FirstName,
                EMail = value.EMail,
                UserId = value.Id,
                LastName = value.LastName,
            });
        }

        // DELETE: api/Users/5
        public void Delete(Guid id)
        {
            _bus.Send(new UserDelete { UserId = id });
        }
    }
}
