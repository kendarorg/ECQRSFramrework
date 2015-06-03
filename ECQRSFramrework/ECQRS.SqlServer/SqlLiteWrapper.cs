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
using System.Data;

using System.Linq;
using System.Reflection;

namespace ECQRS.SqLite
{
    public static class Sc
    {
        public static object ToSqLite(DateTime o)
        {
            return o.ToUniversalTime().Ticks/TimeSpan.TicksPerMillisecond;
        }
        public static object ToSqLite(Guid o)
        {
            return o.ToString();
        }
        public static object ToSqLite(bool o)
        {
            return o ? 1 : 0;
        }
        
        public static object PropToSqLite(PropertyInfo p, object o)
        {
            if (p.PropertyType.IsEnum)
            {
                return (int)p.GetValue(o);
            }
            switch (p.PropertyType.Name)
            {
                case "DateTime":
                    var dt = (DateTime)p.GetValue(o);
                    return dt.ToUniversalTime().Ticks / TimeSpan.TicksPerMillisecond;
                case "Guid":
                    var gu = (Guid)p.GetValue(o);
                    return gu.ToString();
                case "Boolean":
                    var bo = (bool)p.GetValue(o);
                    return bo ? 1 : 0;
                default:
                    return p.GetValue(o);
            }
        }
        public static object SqLiteToProp(PropertyInfo p, object v)
        {
            if (p.PropertyType.IsEnum)
            {
                var intVal = (int)v;
                return Enum.ToObject(p.PropertyType, intVal);
            }
            switch (p.PropertyType.Name)
            {
                case "DateTime":
                    var dt = Convert.ToInt64(v);
                    return new DateTime(dt*TimeSpan.TicksPerMillisecond).ToUniversalTime();
                case "Guid":
                    var gu = (string)v;
                    return Guid.Parse(gu);
                case "Boolean":
                    var bo = (int)v;
                    return bo == 1;
                default:
                    return v;
            }
        }
    }

    public class SqLiteWrapper:IDisposable
    {
        private readonly string _connectionString;
        private SQLiteConnection _connection;

        private HashSet<string> GetFields(string tableName)
        {
            return new HashSet<string>(
                ExecuteQuery("PRAGMA table_info('" + tableName + "')").Select(a => a["name"].ToString()),
                StringComparer.InvariantCultureIgnoreCase);
        }

        private HashSet<string> GetNonPrimaryFields(string tableName)
        {
            return new HashSet<string>(
                ExecuteQuery("PRAGMA table_info('" + tableName + "')").Where(a => (int)a["pk"] == 0).Select(a => a["name"].ToString()),
                StringComparer.InvariantCultureIgnoreCase);
        }

        private HashSet<string> GetPrimaryKeys(string tableName)
        {
            return new HashSet<string>(
                ExecuteQuery("PRAGMA table_info('" + tableName + "')").Where(a => (int)a["pk"] == 1).Select(a => a["name"].ToString()),
                StringComparer.InvariantCultureIgnoreCase);
        }

        

        public SqLiteWrapper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Open()
        {
            if (_connection == null)
            {
                _connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", _connectionString));
            }
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
        }

        public void Add<T>(string tableName, T obj)
        {
            var cols = GetFields(tableName);
            var props =
                typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(a => a.CanRead && a.CanWrite && cols.Contains(a.Name)).ToList();

            var parNames = string.Join(",", props.Select(p => "@par" + p.Name));
            var parVals = props.Select(p => new SQLiteParameter("@par" + p.Name, Sc.PropToSqLite(p,obj))).ToArray();

            var sql = "INSERT INTO " + tableName + " VALUES (" + parNames + ")";
            ExecuteNonQuery(sql, parVals);
        }

        public void Delete<T>(string tableName, T obj)
        {
            var pks = GetPrimaryKeys(tableName);
            var props =
                typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(a => a.CanRead && a.CanWrite && pks.Contains(a.Name)).ToList();

            var parNames = string.Join(" AND ", props.Select(p => p.Name + " = @par" + p.Name));
            var parVals = props.Select(p => new SQLiteParameter("@par" + p.Name, Sc.PropToSqLite(p,obj))).ToArray();

            var sql = "DELETE FROM '" + tableName + "' WHERE " + parNames;
            ExecuteNonQuery(sql, parVals);
        }

