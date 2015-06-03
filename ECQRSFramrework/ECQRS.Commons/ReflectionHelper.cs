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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ECQRS.Commons.Domain;
using ECQRS.Commons.Events;


namespace ECQRS.Commons
{
    public static class ReflectionHelper
    {
        public static Action<AggregateRoot, Event> BuildAction(this AggregateRoot root, MethodInfo method)
        {
            var paramType = method.GetParameters()[0].ParameterType;

            // Creating a parameter for the expression tree.
            ParameterExpression aggregateParam = Expression.Parameter(typeof(AggregateRoot), "aggregate");
            ParameterExpression eventParam = Expression.Parameter(typeof(Event), "eventParam");

            // Creating an expression to hold a local variable.
            ParameterExpression typedEvent = Expression.Parameter(paramType, "typedEvent");
            ParameterExpression typedAggregate = Expression.Parameter(root.GetType(), "typedAggregate");


            BlockExpression block = Expression.Block(
                // Adding a local variable.
                new[]
                {
                    typedEvent,
                    typedAggregate
                },
                Expression.Assign(typedEvent, Expression.Convert(eventParam, paramType)),
                Expression.Assign(typedAggregate, Expression.Convert(aggregateParam, root.GetType())),
                Expression.Call(typedAggregate, method, typedEvent)
                );
            return Expression.Lambda<Action<AggregateRoot, Event>>(block, new[] { aggregateParam, eventParam }).Compile();
        }

        private static readonly Expression[] NullExpression = {};

        public static Func<object, object> BuildGetter(Type type, PropertyInfo property)
        {
            // Creating a parameter for the expression tree.
            ParameterExpression targetParam = Expression.Parameter(typeof(object), "target");

            // Creating an expression to hold a local variable.
            ParameterExpression typedTarget = Expression.Parameter(type, "typedTarget");

            ParameterExpression returnParam = Expression.Parameter(typeof(object), "returnParam");
            LabelTarget returnTarget = Expression.Label(typeof(object));
            GotoExpression returnExpression = Expression.Return(returnTarget, returnParam, typeof(object));
            object pap;
            pap = Guid.NewGuid();
            BlockExpression block = Expression.Block(
                // Adding a local variable.
                new[]
                {
                    typedTarget,
                    returnParam,
                },
                Expression.Assign(typedTarget, Expression.Convert(targetParam, type)),
                Expression.Assign(returnParam, Expression.Convert(Expression.Call(typedTarget, property.GetGetMethod(),NullExpression),typeof(object))),
                returnParam
                );
            return Expression.Lambda<Func<object, object>>(block, new ParameterExpression[] { targetParam }).Compile();
        }

        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetCallingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
