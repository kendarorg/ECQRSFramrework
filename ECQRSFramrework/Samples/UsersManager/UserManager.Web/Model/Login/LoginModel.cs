using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UserManager.Model.Login
{
    public class LoginModel
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}