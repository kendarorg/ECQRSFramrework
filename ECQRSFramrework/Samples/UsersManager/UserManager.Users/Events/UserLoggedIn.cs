using ECQRS.Commons.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManager.Core.Users.Events
{
    public class UserLoggedIn:Event
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string EMail { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
