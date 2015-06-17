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


using System.Collections.Generic;
using ECQRS.Commons.Bus;
using ECQRS.Commons.Commands;
using ECQRS.Commons.Events;
using ECQRS.Commons.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using ECQRS.Commons.Interfaces;

namespace ECQRS.OnTheFly.Bus
{
    public class OnTheFlyBus : IBus, IDisposable, IEventPublisher, ICommandSender, IECQRSService
    {
        private readonly ConcurrentDictionary<string, QueueInstance> _queues;

        public OnTheFlyBus()
        {
            _queues = new ConcurrentDictionary<string, QueueInstance>(StringComparer.InvariantCultureIgnoreCase);
        }

        public void Subscribe<T>(IBusHandler target, string queueId = null) where T : Message
        {
            Subscribe(typeof(T), target, queueId);
        }

        public void Unsubscribe<T>(IBusHandler target, string queueId = null) where T : Message
        {
            Unsubscribe(typeof(T), target, queueId);
        }

        public void Subscribe(Type t, IBusHandler handlerInstance, string queueId = null)
        {
            if (string.IsNullOrWhiteSpace(queueId)) queueId = string.Empty;
            if (!_queues.ContainsKey(queueId))
            {
                throw new MissingQueueException(queueId);
            }

            _queues[queueId].Subscribe(t, handlerInstance, queueId);
        }

        public void Unsubscribe(Type t, IBusHandler handlerInstance, string queueId = null)
        {
            if (string.IsNullOrWhiteSpace(queueId)) queueId = string.Empty;
            if (!_queues.ContainsKey(queueId))
            {
                return;
            }
            _queues[queueId].Unsubscribe(t, handlerInstance, queueId);
        }

        public void SendMessage(Message message, string queueId = null)
        {
            SendMessageInternalSync(queueId, new[] { message });
        }

        public void SendMessage(IEnumerable<Message> messages, string queueId = null)
        {
            SendMessageInternalSync(queueId, messages);
        }

        public void SendMessageSync(Message message, string queueId = null)
        {
            SendMessageInternalSync(queueId, new[] { message });
        }

        public void SendMessageSync(IEnumerable<Message> messages, string queueId = null)
        {
            SendMessageInternalSync(queueId, messages);
        }

        public void Dispose()
        {
            
        }

        public void CreateQueue(string queueId, bool multiple = false)
        {
            if (_queues.ContainsKey(queueId))
            {
                throw new DuplicateQueueException(queueId);
            }
            _queues[queueId] = new QueueInstance(multiple);

        }

        public void Publish(params Event[] events)
        {
            try
            {
                SendMessageSync(events, "ecqrs.events");
            }
            catch (Exception)
            {

            }
        }

        public void Send(params Command[] command)
        {
            SendMessageSync(command, "ecqrs.commands");
        }

        public void SendSync(params Command[] commands)
        {
            SendMessageSync(commands, "ecqrs.commands");
        }


        public void Start()
        {
          
        }

        public void SendMessageInternalSync(string queueId, IEnumerable<Message> messages)
        {
            if (string.IsNullOrWhiteSpace(queueId)) queueId = string.Empty;

            if (_queues.ContainsKey(queueId))
            {
                _queues[queueId].RunSync(messages);
            }
        }
    }
}
