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


using ECQRS.Commons.Domain;
using ECQRS.Commons.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UserManager.Core.Users.Aggregate;
using UserManager.Core.Users.Commands;
using UserManager.Core.Users.Events;

namespace UserManager.Core.Users
{
    public class UserItem : AggregateRoot
    {
        #region Common

        private User _user;

        public override Guid Id
        {
            get { return _user.Id; }
        }

        private void CheckDeleted()
        {
            if (_user.IsDeleted)
            {
                throw new AggregateNotFoundException();
            }
        }

        #endregion Common

        #region Constructors

        public UserItem()
        {
            _user = new User();
            // used to create in repository ... many ways to avoid this, eg making private constructor
        }

        public UserItem(Guid commandId,Guid id, string userName,string email,string hashedPassword,string firstName,string lastName)
        {
            _user = new User();
            ApplyChange(new UserCreated
            {
                CorrelationId = commandId,
                UserId = id,
                UserName = userName,
                EMail = email,
                HashedPassword = hashedPassword,
                FirstName = firstName,
                LastName = lastName
            });
        }

        public void Apply(UserCreated e)
        {
            _user.Id = e.UserId;
            _user.EMail = e.EMail;
            _user.UserName = e.UserName;
            _user.FirstName = e.FirstName;
            _user.LastName = e.LastName;
            _user.HashedPassword = e.HashedPassword;
        }

        #endregion Constructors

        #region CRUD

        public void Modify(string email, string firstName, string lastName, string userName)
        {
            CheckDeleted();
            ApplyChange(new UserModified
            {
                CorrelationId = LastCommand,
                UserId = Id,
                UserName = userName,
                EMail = email,
                FirstName = firstName,
                LastName = lastName
            });
        }

        public void Apply(UserModified e)
        {
            _user.EMail = e.EMail;
            _user.UserName = e.UserName;
            _user.FirstName = e.FirstName;
            _user.LastName = e.LastName;
        }

        public void Delete()
        {
            CheckDeleted();
            ApplyChange(new UserDeleted
            {
                CorrelationId = LastCommand,
                UserId = Id
            });
        }

        public void Apply(UserDeleted e)
        {
            _user.IsDeleted = true;
        }

        #endregion CRUD
    }
}