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


using System.Collections.Concurrent;
using System.Linq.Expressions;
using ECQRS.Commons.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ECQRS.Commons;
using ECQRS.Commons.Commands;

namespace ECQRS.Commons.Domain
{
    public abstract class AggregateRoot
    {
        private readonly List<Event> _changes = new List<Event>();
        private readonly static ConcurrentDictionary<Type, Dictionary<Type, Action<AggregateRoot, Event>>> _eventHandlers =
            new ConcurrentDictionary<Type, Dictionary<Type, Action<AggregateRoot, Event>>>();

        private Guid _lastCorrelationId;

        protected Guid LastCommand
        {
            get
            {
                return _lastCorrelationId;
            }
        }

        public void SetLastCommand(Command lastCommand)
        {
            _lastCorrelationId = lastCommand.CorrelationId;
        }

        protected AggregateRoot()
        {
            if (!_eventHandlers.ContainsKey(GetType()))
            {
                var methods = GetType()
                    .GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Where(m => m.Name == "Apply" && m.GetParameters().Length == 1).ToList();
                var newDictionary = new Dictionary<Type, Action<AggregateRoot, Event>>();
                foreach (var method in methods)
                {
                    var paramType = method.GetParameters()[0].ParameterType;
                    var lambda = this.BuildAction(method);

                    newDictionary.Add(paramType, lambda);
                    _eventHandlers[GetType()] = newDictionary;
                }
            }
        }



        public int Version { get; internal set; }
        public abstract Guid Id { get; }
        public IEnumerable<Event> GetUncommittedChanges()
        {
            return _changes;
        }

        public void MarkChangesAsCommitted()
        {
            _changes.Clear();
        }

        public void LoadsFromHistory(IEnumerable<Event> history)
        {
            Version = 1;
            foreach (var e in history)
            {
                ApplyChange(e, false);
            }
        }

        protected void ApplyChange(Event @event)
        {
            ApplyChange(@event, true);
        }

        // push atomic aggregate changes to local history for further processing (EventStore.SaveEvents)
        private void ApplyChange(Event theEvent, bool isNew)
        {
            if (_eventHandlers[GetType()].ContainsKey(theEvent.GetType()))
            {
                _eventHandlers[GetType()][theEvent.GetType()](this, theEvent);
                Version++;
            }

            if (isNew) _changes.Add(theEvent);
        }

        protected void Check(bool verify, Exception ex =null)
        {
            if (verify)
            {
                if (ex == null)
                {
                    throw new AggregateException();
                }
                throw ex;
            }
        }
    }
}