        public void Update<T>(string tableName, T obj)
        {
            var pks = GetPrimaryKeys(tableName);
            var fld = GetPrimaryKeys(tableName);
            var keyProps =
                typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(a => a.CanRead && a.CanWrite && pks.Contains(a.Name)).ToList();
            var fldProps =
                typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(a => a.CanRead && a.CanWrite && fld.Contains(a.Name)).ToList();

            var fldNames = string.Join(", ", fldProps.Select(p => p.Name + " = @par" + p.Name));
            var fldVals = fldProps.Select(p => new SQLiteParameter("@par" + p.Name, Sc.PropToSqLite(p,obj)));

            var keyNames = string.Join(" AND ", keyProps.Select(p => p.Name + " = @par" + p.Name));
            var keyVals = keyProps.Select(p => new SQLiteParameter("@par" + p.Name, Sc.PropToSqLite(p,obj)));

            var allVals = new List<SQLiteParameter>();
            allVals.AddRange(fldVals);
            allVals.AddRange(keyVals);
            var sql = "UPDATE '" + tableName + "' SET " + fldNames + " WHERE " + keyNames;
            ExecuteNonQuery(sql, allVals.ToArray());
        }

        public int ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
        {
            Open();
            var command = new SQLiteCommand(sql, _connection);
            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }
            var result = command.ExecuteNonQuery();

            
            return result;
        }

        public void ExecuteNonQueryLock(string sql, params SQLiteParameter[] parameters)
        {
            ExecuteNonQuery(sql, parameters);/*
            Open();
            sql = "PRAGMA locking_mode = EXCLUSIVE;\nBEGIN EXCLUSIVE;\n" + sql + "\nCOMMIT;\nPRAGMA locking_mode = NORMAL;";

            var command = new SQLiteCommand(sql, _connection);
            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }
            command.ExecuteNonQuery();

            */
        }

        public IEnumerable<Dictionary<string, object>> ExecuteQuery(string sql, params SQLiteParameter[] parameters)
        {
            Open();
            var command = new SQLiteCommand(sql, _connection);
            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }
            var reader = command.ExecuteReader();
            var fields = reader.VisibleFieldCount;
            while (reader.Read())
            {
                var result = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
                for (var i = 0; i < fields; i++)
                {
                    result.Add(reader.GetName(i), reader.GetValue(i));
                }
                yield return result;
            }

            
        }

        public IEnumerable<T> List<T>(string sql, params SQLiteParameter[] parameters) where T : new()
        {
            Open();
            var type = typeof (T);
            var command = new SQLiteCommand(sql, _connection);
            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }
            var reader = command.ExecuteReader();
            var fields = reader.VisibleFieldCount;
            while (reader.Read())
            {
                var result = new T();
                for (var i = 0; i < fields; i++)
                {
                    var prop = type.GetProperty(reader.GetName(i));
                    var val = Sc.SqLiteToProp(prop, reader.GetValue(i));
                    prop.SetValue(result, val);
                }
                yield return result;
            }

            
        }

        public void BuildDefaultTable<T>(string pKey = "Id",string tableName = null)
        {
            var type = typeof (T);
            if (tableName == null)
            {
                tableName = type.Name.Replace("Entity", "");
            }
            var fields = new List<string>();
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var sqliteType = GetSqLiteType(prop.PropertyType);
                fields.Add(prop.Name + " " + sqliteType);
            }
            fields.Add("PRIMARY KEY(" + pKey + ")");

            var scr = string.Format("CREATE TABLE IF NOT EXISTS '{0}' ({1})", tableName, string.Join(",", fields));
            ExecuteNonQuery(scr);
        }

        private string GetSqLiteType(Type propertyType)
        {
            var name = propertyType.Name;
            switch (name)
            {
                case("DateTime"):
                    return "INT";
                case ("Int32"):
                case ("Int64"):
                case ("Boolean"):
                    return "INT";
                default:
                    return "TEXT";
            }
        }

        public T Get<T>(string tableName,Guid id) where T : new()
        {
            return List<T>(string.Format("SELECT FROM [{0}] WHERE Id=@id", tableName),
                new[] {new SQLiteParameter("@id", Sc.ToSqLite(id))}).FirstOrDefault();
        }

        public void Close()
        {
            if (_connection != null)
            {
                _connection.Close();
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
