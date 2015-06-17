using ECQRS.Commons.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManager.Core.Users.Commands
{
    public class UserLogin : Command
    {
        public string UserId { get; set; }
        public string HashedPassword { get; set; }
        public bool RememberMe { get; set; }
    }
}
