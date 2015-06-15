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
    public class OrganizationGroupUserItem : IEntity
    {
        [AutoGen(false)]
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public Guid GroupId { get; set; }
        public string GroupCode { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string EMail { get; set; }
        public bool Deleted { get; set; }
    }

    public class OrganizationGroupUsersView : IEventView
    {
        private IRepository<OrganizationGroupUserItem> _repository;
        public OrganizationGroupUsersView(IRepository<OrganizationGroupUserItem> repository)
        {
            _repository = repository;
        }

        public void Handle(UserOrganizationGroupAssociated message)
        {
            var result = _repository.Where(ou => ou.OrganizationId == message.OrganizationId
                && ou.UserId == message.UserId
                && ou.GroupId == message.GroupId).FirstOrDefault();

            if (result != null) return;
            _repository.Save(new OrganizationGroupUserItem
            {
                Id = message.CorrelationId,
                OrganizationId = message.OrganizationId,
                OrganizationName = message.OrganizationName,
                UserId = message.UserId,
                UserName = message.UserName,
                EMail = message.EMail,
                GroupId = message.GroupId,
                GroupCode = message.GroupCode
            });
        }

        public void Handle(UserOrganizationGroupDeassociated message)
        {
            var result = _repository.Where(ou => ou.OrganizationId == message.OrganizationId
                && ou.UserId == message.UserId
                && ou.GroupId == message.GroupId).FirstOrDefault();

            if (result == null) return;
            _repository.UpdateWhere(new
            {
                Deleted = true,
            }, x=>x.Id == result.Id);
        }

        public void Handle(UserOrganizationDeassociated message)
        {
            var result = _repository.Where(ou => ou.OrganizationId == message.OrganizationId
                && ou.UserId == message.UserId).FirstOrDefault();

            if (result == null) return;
            _repository.UpdateWhere(new
            {
                Deleted = true,
            },x=> x.Id == result.Id);
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


        public void Handle(OrganizationGroupModified message)
        {
            _repository.UpdateWhere(new
            {
                Code = message.Code
            }, o => o.GroupId == message.GroupId);
        }

        public void Handle(OrganizationGroupDeleted message)
        {
            _repository.UpdateWhere(new
            {
                Deleted = true,
            }, o => o.GroupId == message.GroupId);
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
