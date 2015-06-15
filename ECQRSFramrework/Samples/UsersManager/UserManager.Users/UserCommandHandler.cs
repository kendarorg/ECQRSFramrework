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


using ECQRS.Commons.Commands;
using ECQRS.Commons.Domain;
using ECQRS.Commons.Interfaces;
using ECQRS.Commons.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManager.Core.Users.Commands;

namespace UserManager.Core.Users
{
    public class UserCommandHandler : IECQRSService, ICommandHandler
    {
        private readonly IAggregateRepository<UserItem> _repository;

        public UserCommandHandler(
            IAggregateRepository<UserItem> repository)
        {
            _repository = repository;
        }

        public void Handle(UserCreate message)
        {
            var item = new UserItem(
                message.CorrelationId,
                message.UserId, 
                message.UserName,
                message.EMail,
                message.HashedPassword,
                message.FirstName,
                message.LastName);
            _repository.Save(item, -1);
        }

        public void Handle(UserModify message)
        {
            var item = _repository.GetById(message.UserId);
            item.SetLastCommand(message);
            item.Modify(
                message.EMail,
                message.FirstName,
                message.LastName,
                message.UserName);
            _repository.Save(item, -1);
        }

        public void Handle(UserDelete message)
        {
            var item = _repository.GetById(message.UserId);
            item.SetLastCommand(message);
            item.Delete();
            _repository.Save(item, -1);
        }

    }
}
