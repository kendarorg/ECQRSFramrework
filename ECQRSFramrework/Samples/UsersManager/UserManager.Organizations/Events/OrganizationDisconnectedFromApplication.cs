using ECQRS.Commons.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManager.Core.Organizations.Events;

namespace UserManager.Organizations.Events
{
    public class OrganizationDisconnectedFromApplication : Event
    {
        public Guid ApplicationId { get; set; }
        public Guid OrganizationId { get; set; }
    }
}
