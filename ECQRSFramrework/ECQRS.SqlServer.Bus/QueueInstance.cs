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
using Dapper;
using ECQRS.Commons.Bus;
using ECQRS.SqlServer.Repositories;
using Newtonsoft.Json;
using ECQRS.SqlServer.Bus.Entities;

namespace ECQRS.SqlServer.Bus
{
    internal class QueueInstance
    {
        private readonly Dictionary<Type, Dictionary<object, Action<object>>> _subscriptions;
        private readonly bool _multiple;
        private readonly string _queueId;
        private readonly Guid _handler;
        private readonly Dictionary<string, Type> _types;

        public bool IsMultiple
        {
            get { return _multiple; }
        }

        public QueueInstance(bool multiple, string queueId)
        {
            _handler = Guid.NewGuid();
            _multiple = multiple;
            _queueId = queueId;
            _subscriptions = new Dictionary<Type, Dictionary<object, Action<object>>>();
            _types = new Dictionary<string, Type>();
        }

        public void Run()
        {
            using (var db = new SqlConnection(StartupInitializer.ConnectionString))
            {
                db.Open();

                var cmd = new SqlCommand(string.Format(
                    "UPDATE [{0}] SET Reserved=@resId OUTPUT INSERTED.id WHERE " +
                    "QueueId=@qid AND Elaborated=@elab AND Reserved=@empty AND " +
                    "(SubscriberId=@suid OR SubscriberId='global')", DapperBus.MESSAGES_TABLE));
                cmd.Parameters.Add(new SqlParameter("@resId", _handler));
                cmd.Parameters.Add(new SqlParameter("@qid", _queueId));
                cmd.Parameters.Add(new SqlParameter("@elab", false));
                cmd.Parameters.Add(new SqlParameter("@empty", Guid.Empty));
                cmd.Parameters.Add(new SqlParameter("@suid", DapperBus.SubscriberId()));
                cmd.Connection = db;

                var itemsModifiedCount = cmd.ExecuteNonQuery();
                if (itemsModifiedCount == 0) return;



                var itemsModified =
                    db.Query<MessageEntity>(
                        string.Format(
                            "SELECT * FROM [{0}] WHERE Reserved=@resId AND QueueId=@qid AND Elaborated=@elab " +
                            "ORDER BY Timestamp ASC",
                            DapperBus.MESSAGES_TABLE),
                        new
                        {
                            resId = _handler,
                            qid = _queueId,
                            elab = false
                        }).ToList();


                foreach (var item in itemsModified)
                {
                    if (_types.ContainsKey(item.SubscriptionId))
                    {
                        var realType = _types[item.SubscriptionId];
                        if (_subscriptions.ContainsKey(realType))
                        {
                            var realMessage = JsonConvert.DeserializeObject(item.Message, realType);
                            foreach (var handler in _subscriptions[realType])
                            {
                                try
                                {
                                    handler.Value(realMessage);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex);
                                }
                            }
                        }
                    }
                    item.Elaborated = true;
                    DapperRepository.UpdateGeneric(db, DapperBus.MESSAGES_TABLE,
                        item);

                    db.ExecuteScalar(
                        string.Format(
                            "UPDATE [{0}] SET Reserved=@empty WHERE " +
                            "Reserved=@resId AND QueueId=@qid AND Elaborated=@elab",
                            DapperBus.MESSAGES_TABLE),
                        new
                        {
                            empty = Guid.Empty,
                            resId = _handler,
                            qid = _queueId,
                            elab = false
                        });
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
                _types[type.Name] = type;
                _subscriptions.Add(type, new Dictionary<object, Action<object>>());
            }
            var action = handlerInstance.GetType().GetMethod("Handle", new[] { type });
            _subscriptions[type].Add(handlerInstance, o => action.Invoke(handlerInstance, new[] { o }));
        }
    }
}
