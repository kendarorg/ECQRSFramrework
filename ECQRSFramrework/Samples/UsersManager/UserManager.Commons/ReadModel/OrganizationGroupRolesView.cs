using ECQRS.Commons.Events;
using ECQRS.Commons.Interfaces;
using ECQRS.Commons.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManager.Core.Applications.Events;
using UserManager.Core.Organizations.Events;

namespace UserManager.Commons.ReadModel
{
    public class OrganizationGroupRoleItem : IEntity
    {
        [AutoGen(false)]
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public Guid GroupId { get; set; }
        public string GroupCode { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public Guid RoleId { get; set; }
        public string RoleCode { get; set; }
        public bool Deleted { get; set; }
    }

    public class OrganizationGroupRolesView : IEventView
    {
        private IRepository<OrganizationGroupRoleItem> _repository;
        public OrganizationGroupRolesView(IRepository<OrganizationGroupRoleItem> repository)
        {
            _repository = repository;
        }

        public void Handle(OrganizationGroupRoleAdded message)
        {
            var result = _repository.Where(ou => ou.OrganizationId == message.OrganizationId
                && ou.RoleId == message.RoleId
                && ou.GroupId == message.GroupId).FirstOrDefault();
            if (result != null) return;
            _repository.Save(new OrganizationGroupRoleItem
            {
                Id = message.CorrelationId,
                ApplicationId = message.ApplicationId,
                ApplicationName = message.ApplicationName,
                OrganizationId = message.OrganizationId,
                OrganizationName = message.OrganizationName,
                GroupId = message.GroupId,
                GroupCode = message.GroupCode,
                RoleCode = message.RoleCode,
                RoleId = message.RoleId
            });
        }


        public void Handle(OrganizationGroupRoleDeleted message)
        {
            var result = _repository.Where(ou => ou.OrganizationId == message.OrganizationId
                && ou.RoleId == message.RoleId
                && ou.GroupId == message.GroupId).FirstOrDefault();

            if (result == null) return;
            _repository.DeleteWhere( x => x.Id == result.Id);
        }

        public void Handle(OrganizationRoleDeleted message)
        {
            var result = _repository.Where(ou => ou.OrganizationId == message.OrganizationId
                && ou.RoleId == message.RoleId).FirstOrDefault();

            if (result == null) return;
            _repository.DeleteWhere(x => x.Id == result.Id);
        }

        public void Handle(OrganizationGroupModified message)
        {
            _repository.UpdateWhere(
                new
                {
                    GroupCode = message.Code
                },
                or => or.OrganizationId == message.OrganizationId
                && or.GroupId == message.OrganizationId);
        }

        public void Handle(OrganizationGroupDeleted message)
        {
            _repository.DeleteWhere(or => or.OrganizationId == message.OrganizationId
                && or.GroupId == message.OrganizationId);
        }

        public void Handle(ApplicationModified message)
        {
            _repository.UpdateWhere(
                new
                {
                    ApplicationName = message.Name
                },
                or => or.ApplicationId == message.ApplicationId);
        }

        public void Handle(ApplicationDeleted message)
        {
            _repository.DeleteWhere(or => or.ApplicationId == message.ApplicationId);
        }

        public void Handle(ApplicationRoleModified message)
        {
            _repository.UpdateWhere(
                new
                {
                    RoleCode = message.Code
                },
                or => or.ApplicationId == message.ApplicationId && or.RoleId == message.RoleId);
        }

        public void Handle(ApplicationRoleDeleted message)
        {
            _repository.DeleteWhere(or => or.ApplicationId == message.ApplicationId
                && or.RoleId == message.RoleId);
        }

        public void Handle(OrganizationModified message)
        {
            _repository.UpdateWhere(
                new
                {
                    OrganizationName = message.Name
                },
                or => or.OrganizationId == message.OrganizationId);
        }

        public void Handle(OrganizationDeleted message)
        {
            _repository.DeleteWhere(or => or.OrganizationId == message.OrganizationId);
        }
    }
}
