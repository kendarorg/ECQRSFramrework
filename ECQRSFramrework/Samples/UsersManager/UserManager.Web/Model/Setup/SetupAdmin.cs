using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UserManager.Model.Setup
{
    public class SetupAdmin
    {
        public string UserName { get; set; }
        public string EMail { get; set; }
        public string Password { get; set; }
        public Guid Secret { get; set; }
    }
}