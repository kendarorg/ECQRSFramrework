using ECQRS.Commons.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManager.Organizations.Commands
{
    public class OrganizationUserGroupAssociateCommon : Command
    {
        public Guid OrganizationId { get; set; }

        public Guid GroupId { get; set; }

        public Guid UserId { get; set; }

        public Guid Id { get; set; }
    }
}
