using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UserManager.Core.Organizations.ReadModel;
using UserManager.Core.Users.ReadModel;

namespace UserManager.Model.Organizations
{
    public class OrganizationGroupUserModel
    {
        public string UserName { get; set; }
        public string EMail { get; set; }
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public bool Associated { get; set; }
        public Guid GroupId { get; set; }
        public Guid OrganizationId { get; set; }
    }
    //UserListItem
    public static class OrganizationGroupUserModelExtension
    {
        public static OrganizationGroupUserModel ToOrganizationGroupUserModel(this UserListItem user,List<OrganizationGroupUserItem> groupUserIds,
            Guid organizationId,Guid groupId)
        {
            var gritem = groupUserIds.FirstOrDefault(i => i.UserId == user.Id);
            return new OrganizationGroupUserModel
            {
                Associated = gritem!=null,
                UserId = user.Id,
                GroupId = groupId,
                OrganizationId = organizationId,
                Id = gritem==null?Guid.NewGuid():gritem.Id,
                UserName = user.UserName,
                EMail = user.EMail

            };
        }
    }
}