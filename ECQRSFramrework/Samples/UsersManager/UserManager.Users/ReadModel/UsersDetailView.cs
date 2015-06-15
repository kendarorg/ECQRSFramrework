// ===========================================================
// Copyright (c) 2014-2015, Enrico Da Ros/kendar.org
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// ===========================================================


using ECQRS.Commons.Events;
using ECQRS.Commons.Interfaces;
using ECQRS.Commons.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManager.Core.Users.Events;

namespace UserManager.Core.Users.ReadModel
{
    public class UserDetailItem : IEntity
    {
        public UserDetailItem()
        {

        }
        [AutoGen(false)]
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string EMail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Deleted { get; set; }

        public UserDetailItem(Guid id, string userName, string eMail)
        {
            Id = id;
            UserName = userName;
            EMail = eMail;
        }
    }



    public class UsersDetailView : IEventView
    {
        private readonly IRepository<UserDetailItem> _repository;

        public UsersDetailView(IRepository<UserDetailItem> repository)
        {
            _repository = repository;
        }

        public void Handle(UserCreated message)
        {
            _repository.Save(new UserDetailItem
            {
                Id = message.UserId,
                UserName = message.UserName,
                EMail = message.EMail,
                FirstName = message.FirstName,
                LastName = message.LastName
            });
        }

        public void Handle(UserDeleted message)
        {
            _repository.UpdateWhere(new
            {
                Deleted = true,
            }, x=>x.Id == message.UserId);
        }

        public void Handle(UserModified message)
        {
            var item = _repository.Get(message.UserId);
            item.UserName = message.UserName;
            item.EMail = message.EMail;
            item.FirstName = message.FirstName;
            item.LastName = message.LastName;

            _repository.Update(item);
        }
    }
}
