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
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UserManager.Core;
using UserManager.Core.Users.Commands;
using UserManager.Model.Setup;
using UserManager.Shared;

namespace UserManager.Controllers
{
    public class HomeController : Controller
    {
        private IHashService _hashService;
        private ICommandSender _bus;

        public HomeController(ICommandSender bus, IHashService hashService)
        {
            _hashService = hashService;
            _bus = bus;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Setup()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Setup(SetupAdmin data)
        {
            var appSecret = Guid.Parse(ConfigurationManager.AppSettings["AppSecret"]);

            if (!Request.IsLocal || appSecret != data.Secret)
            {
                throw new Exception("Invalid action!");
            }
            var password = _hashService.CalculateHash(data.Password);
            var id = Guid.NewGuid();
            _bus.SendSync(new UserCreate
            {
                CorrelationId = id,
                HashedPassword = password,
                EMail = data.EMail,
                UserName = data.UserName,
                FirstName = "admin",
                LastName = "admin",
                UserId = id
            });

            _bus.SendSync(new UserRightAssign
            {
                Assigning = appSecret,
                Assignee = id,
                CorrelationId = id,
                Permission = Permissions.SysAdmin
            });

            return View();
        }
    }
}
