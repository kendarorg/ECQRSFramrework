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


using ECQRS.Commons.Bus;
using ECQRS.Commons.Commands;
using ECQRS.Commons.Events;
using ECQRS.Commons.Interfaces;
using MassTransit;
using MassTransit.BusConfigurators;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECQRS.MassTransit.Bus
{
    public class MassTransitBus : IBus, IDisposable, IEventPublisher, ICommandSender, IECQRSService
    {
        private readonly ConcurrentDictionary<string, QueueInstance> _queues;
        

        private Action<ServiceBusConfigurator> _configPublishContext;
        private string _massTransitRoot;

        public MassTransitBus(IMassTransitConfiguration configuration)
        {
            _queues = new ConcurrentDictionary<string, QueueInstance>(StringComparer.InvariantCultureIgnoreCase);
            _configPublishContext = configuration.Configurator;
            _massTransitRoot = configuration.MassTransitRoot;
        }

        public void CreateQueue(string queueId, bool multiple = false)
        {
            if (_queues.ContainsKey(queueId))
            {
                return;
            }
            _queues[queueId] = new QueueInstance(multiple, queueId, _configPublishContext,_massTransitRoot);
        }

        public void SendMessage(Message message, string queueId = null)
        {
            SendMessageInternal(queueId, new[] { message });
        }

        public void SendMessage(IEnumerable<Message> messages, string queueId = null)
        {
            SendMessageInternal(queueId, messages);
        }

        public void SendMessageInternal(string queueId, IEnumerable<Message> messages)
        {
            if (string.IsNullOrWhiteSpace(queueId)) queueId = string.Empty;

            foreach (var message in messages)
            {
               /* _queues[queueId]
                    .Bus
                    .Publish(message, message.GetType(),
                    x => { 
                        x.SetDeliveryMode(DeliveryMode.Persistent); 
                    });*/

                _queues[queueId].Publish(message);
            }
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
            _queues[queueId].Subscribe(t, handlerInstance, queueId);
        }

        public void Unsubscribe(Type t, IBusHandler handlerInstance, string queueId = null)
        {
            _queues[queueId].Unsubscribe(t, handlerInstance, queueId);
        }

        public void Dispose()
        {
            foreach (var queue in _queues)
            {
                queue.Value.Dispose();
            }
            _queues.Clear();
        }

        public void Publish(params Event[] events)
        {
            SendMessage(events, "ecqrs.events");
        }

        public void Send(params Command[] commands)
        {
            SendMessage(commands, "ecqrs.commands");
        }


        public void Start()
        {
            foreach (var queue in _queues.Values)
            {
                queue.Start();
            }
        }
    }
}
