using ECQRS.Commons.Events;
using ECQRS.Commons.Interfaces;
using ECQRS.Commons.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManager.Core.Organizations.Events;
using UserManager.Core.Users.Commands;
using UserManager.Core.Users.Events;

namespace UserManager.Commons.ReadModel
{
    public class OrganizationUserItem : IEntity
    {
        [AutoGen(false)]
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string EMail { get; set; }
        public bool Deleted { get; set; }
    }

    public class OrganizationUsersView : IEventView
    {
        private IRepository<OrganizationUserItem> _repository;
        public OrganizationUsersView(IRepository<OrganizationUserItem> repository)
        {
            _repository = repository;
        }

        public void Handle(UserOrganizationAssociated message)
        {
            var result = _repository.Where(ou => ou.OrganizationId == message.OrganizationId
                && ou.UserId == message.UserId).FirstOrDefault();
            if (result != null)
            {
                result.Deleted = false;
                _repository.Update(result);
                return;
            }
            _repository.Save(new OrganizationUserItem
            {
                Id = message.CorrelationId,
                OrganizationId = message.OrganizationId,
                OrganizationName = message.OrganizationName,
                UserId = message.UserId,
                UserName = message.UserName,
                EMail = message.EMail
            });
        }

        public void Handle(UserOrganizationDeassociated message)
        {
            var result = _repository.Where(ou => ou.OrganizationId == message.OrganizationId
                && ou.UserId == message.UserId).FirstOrDefault();

            if (result == null) return;
            _repository.UpdateWhere(new
            {
                Deleted = true,
            }, x => x.Id == result.Id);
        }

        public void Handle(OrganizationModified message)
        {
            _repository.UpdateWhere(new
            {
                OrganizationName = message.Name
            }, o => o.OrganizationId == message.OrganizationId);
        }

        public void Handle(OrganizationDeleted message)
        {
            _repository.UpdateWhere(new
            {
                Deleted = true,
            }, o => o.OrganizationId == message.OrganizationId);
        }

        public void Handle(UserModified message)
        {
            _repository.UpdateWhere(new
            {
                UserName = message.UserName,
                EMail = message.EMail
            }, o => o.UserId == message.UserId);
        }

        public void Handle(UserDeleted message)
        {
            _repository.UpdateWhere(new
            {
                Deleted = true,
            }, o => o.UserId == message.UserId);
        }
    }
}
