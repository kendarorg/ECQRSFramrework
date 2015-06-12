using ECQRS.Commons.Events;
using ECQRS.Commons.Interfaces;
using ECQRS.Commons.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManager.Commons.ReadModel
{
    public class OrganizationUserItem : IEntity
    {
        [AutoGen(false)]
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public Guid UserId { get; set; }
        public string UserCode { get; set; }
        public string UserDescription { get; set; }
    }

    public class OrganizationUsersView : IEventHandler, IECQRSService
    {
        private IRepository<OrganizationUserItem> _repostory;
        public OrganizationUsersView(IRepository<OrganizationUserItem> repository)
        {
            _repostory = repository;
        }
    }
}
