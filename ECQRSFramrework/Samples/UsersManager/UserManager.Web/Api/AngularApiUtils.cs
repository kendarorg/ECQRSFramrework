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


using System.Linq.Expressions;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UserManager.Api
{
    public class Range
    {
        public Range()
        {
            From = 0;
            Count = 10;
        }
        public int From { get; set; }
        public int Count { get; set; }
    }

    public class AngularApiUtils
    {
        public static IQueryable<T> AddWhere<T>(IQueryable<T> queryable, string name, Dictionary<string, object> data,
            Expression<Func<T, bool>> exp)
        {
            if (!data.ContainsKey(name)) return queryable;
            return queryable.Where(exp);
        }
        public static Range ParseRange(string range)
        {
            if (string.IsNullOrWhiteSpace(range)) return new Range();
            range = range.Trim(new[] { '[', ']' });
            var rangeArray = range.Split(',').Select(a => a.Trim()).ToArray();
            return new Range
            {
                From = Math.Max(0, int.Parse(rangeArray[0])),
                Count = Math.Max(0, int.Parse(rangeArray[1]) - int.Parse(rangeArray[0]))
            };
        }

        public static Dictionary<String,object> ParseFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return new Dictionary<string, object>();
            return JsonConvert.DeserializeObject<Dictionary<String, object>>(filter);
        }
    }
}