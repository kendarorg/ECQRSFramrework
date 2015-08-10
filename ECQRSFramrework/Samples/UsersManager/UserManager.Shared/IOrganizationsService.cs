using ECQRS.Commons.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManager.Shared
{
    public class Kvp
    {
        public Guid Key { get; set; }
        public string Description { get; set; }
    }
    public interface IOrganizationsService : IECQRSService
    {
        IEnumerable<Kvp> GetOrganizations();
        IEnumerable<Kvp> GetOrganizationGroup(Guid organizationId);
    }
}
