using ECQRS.Commons.Repositories;
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
    public class GroupUserItem : IEntity
    {
        [AutoGen(false)]
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public Guid GroupId {get;set;}
        public string GroupCode {get;set;}
        public string GroupDescription {get;set;}
        public Guid ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string EMail { get; set; }
    }

    public class GroupUsersView : IEventHandler, IECQRSService
    {
        private IRepository<GroupUserItem> _repostory;
        public GroupUsersView(IRepository<GroupUserItem> repository)
        {
            _repostory = repository;
        }
    }
}
