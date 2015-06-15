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
using UserManager.Organizations.Events;

namespace UserManager.Commons.ReadModel
{
    public class OrganizationApplicationItem : IEntity
    {
        [AutoGen(false)]
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public bool Deleted { get; set; }
    }

    public class OrganizationApplicationsView : IEventView
    {
        private IRepository<OrganizationApplicationItem> _repository;
        public OrganizationApplicationsView(IRepository<OrganizationApplicationItem> repository)
        {
            _repository = repository;
        }

        public void Handle(OrganizationRoleAdded message)
        {
            var result = _repository.Where(ou => ou.OrganizationId == message.OrganizationId
                && ou.ApplicationId == message.ApplicationId).FirstOrDefault();
            if (result != null)
            {
                result.Deleted = false;
                _repository.Update(result);
                return;
            }
            _repository.Save(new OrganizationApplicationItem
            {
                Id = message.CorrelationId,
                ApplicationId = message.ApplicationId,
                ApplicationName = message.ApplicationName,
                OrganizationId = message.OrganizationId,
                OrganizationName = message.OrganizationName
            });
        }

        public void Handle(OrganizationDisconnectedFromApplication message)
        {
            var result = _repository.Where(ou => ou.OrganizationId == message.OrganizationId
                && ou.ApplicationId == message.ApplicationId).FirstOrDefault();

            if (result == null) return;
            _repository.UpdateWhere(new
            {
                Deleted = true,
            }, x => x.Id == result.Id);
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

        public void Handle(OrganizationModified message)
        {
            _repository.UpdateWhere(
                new
                {
                    OrganizationName = message.Name
                },
                or => or.OrganizationId == message.OrganizationId);
        }

        public void Handle(ApplicationDeleted message)
        {
            _repository.UpdateWhere(new
            {
                Deleted = true,
            }, or => or.ApplicationId == message.ApplicationId);
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
