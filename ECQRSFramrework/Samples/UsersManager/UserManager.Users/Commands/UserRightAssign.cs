
using ECQRS.Commons.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManager.Core.Users.Commands
{
    public class UserRightAssign : Command
    {
        public Guid Assigning { get; set; }
        public Guid Assignee { get; set; }
        public String Permission { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid GroupId { get; set; }
    }
}
