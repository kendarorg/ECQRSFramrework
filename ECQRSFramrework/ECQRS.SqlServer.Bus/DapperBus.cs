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
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Timers;
using Dapper;
using ECQRS.Commons.Bus;
using ECQRS.Commons.Commands;
using ECQRS.Commons.Events;
using ECQRS.Commons.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using ECQRS.Commons.Interfaces;
using ECQRS.SqlServer.Repositories;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;
using ECQRS.SqlServer.Bus.Entities;

namespace ECQRS.SqlServer.Bus
{
    public class DapperBus : IBus, IDisposable, IEventPublisher, ICommandSender, IECQRSService
    {
        internal const string QUEUES_TABLE = "Queue";
        internal const string MESSAGES_TABLE = "Message";
        internal const string SUBSCRIPTIONS_TABLE = "Subscription";

        private readonly ConcurrentDictionary<string, QueueInstance> _queues;
        private readonly Timer _timer;
        private long _running = 0;

        public DapperBus()
        {
            _queues = new ConcurrentDictionary<string, QueueInstance>(StringComparer.InvariantCultureIgnoreCase);
            _timer = new Timer(500);
            _timer.Elapsed += OnTimer;
            _timer.Enabled = StartupInitializer.IsServer;
        }

        void OnTimer(object sender, ElapsedEventArgs e)
        {
            if (!StartupInitializer.IsServer) return;

            if (Interlocked.CompareExchange(ref _running, 1, 0) == 1) return;

            try
            {
                var somethingToDo = false;
                using (var db = new SqlConnection(StartupInitializer.ConnectionString))
                {
                    var res = db.Query<MessageEntity>(
                        string.Format(
                            "SELECT * FROM [{0}] WHERE Reserved=@resId AND  Elaborated=@elab",
                            DapperBus.MESSAGES_TABLE),
                            new
                            {
                                resId = Guid.Empty,
                                elab = false
                            }).FirstOrDefault();
                    somethingToDo = res != null;
                }
                if (somethingToDo)
                {
                    var keys = _queues.Keys.ToList();
                    foreach (var key in keys)
                    {
                        _queues[key].Run();
                    }
                }
            }
            catch (ThreadAbortException ta)
            {
                Debug.WriteLine(ta);
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            Interlocked.Exchange(ref _running, 0);
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

            var subscriberId = SubscriberId();

            using (var db = new SqlConnection(StartupInitializer.ConnectionString))
            {
                var result = db.Query<QueueEntity>(
                        string.Format("SELECT * FROM [{0}] WHERE QueueId=@id ", QUEUES_TABLE),
                        new { id = queueId }).FirstOrDefault();
                if (result == null)
                {
                    throw new MissingQueueException(queueId);
                }

                var subscriptions = db.Query<SubscriptionEntity>(
                    string.Format("SELECT * FROM [{0}] WHERE QueueId=@qid AND SubscriberId=@sid AND TypeName=@tn", SUBSCRIPTIONS_TABLE),
                        new
                        {
                            qid = queueId,
                            sid = subscriberId,
                            tn = t.Name
                        }).FirstOrDefault();
                if (subscriptions == null)
                {
                    DapperRepository.InsertGeneric(db, SUBSCRIPTIONS_TABLE, new SubscriptionEntity
                    {
                        LastMessage = DateTime.UtcNow,
                        QueueId = queueId,
                        SubscriberId = subscriberId,
                        TypeName = t.Name
                    });
                }
            }
            _queues[queueId].Subscribe(t, handlerInstance, queueId);
        }

        public static string SubscriberId()
        {
#if DEBUG
            return "localhost";
#else
            var subscriberId = ConfigurationManager.AppSettings["CQRS.Subscriber"];
            if (string.IsNullOrWhiteSpace(subscriberId))
            {
                subscriberId = Environment.MachineName;
                return subscriberId;
            }
#endif
        }

        public void Unsubscribe(Type t, IBusHandler handlerInstance, string queueId = null)
        {
            if (string.IsNullOrWhiteSpace(queueId)) queueId = string.Empty;

            var subscriberId = SubscriberId();

            using (var db = new SqlConnection(StartupInitializer.ConnectionString))
            {
                var result =
                    db.Query<QueueEntity>(string.Format("SELECT * FROM [{0}] WHERE QueueId=@id", QUEUES_TABLE),
                        new { id = queueId }).FirstOrDefault();
                if (result == null)
                {
                    throw new MissingQueueException(queueId);
                }

                db.ExecuteScalar(
                    string.Format("DELETE FROM [{0}] WHERE QueueId=@id AND SubscriberId=@sid AND TypeName=@tn",
                        SUBSCRIPTIONS_TABLE),
                    new
                    {
                        qid = queueId,
                        sid = subscriberId,
                        tn = t.Name
                    });
            }
            _queues[queueId].Unsubscribe(t, handlerInstance, queueId);
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

            var subscriberId = SubscriberId();
            if (_queues[queueId].IsMultiple)
            {
                using (var db = new SqlConnection(StartupInitializer.ConnectionString))
                {
                    db.Open();

                    using (var tr = db.BeginTransaction())
                    {
                        foreach (var message in messages)
                        {
                            db.ExecuteScalar(
                                string.Format("INSERT INTO [{0}] SELECT (" +
                                              "@Elaborated AS Elaborated," +
                                              "@Message AS Message," +
                                              "@QueueId= AS QueueId," +
                                              "@SubscriptionId AS SubscriptionId," +
                                              "SubscriberId," +
                                              "@Timestamp AS Timestamp," +
                                              "@Reserved AS Reserved FROM [{1}] " +
                                              "WHERE " +
                                              "QueueId = @QueueId  AND " +
                                              "TypeName = @SubscriptionId",
                                    MESSAGES_TABLE, SUBSCRIPTIONS_TABLE),
                                new
                                {
                                    Elaborated = false,
                                    Message = JsonConvert.SerializeObject(message),
                                    QueueId = queueId,
                                    SubscriptionId = message.GetType().Name,
                                    SubscriberId = subscriberId,
                                    Timestamp = DateTime.UtcNow,
                                    Reserved = Guid.Empty
                                }, tr);
                        }
                        tr.Commit();
                    }
                }
            }
            else
            {
                using (var db = new SqlConnection(StartupInitializer.ConnectionString))
                {
                    db.Open();

                    using (var tr = db.BeginTransaction())
                    {
                        foreach (var message in messages)
                        {
                            DapperRepository.InsertGeneric(db, MESSAGES_TABLE,
                                new MessageEntity
                                {
                                    Elaborated = false,
                                    Id = Guid.NewGuid(),
                                    Message = JsonConvert.SerializeObject(message),
                                    QueueId = queueId,
                                    SubscriptionId = message.GetType().Name,
                                    SubscriberId = "global",
                                    Timestamp = DateTime.UtcNow,
                                    Reserved = Guid.Empty
                                }, tr);
                        }
                        tr.Commit();
                    }

                }
            }
        }

        public void Dispose()
        {
            _running = 0;
            _timer.Enabled = false;
            _timer.Stop();
            _timer.Elapsed -= OnTimer;
        }



        public void CreateQueue(string queueId, bool multiple = false)
        {
            using (var db = new SqlConnection(StartupInitializer.ConnectionString))
            {
                var result =
                    db.Query<QueueEntity>(string.Format("SELECT * FROM [{0}] WHERE QueueId=@id", QUEUES_TABLE),
                        new { id = queueId }).FirstOrDefault();
                if (result == null)
                {
                    DapperRepository.InsertGeneric(db, QUEUES_TABLE, new QueueEntity
                    {
                        Id = Guid.NewGuid(),
                        QueueId = queueId,
                        IsMultiple = multiple
                    });
                }
            }
            if (_queues.ContainsKey(queueId))
            {
                return;
            }
            _queues[queueId] = new QueueInstance(multiple, queueId);
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
            if (StartupInitializer.IsServer)
            {
                _timer.Start();
            }
        }
    }
}
