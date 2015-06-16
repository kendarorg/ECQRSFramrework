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


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ECQRS.Commons.Bus;
using MassTransit.Services.Routing.Configuration;
using Newtonsoft.Json;
using MassTransit;
using MassTransit.BusConfigurators;
using System.Reflection;

namespace ECQRS.MassTransit.Bus
{
    public interface IHasUnsubscribe
    {
        UnsubscribeAction Unsubscribe { get; set; }
        void ConsumeGeneric(object obj);
    }

    public class InternalConsumer<T> : Consumes<T>.All, IHasUnsubscribe where T : class
    {
        private IBusHandler _handler;
        private MethodInfo _method;

        public UnsubscribeAction Unsubscribe { get; set; }

        public InternalConsumer()
        {

        }

        public InternalConsumer(MethodInfo method, IBusHandler handler)
        {
            _handler = handler;
            _method = method;
        }

        public void Consume(T message)
        {
            _method.Invoke(_handler, new[] { message });
        }

        public void ConsumeGeneric(object message)
        {
            _method.Invoke(_handler, new[] { message });
        }
    }

    internal class QueueInstance : IDisposable
    {
        private List<PersistentSubscription> _persistentSubscriptions;
        private readonly Dictionary<Type, Dictionary<object, IHasUnsubscribe>> _subscriptions;
        private readonly bool _multiple;
        private readonly string _queueId;
        private readonly Guid _handler;
        private readonly Dictionary<string, Type> _types;

        private Action<ServiceBusConfigurator> _configPublishContext;
        private string _massTransitRoot;
        private IServiceBus _bus;
        private bool _started;

        internal IServiceBus Bus
        {
            get
            {
                return _bus;
            }
        }

        public bool IsMultiple
        {
            get { return _multiple; }
        }

        public QueueInstance(bool multiple, string queueId, Action<ServiceBusConfigurator> configPublishContext, string massTransitRoot)
        {
            _handler = Guid.NewGuid();
            _multiple = multiple;
            _queueId = queueId;
            _subscriptions = new Dictionary<Type, Dictionary<object, IHasUnsubscribe>>();
            _types = new Dictionary<string, Type>();
            _configPublishContext = configPublishContext;
            _massTransitRoot = massTransitRoot;
            _started = false;
            _persistentSubscriptions = new List<PersistentSubscription>();
        }


        public void Start()
        {
            _bus = ServiceBusFactory.New(x =>
            {
                _configPublishContext(x);
                x.ReceiveFrom(_massTransitRoot + "/" + _queueId.Trim('/')+".rx");
                
                //http://functionsoftware.co.uk/2015/05/01/masstransit-msmq-setup-and-configuration/
                x.Subscribe(subs =>
                {
                    foreach (var sub in _persistentSubscriptions)
                    {

                        Type classType = typeof(InternalConsumer<>);
                        Type[] typeParams = new Type[] { sub.MessageType };
                        Type constructedType = classType.MakeGenericType(typeParams);

                        var consumer = (IHasUnsubscribe)Activator.CreateInstance(constructedType, new object[] { sub.Method, sub.HandlerInstance });
                        _subscriptions[sub.MessageType].Add(sub.HandlerInstance, consumer);
                        subs.Consumer(sub.MessageType, (t) => consumer).Permanent();
                        //subs.Handler()
                    }
                });
            });
            Thread.Sleep(5*1000);
            _started = true;
        }



        public void Subscribe(Type type, IBusHandler handlerInstance, string queueId)
        {
            if (!_subscriptions.ContainsKey(type))
            {
                _types[type.Name] = type;
                _subscriptions.Add(type, new Dictionary<object, IHasUnsubscribe>());
            }
            var action = handlerInstance.GetType().GetMethod("Handle", new[] { type });

            if (!_started)
            {
                _persistentSubscriptions.Add(new PersistentSubscription(type, handlerInstance, action));
                return;
            }

            /*_bus.SubscribeHandler<TIn>(msg =>
            {
                var output = hostedClassesFunc.Invoke(msg);
                var context = _bus.MessageContext<TIn>();
                context.Respond(output);
            });*/
            Type classType = typeof(InternalConsumer<>);
            Type[] typeParams = new Type[] { type };
            Type constructedType = classType.MakeGenericType(typeParams);

            var consumer = (IHasUnsubscribe)Activator.CreateInstance(constructedType, new object[] { action, handlerInstance });

            _subscriptions[type].Add(handlerInstance, consumer);
            consumer.Unsubscribe = _bus.SubscribeConsumer(type, (t) => consumer);
        }

        public void Unsubscribe(Type type, IBusHandler handlerInstance, string queueId)
        {
            if (!_subscriptions.ContainsKey(type))
            {
                return;
            }
            if (!_subscriptions[type].ContainsKey(handlerInstance))
            {
                return;
            }
            var unsub = _subscriptions[type][handlerInstance];
            _subscriptions[type].Remove(handlerInstance);
            unsub.Unsubscribe.Invoke();
        }

        public void Dispose()
        {
            _bus.Dispose();
            _bus = null;
        }

        internal void Publish(Message message)
        {
            _bus.GetEndpoint(new Uri(_massTransitRoot + "/" + _queueId.Trim('/')+".tx")).Send(message);
        }

        internal void RunSync(IEnumerable<Message> messages)
        {
            foreach (var message in messages)
            {
                var realType = message.GetType();
                if (_subscriptions.ContainsKey(realType))
                {
                    foreach (var handler in _subscriptions[realType])
                    {
                        handler.Value.ConsumeGeneric(message);
                    }
                }
            }
        }
    }
}
