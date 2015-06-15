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
    public class OrganizationRoleItem : IEntity
    {
        [AutoGen(false)]
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public Guid RoleId { get; set; }
        public string RoleCode { get; set; }
        public bool Deleted { get; set; }
    }

    public class OrganizationRolesView : IEventView
    {
        private IRepository<OrganizationRoleItem> _repository;
        public OrganizationRolesView(IRepository<OrganizationRoleItem> repository)
        {
            _repository = repository;
        }

        public void Handle(OrganizationRoleAdded message)
        {
            var result = _repository.Where(ou => ou.OrganizationId == message.OrganizationId
                && ou.RoleId == message.RoleId).FirstOrDefault();
            if (result != null)
            {
                result.Deleted = false;
                _repository.Update(result);
                return;
            }
            _repository.Save(new OrganizationRoleItem
            {
                Id = message.CorrelationId,
                ApplicationId = message.ApplicationId,
                ApplicationName = message.ApplicationName,
                OrganizationId = message.OrganizationId,
                OrganizationName = message.OrganizationName,
                RoleCode = message.RoleCode,
                RoleId = message.RoleId
            });
        }

        public void Handle(OrganizationRoleDeleted message)
        {
            var result = _repository.Where(ou => ou.OrganizationId == message.OrganizationId
                && ou.RoleId == message.RoleId).FirstOrDefault();

            if (result == null) return;
            _repository.UpdateWhere(new
            {
                Deleted = true,
            }, x=>x.Id == result.Id);
        }

        public void Handle(ApplicationModified message)
        {
            _repository.UpdateWhere(
                new {
                    ApplicationName = message.Name
                },
                or => or.ApplicationId == message.ApplicationId);
        }

        public void Handle(ApplicationDeleted message)
        {
            _repository.UpdateWhere(new
            {
                Deleted = true,
            }, or => or.ApplicationId == message.ApplicationId);
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
            _repository.UpdateWhere(new
            {
                Deleted = true,
            }, or => or.ApplicationId == message.ApplicationId
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
            _repository.UpdateWhere(new
            {
                Deleted = true,
            }, or => or.OrganizationId == message.OrganizationId);
        }
    }
}
