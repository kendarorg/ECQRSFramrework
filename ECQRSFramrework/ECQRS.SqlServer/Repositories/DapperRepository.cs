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
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Dapper;
using ECQRS.Commons;
using ECQRS.Commons.Repositories;
using System.Collections;
using System.Linq.Expressions;
using LinqToSqlServer;

namespace ECQRS.SqlServer.Repositories
{
    public class DapperRepository<T> : IRepository<T> where T : IEntity, new()
    {
        private readonly IRepositoryConfiguration _config;

        protected static readonly string[] _fieldsNames;
        protected static readonly string[] _fields;
        protected static readonly string[] _values;
        protected static readonly string[] _fieldsValues;
        protected static readonly string _table;
        protected readonly string _connString;

        public string Table { get { return _table; } }
        public static string[] Fields { get { return _fields; } }

        static DapperRepository()
        {
            _table = typeof(T).Name;

            var fields = typeof(T)
                   .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                   .Where(p => p.CanRead && p.CanWrite && p.Name != "Id")
                   .Select(p => p.Name)
                   .ToArray();

            _fieldsNames = fields.ToArray();
            _fields = fields.Select(f => "[" + f + "]").ToArray();
            _values = fields.Select(f => "@" + f).ToArray();
            var fieldsValues = new List<string>();
            for (int i = 0; i < _fields.Length; i++)
            {
                fieldsValues.Add(string.Format("{0} = {1}", _fields[i], _values[i]));
            }
            _fieldsValues = fieldsValues.ToArray();

            
        }

        public DapperRepository(IRepositoryConfiguration config)
        {
            _config = config;
            //Initialize(_table, new SqlConnection(config.ConnectionString), false);
            _connString = config.ConnectionString;
            DapperRepository.InitializeParsAndKeys<T>(_connString);
            
        }

        public T Get(Guid id)
        {
            using (var db = new SqlConnection(_connString))
            {
                return db.Query<T>(
                    string.Format("SELECT * FROM [{0}] WHERE [Id] = @id", _table),
                    new { id }).SingleOrDefault();
            }
        }

        public IQueryable<T> Where(string filter = null, Dictionary<string, object> param = null)
        {
            using (var db = new SqlConnection(_connString))
            {
                if (string.IsNullOrWhiteSpace(filter))
                {
                    return db.Query<T>(string.Format("SELECT * FROM [{0}]", _table)).AsQueryable();
                }
                return db.Query<T>(string.Format("SELECT * FROM [{0}] WHERE {1}", _table, filter), param).AsQueryable();
            }
        }

        public void Update(T item)
        {
            UpdateWhere(item,i=>i.Id ==item.Id);
        }

        public Guid Save(T item)
        {
            var autoGen = DapperRepository._autoGenPk.ContainsKey(typeof(T)) && DapperRepository._autoGenPk[typeof(T)];
            using (var db = new SqlConnection(_connString))
            {
                var fields = string.Join(",", _fields);
                if (autoGen) fields += ",[Id]";
                var values = string.Join(",", _values);
                if (autoGen) values += ",@Id";
                var sqlQuery = string.Format(
                    "INSERT INTO [{0}] ({1}) output INSERTED.Id VALUES({2}) ",
                    //"OUTPUT inserted.Id VALUES(1)",
                    _table, fields, values);
                return db.ExecuteScalar<Guid>(sqlQuery, item);
            }
        }

        public void Delete(Guid id)
        {
            DeleteWhere(a => a.Id == id);
        }



        private static Dictionary<string, object> ToPropertyDictionary(object obj)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var propertyInfo in obj.GetType().GetProperties())
                if (propertyInfo.CanRead && propertyInfo.GetIndexParameters().Length == 0)
                    dictionary[propertyInfo.Name] = propertyInfo.GetValue(obj, null);
            return dictionary;
        }


        public IQueryable<T> Where(Expression<Func<T, bool>> expr)
        {
            var db = new SqlConnection(_connString);
                         
                IQueryable<T> pq = new SqlServerQueryable<T>(_table, db, false);
                return pq.Where(expr);
            
        }

        public void UpdateWhere(object values, Expression<Func<T, bool>> expr)
        {
         
            using (var db = new SqlConnection(_connString))
            {
                var realParams = new Dictionary<string, object>();
                var target = ToPropertyDictionary(values);
                 foreach(var item in target){
                     realParams.Add(item.Key, item.Value);
                }
                
                IQueryable<T> pq = new SqlServerQueryable<T>(null, null, true);
                pq.Where(expr).ToArray();
                var result= ((SqlServerQueryable<T>)pq).Result;
                foreach(var item in result.Parameters){
                    realParams.Add(item.Key, item.Value);
                }
           

                var fieldsValues = string.Join(",", target.Select(kvp => "["+kvp.Key + "]=@" + kvp.Key));


                var sqlQuery = string.Format(
                    "UPDATE [{0}] SET {1} {2}",
                    _table, fieldsValues, result.Sql);
                if (result.Sql.Contains("[Id]=[Id]"))
                {
                    throw new Exception("AAAARG");
                }
                db.Execute(sqlQuery, realParams);

            }
        }

        public void DeleteWhere(Expression<Func<T, bool>> expr)
        {
            using (var db = new SqlConnection(_connString))
            {
                var fieldsValues = new Dictionary<string,object>();
                IQueryable<T> pq = new SqlServerQueryable<T>(null, null, true);
                pq.Where(expr).ToArray();
                var result= ((SqlServerQueryable<T>)pq).Result;
                foreach(var item in result.Parameters){
                    fieldsValues.Add(item.Key,item.Value);
                }

                if (result.Sql.Contains("[Id]=[Id]"))
                {
                    throw new Exception("AAAARG");
                }
                db.Query<T>(
                    string.Format("DELETE FROM [{0}] {1}", _table,result.Sql),
                    fieldsValues).SingleOrDefault();
            }
        }


        public IEnumerable<T> GetAll()
        {
            using (var db = new SqlConnection(_connString))
            {
                return db.Query<T>(string.Format("SELECT * FROM [{0}]", _table));
            }
        }
    }

}