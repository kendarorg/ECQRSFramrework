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
using UserManager.Model.Login;
using UserManager.Core.Users.Commands;
using UserManager.Core;
using System.Web.Security;
using UserManager.Core.Users.ReadModel;

namespace UserManager.Api
{
    public class AccountController : ApiController
    {
        private ICommandSender _bus;
        private IHashService _hashing;
        private IRepository<UserListItem> _users;

        public AccountController(IRepository<UserListItem> users, ICommandSender bus,IHashService hashing)
        {
            _bus = bus;
            _hashing = hashing;
            _users = users;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("api/Account/Login")]
        public LoginResult Login(LoginModel model)
        {
            _bus.SendSync(new UserLogin
            {
                HashedPassword = _hashing.CalculateHash(model.Password),
                UserId = model.UserId,
                RememberMe = model.RememberMe
            });
            FormsAuthentication.SetAuthCookie(model.UserId, false);

            var user = _users.Where(u => u.UserName == model.UserId || u.EMail == model.UserId).FirstOrDefault();
            return new LoginResult
            {
                EMail = user.EMail,
                IsAuthorized = true,
                UserName = user.UserName
            };
        }

        [HttpGet]
        [Authorize]
        [Route("api/Account/Logoff")]
        public void Logoff()
        {
            FormsAuthentication.SignOut();
        }
    }
}
