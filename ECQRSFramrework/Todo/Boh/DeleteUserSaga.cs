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
using ECQRS.Commons.Events;
using ECQRS.Commons.Sagas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UserManager.Core.Users.Commands;
using UserManager.Core.Users.Events;

namespace UserManager.Commons
{
    public class DeleteUserSagaCommand:Command
    {
        public Guid UserId { get; set; }
    }
    public class DeleteUserSaga : ISaga
    {
        const int STARTED = 0;
        const int MAIN_USER_DELETED = 1;

        private ISagaManager _manager;
        public DeleteUserSaga(ISagaManager manager)
        {
            _manager = manager;
        }
        
        public void Handle(DeleteUserSagaCommand command)
        {
            _manager.Proceed(
                command.CorrelationId, 
                new UserDelete
                {
                    CorrelationId = command.CorrelationId,
                    UserId = command.UserId
                },
                /*new UserUndelete
                {
                    CorrelationId = command.CorrelationId,
                    UserId = command.UserId
                }*/ null,
                STARTED);

            //var item = new ApplicationItem(
              //  message.ApplicationId,
                //message.Name);
            //s_repository.Save(item, -1);
        }

        public void Handle(Event  userUndeleted)
        {
           // _manager.RollBack(userUndeleted.CorrelationId);
        }

        public void Handle(UserDeleted evt)
        {
            var sagaState = _manager.GetStatus(evt.CorrelationId, STARTED);
            if (sagaState != null)
            {
                _manager.Proceed(
                evt.CorrelationId,
                /*new UserApplicationDelete
                {
                    CorrelationId = command.CorrelationId,
                    UserId = command.UserId
                }*/ null,
                    /*new UserApplicatinUndelete
                    {
                        CorrelationId = command.CorrelationId,
                        UserId = command.UserId
                    }*/ null,
                MAIN_USER_DELETED);
            }
        }
    }
}
