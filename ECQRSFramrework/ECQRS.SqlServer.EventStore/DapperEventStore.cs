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
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using ECQRS.Commons.Events;
using ECQRS.Commons.Exceptions;
using ECQRS.Commons.Interfaces;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using ECQRS.SqlServer.Repositories;

namespace ECQRS.SqlServer.EventStore
{
    public class DapperEventStore : IECQRSService, IEventStore
    {
        private ConcurrentDictionary<string, Type> _foundedTypes = new ConcurrentDictionary<string, Type>();

        private readonly IEventPublisher _publisher;

        public DapperEventStore(IEventPublisher publisher)
        {
            _publisher = publisher;
        }

        private const string TABLE_NAME = "Event";

        public void SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion)
        {

            using (var db = new SqlConnection(StartupInitializer.ConnectionString))
            {
                db.Open();
                using (var tc = db.BeginTransaction())
                {

                    var lastEvent = db.Query<EventEntity>(
                        string.Format("SELECT * FROM [{0}] WHERE AggregateId=@aggregateId ORDER BY Version DESC",
                            TABLE_NAME),
                        new { aggregateId = aggregateId }, transaction: tc).FirstOrDefault();


                    // check whether latest event version matches current aggregate version
                    // otherwise -> throw exception
                    if (lastEvent != null && lastEvent.Version != expectedVersion && expectedVersion != -1)
                    {
                        throw new ConcurrencyException();
                    }
                    var i = lastEvent==null?expectedVersion: lastEvent.Version;



                    // iterate through current aggregate events increasing version with each processed event
                    foreach (var @event in events)
                    {
                        i++;
                        @event.Version = i;

                        // push event to the event descriptors list for current aggregate
                        DapperRepository.InsertGeneric(db, TABLE_NAME, new EventEntity
                        {
                            AggregateId = aggregateId,
                            Timestamp = DateTime.UtcNow,
                            EventData = JsonConvert.SerializeObject(@event),
                            Id = Guid.NewGuid(),
                            TypeName = @event.GetType().Name,
                            Version = i
                        }, tc);

                        // publish current event to the bus for further processing by subscribers
                        _publisher.Publish(@event);

                    }

                    tc.Commit();
                }
            }
        }

        // collect all processed events for given aggregate and return them as a list
        // used to build up an aggregate from its history (Domain.LoadsFromHistory)
        public IEnumerable<Event> GetEventsForAggregate(Guid aggregateId, bool untilFirstSnapshot = false)
        {
            using (var db = new SqlConnection(StartupInitializer.ConnectionString))
            {
                db.Open();

                var sql = string.Format("SELECT * FROM [{0}] WHERE AggregateId=@aggregateId ORDER BY Version ASC",
                    TABLE_NAME);

                if (untilFirstSnapshot)
                {
                    sql = string.Format("SELECT * FROM [{0}] WHERE AggregateId=@aggregateId " +
                        " AND Version >= (SELECT COALESCE(MAX(Version),0) FROM [{0}] WHERE  AggregateId=@aggregateId " +
                        " AND TypeName='" + typeof(SnapshotCreated).Name + "') " +
                        "ORDER BY Version DESC",
                        TABLE_NAME);
                }
                var events = db.Query<EventEntity>(
                    sql,
                    new
                    {
                        aggregateId = aggregateId
                    });

                var somethingFounded = false;

                foreach (EventEntity @event in events)
                {
                    somethingFounded = true;
                    Type type = null;
                    if (!_foundedTypes.ContainsKey(@event.TypeName))
                    {
                        type = FindType(@event.TypeName);
                        _foundedTypes[@event.TypeName] = type;
                    }
                    type = _foundedTypes[@event.TypeName];
                    yield return (Event)JsonConvert.DeserializeObject(@event.EventData, type);
                };

                if (!somethingFounded)
                {
                    throw new AggregateNotFoundException();
                }
            }
        }

        private static Type FindType(string className)
        {
            var ass = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).ToList();

            for (var i = ass.Count - 1; i >= 0; i--)
            {
                var type = ass[i].GetTypes().FirstOrDefault(t => t.Name == className);
                if (type != null)
                {
                    return type;
                }
            }
            throw new TypeLoadException(string.Format("Unable to find item of type '{0}'.", className));
        }
    }
}
