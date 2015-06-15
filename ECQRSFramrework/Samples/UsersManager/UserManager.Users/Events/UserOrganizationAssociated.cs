using ECQRS.Commons.Commands;
using ECQRS.Commons.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManager.Core.Users.Commands
{
    public class UserOrganizationAssociated:Event
    {
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public Guid UserId { get; set; }
        public string  UserName { get; set; }
        public string EMail { get; set; }
    }

    public class UserOrganizationGroupAssociated : Event
    {
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public Guid GroupId { get; set; }
        public string GroupCode { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string EMail { get; set; }
    }
}
