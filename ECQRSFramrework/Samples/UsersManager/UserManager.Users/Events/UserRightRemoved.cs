
using ECQRS.Commons.Commands;
using ECQRS.Commons.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManager.Core.Users.Events
{
    public class UserRightRemoved : Event
    {
        public Guid Assigning { get; set; }
        public Guid Assignee { get; set; }
        public String Permission { get; set; }
        public Guid DataId { get; set; }
    }
}
