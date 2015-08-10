using ECQRS.Commons.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManager.Shared
{
    public interface IPermissionsServiceFactory : IECQRSService
    {
        IPermissionsService Create(Guid assigning,Guid assignee);
    }
}
