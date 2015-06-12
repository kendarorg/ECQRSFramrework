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
    public class GroupRoleItem : IEntity
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
        public Guid RoleId { get; set; }
        public string RoleCode { get; set; }
        public string RoleDescription { get; set; }
    }

    public class GroupRolesView : IEventHandler, IECQRSService
    {
        private IRepository<GroupRoleItem> _repostory;
        public GroupRolesView(IRepository<GroupRoleItem> repository)
        {
            _repostory = repository;
        }
    }
}
