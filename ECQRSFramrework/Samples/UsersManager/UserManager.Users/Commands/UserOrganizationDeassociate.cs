using ECQRS.Commons.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManager.Core.Users.Commands
{
    public class UserOrganizationDeassociate:Command
    {
        public Guid OrganizationId { get; set; }
        public Guid UserId { get; set; }
    }

    public class UserOrganizationGroupDeassociate : Command
    {
        public Guid OrganizationId { get; set; }
        public Guid GroupId { get; set; }
        public Guid UserId { get; set; }
    }
}
