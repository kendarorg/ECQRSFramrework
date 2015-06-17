using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UserManager.Model.Login
{
    

    public class LoginResult
    {
        public LoginResult()
        {
            Permissions = new List<LoginPermission>();
            IsAuthorized = false;
        }

        public string UserName { get; set; }
        public string EMail { get; set; }
        public bool IsAuthorized { get; set; }
        public List<LoginPermission> Permissions { get; set; }

    }
}