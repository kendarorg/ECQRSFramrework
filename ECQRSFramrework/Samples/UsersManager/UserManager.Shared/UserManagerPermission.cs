using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManager.Shared
{
    public class UserManagerPermission
    {
        public Guid UserId { get; set; }
        public String Permission { get; set; }
        public Guid DataId { get; set; }
    }
}
