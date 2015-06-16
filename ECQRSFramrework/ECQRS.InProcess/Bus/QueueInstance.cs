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
using System.Diagnostics;
using System.Linq;
using ECQRS.Commons.Bus;

namespace ECQRS.InProcess.Bus
{
    internal class QueueInstance
    {
        private readonly ConcurrentQueue<object> _queue;
        private readonly Dictionary<Type, Dictionary<object, Action<object>>> _subscriptions;
        private readonly bool _multiple;

        public QueueInstance(bool multiple = false)
        {
            _multiple = multiple;
            _queue = new ConcurrentQueue<object>();
            _subscriptions = new Dictionary<Type, Dictionary<object, Action<object>>>();
        }

        public void Send<T>(T message)
        {
            _queue.Enqueue(message);
        }

        public void Run()
        {
            object item;
            while (_queue.TryDequeue(out item))
            {
                var type = item.GetType();
                if (_subscriptions.ContainsKey(type))
                {
                    var keys = _subscriptions[type].Keys.ToList();

                    foreach (var key in keys)
                    {
                        try
                        {
                            _subscriptions[type][key](item);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }
            }
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
            _subscriptions[type].Remove(handlerInstance);
        }

        public void Subscribe(Type type, IBusHandler handlerInstance, string queueId)
        {
            if (!_subscriptions.ContainsKey(type))
            {
                _subscriptions.Add(type, new Dictionary<object, Action<object>>());
            }
            var action = handlerInstance.GetType().GetMethod("Handle", new[] {type});
            _subscriptions[type].Add(handlerInstance, o => action.Invoke(handlerInstance,new []{o}));
        }

        internal void RunSync(IEnumerable<Message> messages)
        {
            foreach (var message in messages)
            {
                var type = message.GetType();
                if (_subscriptions.ContainsKey(type))
                {
                    var keys = _subscriptions[type].Keys.ToList();

                    foreach (var key in keys)
                    {
                        _subscriptions[type][key](message);
                    }
                }
            }
        }
    }
}
