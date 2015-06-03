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
using ECQRS.Commons.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECQRS.Commons.Sagas
{
    public interface ISaga : ICommandHandler, IEventHandler
    {
    }

    public class SagaStep
    {
        public int Status { get; set; }
        public Command AssociatedCommand { get; set; }
        public Command RepairCommand { get; set; }
        public Event AssociatedEvent { get; set; }
    }

    public class SagaState
    {
        public SagaState()
        {
            Steps = new List<SagaStep>();
        }
        public Guid CorrelationId { get; set; }
        public List<SagaStep> Steps { get; private set; }
    }

    public interface ISagaManager
    {

        void Proceed(Guid correlationId, Command command, Command repairCommand, int status);
        SagaState GetStatus(Guid correlationId, int expectedStatus);
        void RollBack(Guid guid, int status=-1);
    }

    public abstract class BaseSagaManager : ISagaManager
    {
        protected ICommandSender Sender { get; private set; }
        protected BaseSagaManager(ICommandSender cmdSender)
        {
            Sender = cmdSender;
        }
        public void Proceed(Guid correlationId, Command command, Command repairCommand, int status)
        {
            command.CorrelationId = correlationId;
            repairCommand.CorrelationId = correlationId;
            SaveStatus(correlationId, command, repairCommand, status);
            Sender.Send(command);
        }

        protected abstract void SaveStatus(Guid correlationId, Command command,
            Command repairCommand, int status);

        public SagaState GetStatus(Guid correlationId, int expectedStatus)
        {
            var status = Find(correlationId);
            if (status == null)
            {
                throw new Exception("MISSING SAGA STATE!!");
            }
            var lastStep = status.Steps.LastOrDefault();
            if (lastStep == null)
            {
                throw new Exception("WRONG SAGA STATE!!");
            }
            if (lastStep.Status != expectedStatus)
            {
                throw new NotImplementedException("CORRECTIVE ACTION :O");
            }
            return status;
        }

        protected abstract SagaState Find(Guid correlationId);


        public void RollBack(Guid correlationId, int status = -1)
        {
            var status = Find(correlationId);
            var item = status.Steps.FirstOrDefault(st=>st.Status == -1);
            if(item==null){
                SetSagaFailed(correlationId);
            }
            var withStatus = status.Steps.IndexOf();
            throw new NotImplementedException();
        }
    }
}
