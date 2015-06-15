using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManager.Core.Users.Aggregate
{
    public class Organization
    {
        public Organization()
        {
            Groups = new HashSet<Guid>();
        }
        public Guid Id { get; set; }
        public HashSet<Guid> Groups { get; set; }
    }
}
