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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UserManager.Commons.ReadModel;
using UserManager.Core.Applications.ReadModel;
using UserManager.Core.Organizations.ReadModel;

namespace UserManager.Model.Organizations
{
    public class OrganizationRoleModel
    {
        public Guid Id { get; set; }
        public Guid RoleId { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool Assigned { get; set; }
    }

    public static class OrganizationRoleModelExtension
{

        public static OrganizationRoleModel ToOrganizationRoleModel(this ApplicationRoleItem item, List<OrganizationRoleItem> rolesSelected)
        {
            var orgRole = rolesSelected.FirstOrDefault(r=>r.RoleId == item.Id);
            return new OrganizationRoleModel
            {
                Id = orgRole == null ? Guid.NewGuid() : orgRole.Id,
                ApplicationId = item.ApplicationId,
                ApplicationName = item.ApplicationName,
                Assigned = orgRole != null,
                RoleId= item.Id,
                Code = item.Code,
                Description = item.Description
            };
        }
}
}