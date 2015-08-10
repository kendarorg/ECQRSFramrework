using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManager.Core.Users.Aggregate
{
    public class Permission
    {
        public Permission()
        {
            Data = new Guid[] { };
        }
        public string Name { get; set; }
        public Guid[] Data { get; set; }
    }
}
