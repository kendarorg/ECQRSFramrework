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

namespace ECQRS.SqlServer.Repositories
{
    public class DapperRepository
    {

        // ReSharper disable InconsistentNaming
        internal static readonly ConcurrentDictionary<Type, string[]> _params = new ConcurrentDictionary<Type, string[]>();
        internal static readonly ConcurrentDictionary<Type, Dictionary<string, Func<object, object>>> _getters = new ConcurrentDictionary<Type, Dictionary<string, Func<object, object>>>();
        internal static readonly ConcurrentDictionary<Type, bool> _autoGenPk = new ConcurrentDictionary<Type, bool>();
        internal static readonly ConcurrentDictionary<Type, string> _tables = new ConcurrentDictionary<Type, string>();
        // ReSharper restore InconsistentNaming

        public static void InitializeParsAndKeys<T>(string connectionString)
        {
            if (!_params.ContainsKey(typeof(T)) || !_autoGenPk.ContainsKey(typeof(T)))
            {
                var getters = new Dictionary<string, Func<object, object>>();
                var type = typeof(T);
                var tableName = type.Name.Replace("Entity", "");
                foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
                {
                    var autoGenAttribute = (AutoGenAttribute)Attribute.GetCustomAttributes(prop).FirstOrDefault(a => a is AutoGenAttribute);
                    if (autoGenAttribute != null)
                    {
                        _autoGenPk[type] = autoGenAttribute.Autogen;
                    }
                    else
                    {
                        _autoGenPk[type] = true;
                    }
                    if (prop.CanRead && prop.CanWrite)
                    {
                        getters[prop.Name] = ReflectionHelper.BuildGetter(typeof(T), prop);
                    }
                }
                _getters[type] = getters;
                _params[type] =
                    typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                        .Where(a => a.Name != "Id")
                        .Select(a => a.Name)
                        .ToArray();
            }
        }

        public static Guid UpdateGeneric<T>(SqlConnection connection, string table, T item, SqlTransaction transaction = null)
        {
            string[] availableFields;

            if (!_params.ContainsKey(typeof(T)))
            {
                availableFields =
                    typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(a => a.Name != "Id")
                        .Select(a => a.Name)
                        .ToArray();
            }
            else
            {
                availableFields = _params[typeof(T)];
            }
            var availableValues = availableFields.Select(f => f + "=@" + f).ToArray();

            var fieldsValues = string.Join(",", availableValues);
            var sqlQuery = string.Format(
                "UPDATE [{0}] SET {1} WHERE Id=@Id",
                table, fieldsValues);
            return connection.ExecuteScalar<Guid>(sqlQuery, item, transaction: transaction);
        }
        public static Guid UpdateGeneric<T>(string connectionString, string table, T item, SqlTransaction transaction = null)
        {
            using (var db = new SqlConnection(connectionString))
            {
                return UpdateGeneric(db, table, item, transaction);
            }
        }

        public static Guid InsertGeneric<T>(SqlConnection connection, string table, T item, SqlTransaction transaction = null)
        {

            string[] availableFields;
            var autoGen = _autoGenPk.ContainsKey(typeof(T)) && _autoGenPk[typeof(T)];
            if (!_params.ContainsKey(typeof(T)))
            {
                availableFields =
                    typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(a => a.Name != "Id" || !autoGen)
                        .Select(a => a.Name)
                        .ToArray();
            }
            else
            {
                availableFields = _params[typeof(T)];
            }
            var availableValues = availableFields.Select(f => "@" + f).ToArray();
            var fields = string.Join(",", availableFields);
            var values = string.Join(",", availableValues);
            var sqlQuery = string.Format(
                "INSERT INTO [{0}] ({1}) output INSERTED.Id VALUES({2}) ",
                //"OUTPUT inserted.Id VALUES(1)",
                table, fields, values);

            return connection.ExecuteScalar<Guid>(sqlQuery, item, transaction: transaction);
        }

        public static Guid InsertGeneric<T>(string connectionString, string table, T item, SqlTransaction transaction = null)
        {
            using (var db = new SqlConnection(connectionString))
            {
                return InsertGeneric(db, table, item, transaction);
            }
        }

        public static void BuildDefaultTable<TK>(SqlConnection connection) where TK : IEntity, new()
        {

            string pKey = "Id";
            var type = typeof(TK);
            var tableName = type.Name.Replace("Entity", "");

            var fields = new List<string>();
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var sqliteType = GetSqlType(prop);
                fields.Add("[" + prop.Name + "] " + sqliteType);
            }
            fields.Add("PRIMARY KEY(" + pKey + ")");

            var sql = string.Format("IF NOT EXISTS (SELECT * FROM [sysobjects] WHERE name='{0}' AND xtype='U') CREATE TABLE [{0}] ({1})", tableName, string.Join(",", fields));
            connection.Execute(sql);
            InitializeParsAndKeys<TK>(null);
        }

        public static void BuildDefaultTable<TK>(string connectionString) where TK : IEntity, new()
        {
            using (var db = new SqlConnection(connectionString))
            {
                BuildDefaultTable<TK>(db);
            }
        }

        private static string GetSqlType(PropertyInfo info)
        {
            Type propertyType = info.PropertyType;
            string propertyName = info.Name;
            var autoGenAttribute = (AutoGenAttribute)Attribute.GetCustomAttributes(info).FirstOrDefault(a => a is AutoGenAttribute);
            if (autoGenAttribute != null)
            {
                if (propertyType == typeof(Guid))
                {
                    if (autoGenAttribute.Autogen)
                    {
                        return "UNIQUEIDENTIFIER DEFAULT NEWID()";
                    }
                    return "UNIQUEIDENTIFIER NOT NULL";
                }
                if (propertyType == typeof(Int32) || propertyType == typeof(Int64))
                {
                    if (autoGenAttribute.Autogen)
                    {
                        return "INT NOT NULL IDENTITY (1,1)";
                    }
                    return "INT NOT NULL";
                }
            }
            else if (propertyName == "Id")
            {
                if (propertyType == typeof(Guid))
                {
                    return "UNIQUEIDENTIFIER DEFAULT NEWID()";
                }
                if (propertyType == typeof(Int32) || propertyType == typeof(Int64))
                {
                    return "INT NOT NULL IDENTITY (1,1)";
                }
            }


            var dbTypeAttribute = (DbTypeAttribute)Attribute.GetCustomAttributes(info).FirstOrDefault(a => a is DbTypeAttribute);
            if (dbTypeAttribute != null)
            {
                return dbTypeAttribute.DbType;
            }
            var lengthAttribute =
                (LengthAttribute)Attribute.GetCustomAttributes(info).FirstOrDefault(a => a is LengthAttribute);
            var length = 255;
            if (lengthAttribute != null)
            {
                length = lengthAttribute.Length;
            }

            var name = propertyType.Name;
            switch (name)
            {
                case ("Guid"):
                    return "UNIQUEIDENTIFIER";
                case ("DateTime"):
                    return "DATETIME";
                case ("Int32"):
                    return "INT";
                case ("Int64"):
                    return "BIGINT";
                case ("Boolean"):
                    return "BIT";
                default:
                    if (length > 255)
                    {
                        return "TEXT";
                    }
                    if (length < 255 && length > 0)
                    {
                        return "VARCHAR(" + length + ")";
                    }
                    return "VARCHAR(255)";
            }
        }

    }
}
