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
using System.Linq;
using ECQRS.Commons.Repositories;
using System.Linq.Expressions;
using LinqToObject;
using LinqToAnything;
using System.Collections;
using System.Reflection;

namespace ECQRS.OnTheFly.Repositories
{
    public class OnTheFlyRepository<T> : IRepository<T> where T : IEntity
    {
        private static readonly ConcurrentDictionary<Guid, T> _data;
        private ConstantExpression _expression;

        static OnTheFlyRepository()
        {
            _data = new ConcurrentDictionary<Guid, T>();
        }

        public OnTheFlyRepository()
        {
            _expression = Expression.Constant(this);
        }

        public T Get(Guid id)
        {
            if (!_data.ContainsKey(id)) return default(T);
            return _data[id];
        }

        public void Update(T item)
        {
            _data[item.Id] = item;
        }

        public Guid Save(T item)
        {
            _data[item.Id] = item;
            return item.Id;
        }

        public void Delete(Guid item)
        {
            if (_data.ContainsKey(item))
            {
                ((IDictionary<Guid, T>)_data).Remove(item);
            }
        }

        public IQueryable<T> Where(string filter = null, Dictionary<string, object> param = null)
        {
            if (param != null)
            {
                throw new NotImplementedException();
            }
            return _data.Values.AsQueryable();
        }


        public IQueryable<T> Where(Expression<Func<T, bool>> expr)
        {
            return _data.Values.Where(expr.Compile()).AsQueryable();
        }

        private static Dictionary<string, object> ToPropertyDictionary(object obj)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var propertyInfo in obj.GetType().GetProperties())
                if (propertyInfo.CanRead && propertyInfo.GetIndexParameters().Length == 0)
                    dictionary[propertyInfo.Name] = propertyInfo.GetValue(obj, null);
            return dictionary;
        }

        private static Dictionary<string, MethodInfo> GetSetters()
        {
            var dictionary = new Dictionary<string, MethodInfo>();
            foreach (var propertyInfo in typeof(T).GetProperties())
                if (propertyInfo.CanRead && propertyInfo.GetIndexParameters().Length == 0)
                    dictionary[propertyInfo.Name] = propertyInfo.GetSetMethod();
            return dictionary;
        }

        public void UpdateWhere(object values, Expression<Func<T, bool>> expr)
        {
            var props = GetSetters();

            var executable = expr.Compile();
            var realValues = ToPropertyDictionary(values);
            foreach (var sel in _data.Values.Where(executable))
            {
                var item = sel;
                foreach (var prop in realValues)
                {
                    props[prop.Key].Invoke(item, new object[] { prop.Value });
                }
            }
        }

        public void DeleteWhere(Expression<Func<T, bool>> expr)
        {
            var keys = new HashSet<Guid>();
            var executable = expr.Compile();
            foreach (var sel in _data.Values.Where(executable))
            {
                keys.Add(sel.Id);
            }
            foreach (var item in keys)
            {
                T result;
                _data.TryRemove(item, out result);
            }
        }



        public IEnumerable<T> GetAll()
        {
            return _data.Values.ToList();
        }
    }
}
