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
using UserManager.Core.Users.ReadModel;
using UserManager.Core.Users.Commands;
using UserManager.Commons.Applications;
using UserManager.Commons.Roles;

namespace UserManager.Api
{
    public class ApplicationRolesController : ApiController
    {
        private readonly IRepository<ApplicationRoleItem> _roles;
        private readonly IRepository<ApplicationRolePermissionItem> _rolesPermissions;
        private readonly IRepository<ApplicationPermissionItem> _permissions;
        private readonly IRepository<UserListItem> _users;
        private readonly ICommandSender _bus;

        public ApplicationRolesController(
            IRepository<ApplicationRoleItem> list,
            IRepository<ApplicationRolePermissionItem> rolesPermissions,
            IRepository<ApplicationPermissionItem> permissions,
            IRepository<UserListItem> users,
            ICommandSender bus)
        {
            _roles = list;
            _bus = bus;
            _rolesPermissions = rolesPermissions;
            _permissions = permissions;
            _users = users;
        }

        // GET: api/Applications
        [Route("api/ApplicationRoles/list/{applicationId}")]
        public IEnumerable<ApplicationRoleItem> GetList(Guid? applicationId, string range = null, string filter = null)
        {
            if (applicationId == null) throw new HttpException(400, "Invalid application Id");
            var parsedRange = AngularApiUtils.ParseRange(range);
            var parsedFilters = AngularApiUtils.ParseFilter(filter);

            var where = _roles.Where(a => a.ApplicationId == applicationId.Value);
            if (parsedFilters.ContainsKey("Code")) where = where.Where(a => a.Code.Contains(parsedFilters["Code"].ToString()));
            if (parsedFilters.ContainsKey("Description")) where = where.Where(a => a.Description.Contains(parsedFilters["Description"].ToString()));

            return where
                .Skip(parsedRange.From).Take(parsedRange.Count);
        }

        // GET: api/Applications/5/1
        public ApplicationRoleItem Get(Guid id)
        {
            var res = _roles.Get(id);
            if (res != null) return res;
            return null;
        }

        // POST: api/Applications
        public void Post([FromBody]ApplicationRoleCreateModel value)
        {
            _bus.Send(new ApplicationRoleCreate
            {
                Code = value.Code,
                Description = value.Description,
                ApplicationId = value.ApplicationId,
                RoleId = Guid.NewGuid()
            });
        }

        // PUT: api/Applications
        public void Put([FromBody]ApplicationRoleModifyModel value)
        {
            _bus.Send(new ApplicationRoleModify
            {
                Code = value.Code,
                Description = value.Description,
                ApplicationId = value.ApplicationId,
                RoleId = value.Id
            });
        }

        // DELETE: api/Applications/5
        public void Delete(Guid id)
        {
            var item = _roles.Get(id);
            _bus.Send(new DeleteCommonRole { ApplicationId = item.ApplicationId, RoleId = id });
        }

        [Route("api/ApplicationRoles/permissions/{applicationId}/{roleId}")]
        public IEnumerable<ApplicationRolePermissionModel> GetList(Guid? applicationId, Guid? roleId, string range = null, string filter = null)
        {
            if (applicationId == null) throw new HttpException(400, "Invalid application Id");
            if (roleId == null) throw new HttpException(400, "Invalid role Id");
            var parsedRange = AngularApiUtils.ParseRange(range);
            var parsedFilters = AngularApiUtils.ParseFilter(filter);


            var associatedPermissions = _rolesPermissions.Where(p=>p.RoleId==roleId.Value && p.ApplicationId==applicationId.Value)
                .ToList();

            var wherePermissions = _permissions.Where(a => a.ApplicationId == applicationId.Value);
            if (parsedFilters.ContainsKey("Code")) wherePermissions = wherePermissions.Where(a => a.Code.Contains(parsedFilters["Code"].ToString()));
            if (parsedFilters.ContainsKey("Description")) wherePermissions = wherePermissions.Where(a => a.Description.Contains(parsedFilters["Description"].ToString()));
            
            return wherePermissions.ToList()
                .Skip(parsedRange.From).Take(parsedRange.Count)
                .Select(u =>
                {
                    var item = u.ToApplicationRolePermissionModel(roleId.Value);
                    var matching = associatedPermissions.FirstOrDefault(a => a.PermissionId == item.PermissionId);
                    item.Selected = matching != null;
                    if (matching != null)
                    {
                        item.Id = matching.Id;
                    }
                    return item;
                }).ToList();


        }

        [HttpPost]
        [Route("api/ApplicationRoles/permissions/{applicationId}/{roleId}")]
        public void AddApplicationRole(Guid? applicationId, Guid? roleId, ApplicationRolePermissionItem value)
        {
            if (applicationId == null) throw new HttpException(400, "Invalid application Id");
            if (roleId == null) throw new HttpException(400, "Invalid role Id");

            _bus.Send(new ApplicationRolePermissionAdd
            {
                ApplicationId = applicationId.Value,
                RoleId = roleId.Value,
                PermissionId = value.PermissionId,
                RolePermissionId = Guid.NewGuid()
            });
        }

        [HttpDelete]
        [Route("api/ApplicationRoles/permissions/{applicationId}/{roleId}/{id}")]
        public void DeleteApplicationRole(Guid? applicationId, Guid? roleId, Guid? id)
        {
            if (applicationId == null) throw new HttpException(400, "Invalid application Id");
            if (roleId == null) throw new HttpException(400, "Invalid role Id");
            if (id == null) throw new HttpException(400, "Invalid application-role Id");


            var item = _rolesPermissions.Get(id.Value);
            _bus.Send(new ApplicationRolePermissionDelete
            {
                ApplicationId = applicationId.Value,
                RoleId = roleId.Value,
                PermissionId = item.PermissionId,
                RolePermissionId = item.Id
            });
        }


        //// GET: api/Users for application
        //[Route("api/ApplicationUsers/{applicationId}")]
        //public IEnumerable<UserListItem> GetUsersByApplication(Guid? applicationId, string range = null, string filter = null)
        //{
        //    if (applicationId == null) throw new HttpException(400, "Invalid application Id");
        //    var parsedRange = AngularApiUtils.ParseRange(range);
        //    var parsedFilters = AngularApiUtils.ParseFilter(filter);


        //    var appUsers = _userApplicationItems.Where(u => u.ApplicationId == applicationId.Value).ToList().Select(u => u.UserId).ToList();
        //    parsedFilters.Add("Id", appUsers);

        //    var whereUsers = _users.Where(a => appUsers.Contains(a.Id));
        //    if (parsedFilters.ContainsKey("UserName")) whereUsers = whereUsers.Where(a => a.UserName.Contains(parsedFilters["UserName"].ToString()));
        //    if (parsedFilters.ContainsKey("EMail")) whereUsers = whereUsers.Where(a => a.EMail.Contains(parsedFilters["EMail"].ToString()));

        //    var result = whereUsers
        //        .Skip(parsedRange.From).Take(parsedRange.Count)
        //        .ToList();
        //    return result;
        //}
    }
}
