using ECQRS.Commons.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManager.Core.Users.Commands
{
    public class UserOrganizationAssociate:Command
    {
        public Guid OrganizationId { get; set; }
        public Guid UserId { get; set; }
    }

    public class UserOrganizationGroupAssociate:Command
    {
        public Guid OrganizationId { get; set; }
        public Guid GroupId { get; set; }
        public Guid UserId { get; set; }
    }
}
