using KCore.Base;
using KCore.DB.Scripts;
using KCore.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace KCore.DB
{
    [Obsolete]
    public static partial class Factory_v1
    {
        public static string LOG => typeof(Factory_v1).Name;

        public static Type __client;// { get; internal set; }

        public static IBaseClient Client
        {
            get
            {
                return (IBaseClient)Activator.CreateInstance(__client, new object[] { true });

            }
        }

        [Obsolete("Temp")]
        public static T[] LoadingArray<T>(string where = null, int limit = 1000) where T : KCore.Base.BaseTable_v1, new()
        {
            var t = new T();
            var sql = $"SELECT TOP {limit} * FROM [{t.TableInfo.Name}]";
            if (!String.IsNullOrEmpty(where))
                sql += $" WHERE {where}";

            using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
            {
                client.DoQuery(sql);
                var listModel = new List<T>();

                var r = 0;

                while (client.Next(limit))
                {
                    var fields = client.Fields();


                    var modelType = typeof(T);
                    if (modelType != null)
                    {
                        var formConstructor = modelType.GetConstructor(Type.EmptyTypes);
                        object formClassObject = formConstructor.Invoke(new object[] { });

                        //var methodField = modelType.GetMethod("AddFields");
                        //methodField.Invoke(formClassObject, new object[] { fields });

                        foreach (var proper in KCore.Reflection.FilterOnlySetProperties(formClassObject))
                        {
                            try
                            {
                                var value = fields.Where(c => c.Key.Equals(proper.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                                if (value.Key != null && !value.Value.IsEmpty())
                                    proper.SetValue(formClassObject, value.Value.Value);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }


                        }

                        listModel.Add((T)formClassObject);
                    }
                }

                return listModel.ToArray();
            }

        }


        public static KCore.Model.DataInfo DataInfo
        {
            get
            {
                using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
                {
                    return client.DataInfo;
                }
            }
        }

        #region SetClient
        /// <summary>
        /// Set the connection default
        /// </summary>
        public static void SetClient(Type client)
        {
            __client = client;
        }

        /// <summary>
        /// Set the connection default
        /// </summary>
        /// <param name="cred"></param>
        public static void SetClient(Credential_v2 cred)
        {
            var dbtype = (KCore.C.Database.DBaseType)cred.GetProperty("DBType").ToInt();
            KCore.Base.IBaseClient client;
            switch (dbtype)
            {
                case KCore.C.Database.DBaseType.Hana:
                    client = new KCore.DB.Clients.HanaClient(); break;
                case KCore.C.Database.DBaseType.MSQL:
                    client = new KCore.DB.Clients.MSQLClient(); break;
                default:
                    throw new NotImplementedException();

            }

            client.Connect(cred);

            SetClient(client.GetType());

            client.Dispose();
        }

        [Obsolete("User the Factory2 version")]
        public static void SetClient(DataInfo dataInfo)
        {
            KCore.Base.IBaseClient client;
            switch (dataInfo.DBaseType)
            {
                case KCore.C.Database.DBaseType.Hana:
                    client = new KCore.DB.Clients.HanaClient(); break;
                case KCore.C.Database.DBaseType.MSQL:
                    client = new KCore.DB.Clients.MSQLClient(); break;
                default:
                    throw new NotImplementedException();

            }

            client.Connect(dataInfo);

            SetClient(client.GetType());

            client.Dispose();
        }
        #endregion

        public static string Source()
        {
            using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
            {
                return $"{client.DataInfo.Server}.{client.DataInfo.Schema}".ToUpper();
            };
        }

        public static KCore.C.Database.DBaseType ServerType()
        {
            using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
            {
                return client.DataInfo.DBaseType;
            };
        }

        public static class Scripts
        {
            /// <summary>
            /// Execute many actions to prepare the query.
            /// </summary>
            /// <param name="manipulation"></param>
            /// <param name="sql"></param>
            /// <param name="values"></param>
            public static void Prepare(KCore.C.Database.DBaseType type, bool manipulation, ref string sql, params object[] values)
            {
                Factory.MyTags.CommentLine(ref sql, type);
                Factory.MyTags.Namespace(ref sql);
                using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
                {
                    switch (client.DataInfo.DBaseType)
                    {
                        case KCore.C.Database.DBaseType.MSQL: DB.Scripts.MSQLFix.Format(ref sql, values, manipulation); break;
                        case KCore.C.Database.DBaseType.Hana: DB.Scripts.HanaFix.Format(ref sql, values, manipulation); break;
                        default: throw new NotImplementedException();
                    }
                }
            }

            public static void Top(int limit, ref string sql)
            {
                using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
                {
                    switch (client.DataInfo.DBaseType)
                    {
                        case KCore.C.Database.DBaseType.MSQL: DB.Scripts.MSQLFix.Top(limit, ref sql); break;
                        case KCore.C.Database.DBaseType.Hana: DB.Scripts.HanaFix.Top(limit, ref sql); break;
                        default: throw new NotImplementedException();
                    }
                }
            }

            public static ICreate Create(string database, string table, bool pkString)
            {
                using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
                {
                    switch (client.DataInfo.DBaseType)
                    {
                        case KCore.C.Database.DBaseType.MSQL: return new MSQLCreate(database, table, pkString);
                        default: throw new NotImplementedException();
                    }
                }
            }

            public static ISelect Select
            {
                get
                {
                    using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
                    {
                        switch (client.DataInfo.DBaseType)
                        {
                            case KCore.C.Database.DBaseType.MSQL: return new MSQLSelect();
                            case KCore.C.Database.DBaseType.Hana: return new HanaSelect();
                            default: throw new NotImplementedException();
                        }
                    }
                }
            }

            public static IInsert Insert
            {
                get
                {
                    using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
                    {
                        switch (client.DataInfo.DBaseType)
                        {
                            case KCore.C.Database.DBaseType.MSQL: return new MSQLInsert();
                            default: throw new NotImplementedException();
                        }
                    }
                }
            }

            public static IUpdate Update
            {
                get
                {
                    using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
                    {
                        switch (client.DataInfo.DBaseType)
                        {
                            case KCore.C.Database.DBaseType.MSQL: return new MSQLUpdate();
                            default: throw new NotImplementedException();
                        }
                    }
                }
            }
        }
        [Obsolete]
        public static class Result
        {
            public static T[] Column<T>(string sql, params dynamic[] values)
            {
                using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
                {
                    try
                    {
                        Factory_v1.Scripts.Prepare(client.DataInfo.DBaseType, false, ref sql, values);

                        if (client.DoQuery(sql, values))
                            return client.Column<T>(0);
                        else
                            return null;
                    }
                    catch (SqlException ex)
                    {
                        var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
                        Diagnostic.Error(R.ID, LOG, id, ex.Message);
                        throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
                    }
                }
            }

            public static bool Exists<T>(T model) where T : KCore.Base.BaseTable_v1
            {
                using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
                {
                    try
                    {
                        var sql = Scripts.Select.ByPKey(model);

                        return client.DoQuery(sql);
                    }
                    catch (SqlException ex)
                    {
                        var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
                        Diagnostic.Error(R.ID, LOG, id, ex.Message);
                        throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
                    }
                }
            }

            public static bool Exists(string sql, params dynamic[] values)
            {
                using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
                {
                    try
                    {
                        Factory_v1.Scripts.Prepare(client.DataInfo.DBaseType, false, ref sql, values);

                        Scripts.Top(1, ref sql);

                        return client.DoQuery(sql, values);
                    }
                    catch (SqlException ex)
                    {
                        var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
                        Diagnostic.Error(R.ID, LOG, id, ex.Message);
                        throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
                    }
                }
            }

            /// <summary>
            /// First line and column
            /// </summary>
            /// <param name="sql"></param>
            /// <param name="values"></param>
            /// <returns></returns>
            public static Dynamic First(string sql, params dynamic[] values)
            {
                using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
                {
                    try
                    {
                        Factory_v1.Scripts.Prepare(client.DataInfo.DBaseType, false, ref sql, values);

                        Scripts.Top(1, ref sql);

                        if (client.DoQuery(sql, values))
                            return client.Field(0);
                        else
                            return Dynamic.Empty;
                    }
                    catch (SqlException ex)
                    {
                        var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
                        Diagnostic.Error(R.ID, LOG, id, ex.Message);
                        throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
                    }
                }
            }


            [Obsolete]
            public static Model.ResultSet Top2(int limit, string dbase, string sql, params dynamic[] values)
            {
                sql = sql.Replace("{%credential.p.ousr.userid%}", "1");

                using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { false }))
                {
                    try
                    {
                        client.Connect(dbase);
                        Factory_v1.Scripts.Prepare(client.DataInfo.DBaseType, false, ref sql, values);

                        Scripts.Top(limit, ref sql);

                        if (client.DoQuery(sql, values))
                        {
                            var res = new Model.ResultSet();
                            for (int i = 0; i < client.FieldCount; i++)
                                res.AddColumn(client.Field(i).Text);

                            while (client.Next(limit))
                                for (int i = 0; i < client.FieldCount; i++)
                                    res.AddData(client.Field(i).Text, client.Field(i).Value);

                            res.Title = Factory.MyTags.GetHeaderTitle(ref sql);
                            res.Search = Factory.MyTags.IsSearch(ref sql);
                            return res;
                        }
                        else
                            return null;
                    }
                    catch (SqlException ex)
                    {
                        var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
                        Diagnostic.Error(R.ID, LOG, id, ex.Message);
                        throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
                    }
                }
            }

            [Obsolete]
            public static Model.ResultSet Top(int limit, string sql, params dynamic[] values)
            {

                using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { false }))
                {
                    try
                    {
                        Factory_v1.Scripts.Prepare(client.DataInfo.DBaseType, false, ref sql, values);

                        Scripts.Top(limit, ref sql);

                        if (client.DoQuery(sql, values))
                        {
                            var res = new Model.ResultSet();
                            for (int i = 0; i < client.FieldCount; i++)
                                res.AddColumn(client.Field(i).Text);

                            while (client.Next(limit))
                                for (int i = 0; i < client.FieldCount; i++)
                                    res.AddData(client.Field(i).Text, client.Field(i).Value);

                            res.Title = Factory.MyTags.GetHeaderTitle(ref sql);
                            res.Search = Factory.MyTags.IsSearch(ref sql);
                            return res;
                        }
                        else
                            return null;
                    }
                    catch (SqlException ex)
                    {
                        var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
                        Diagnostic.Error(R.ID, LOG, id, ex.Message);
                        throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
                    }
                }
            }



            public static T[] Models<T>() where T : KCore.Base.BaseTable_v1, new()
            {
                var list = new List<T>();
                var sql = Scripts.Select.ByPKey<T>();

                var modelType = typeof(T);
                var formConstructor = modelType.GetConstructor(Type.EmptyTypes);


                using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
                {
                    client.DoQuery(sql);

                    while (client.Next())
                    {

                        var fields = client.Fields();

                        if (modelType != null)
                        {


                            var methodField = modelType.GetMethod("QFields");
                            object formClassObject = formConstructor.Invoke(new object[] { });
                            methodField.Invoke(formClassObject, new object[] { fields });

                            foreach (var proper in KCore.Reflection.GetFields(formClassObject))
                            {
                                var value = fields.Where(c => c.Key.Equals(proper.Name, StringComparison.OrdinalIgnoreCase)).Select(v => v.Value.Value).FirstOrDefault();

                                try
                                {
                                    if (value != null)
                                    {
                                        if (value.Equals(System.DBNull.Value))
                                            value = null;

                                        switch (proper.FieldType.Name.ToLower())
                                        {
                                            case "char":
                                                // TODO - Change the char.parse method to char.tryparse
                                                proper.SetValue(formClassObject, char.Parse(value)); break;
                                            default:
                                                proper.SetValue(formClassObject, value); break;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw new KDBException(LOG, C.MessageEx.FatalError1_1, ex);
                                }
                            }

                            list.Add((T)formClassObject);
                        }
                    }

                    return list.ToArray();
                }
            }

            public static T Model<T>(params dynamic[] pks) where T : KCore.Base.BaseTable_v1, new()
            {
                var t = new T();
                var sql = Scripts.Select.ByPKey<T>(pks);

                var modelType = typeof(T);
                var formConstructor = modelType.GetConstructor(Type.EmptyTypes);
                object formClassObject = formConstructor.Invoke(new object[] { });

                using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
                {
                    client.DoQuery(sql);

                    if (client.Next())
                    {
                        var fields = client.Fields();

                        if (modelType != null)
                        {


                            var methodField = modelType.GetMethod("QFields");
                            methodField.Invoke(formClassObject, new object[] { fields });

                            foreach (var proper in KCore.Reflection.GetFields(formClassObject))
                            {
                                var value = fields.Where(c => c.Key.Equals(proper.Name, StringComparison.OrdinalIgnoreCase)).Select(v => v.Value.Value).FirstOrDefault();

                                try
                                {
                                    if (value != null)
                                        proper.SetValue(formClassObject, value);
                                }
                                catch (Exception ex)
                                {
                                    throw new KDBException(LOG, C.MessageEx.FatalError1_1, ex);
                                }
                            }
                        }
                    }

                    return (T)formClassObject;
                }
            }

            public static T Model<T>(Func<T, dynamic> selector) where T : KCore.Base.BaseTable_v1, new()
            {
                throw new NotImplementedException();

                //return new T();
                foreach (var foo in KCore.Reflection.GetMembers(selector.Target))
                {
                    //var bar = foo.Name;
                    var foobar = KCore.Reflection.GetMember(selector.Target, foo.Name);
                    var bar = foobar.ToString();
                }


                var a = selector.Target;

                throw new NotImplementedException();
                return new T();
            }

            public static KCore.Model.Select_v1[] SelectModel(int limit, string sql, params dynamic[] values)
            {
                var commander = String.Empty;

                using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
                {
                    try
                    {
                        if (limit > 0)
                            Scripts.Top(limit, ref sql);

                        client.DoQuery(sql, values);
                        var selects = new List<KCore.Model.Select_v1>();
                        while (client.Next(limit))
                        {
                            if (client.FieldCount > 1)
                                selects.Add(new KCore.Model.Select_v1(client.Field(0).Value, client.Field(1).ToString()));
                            else
                                selects.Add(new KCore.Model.Select_v1(client.Field(0).Value));
                        }

                        return selects.ToArray();

                    }
                    catch (SqlException ex)
                    {
                        var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
                        Diagnostic.Error(R.ID, LOG, id, ex.Message);
                        throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
                    }
                }
            }
            public static Dynamic[][] Array(int limit, string sql, params object[] values)
            {
                var list = new List<Dynamic[]>();

                using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
                {
                    try
                    {
                        if (limit > 0)
                            Scripts.Top(limit, ref sql);


                        client.DoQuery(sql, values);

                        while (client.Next(limit))
                        {
                            var dyn = new Dynamic[client.FieldCount];

                            for (int c = 0; c < dyn.Length; c++)
                                dyn[c] = client.Field(c);

                            list.Add(dyn);

                        }

                        return list.ToArray();

                    }
                    catch (SqlException ex)
                    {
                        var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
                        Diagnostic.Error(R.ID, LOG, id, ex.Message);
                        throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
                    }
                }
            }

            public static T[] Top<T>(Func<T, object> p) where T : KCore.Base.BaseTable_v1
            {


                throw new NotImplementedException();
            }
        }



        public static class Result2
        {
            /// <summary>
            /// Create the select model
            /// </summary>
            /// <param name="limit">Select top limit</param>
            /// <param name="dbase">Database</param>
            /// <param name="sql">Query</param>
            /// <param name="values">Params of query</param>
            /// <returns></returns>
            public static KCore.Model.Select_v2[] Select(int limit, bool encrypt, string dbase, string sql, params dynamic[] values)
            {
                var commander = String.Empty;

                var auto = String.IsNullOrEmpty(dbase);

                using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { auto }))
                {
                    if (!auto && client.ClientType != KCore.C.Database.ClientType.SAPClient)
                        client.Connect(dbase);

                    try
                    {
                        //client.Connect(dbase);

                        if (limit > 0)
                            Scripts.Top(limit, ref sql);

                        client.DoQuery(sql, values);
                        var selects = new List<KCore.Model.Select_v2>();
                        while (client.Next(limit))
                        {
                            if (client.FieldCount > 1)
                                selects.Add(new KCore.Model.Select_v2(client.Field(0).Value, client.Field(1).ToString(), encrypt));
                            else
                                selects.Add(new KCore.Model.Select_v2(client.Field(0).Value, encrypt));
                        }

                        return selects.ToArray();

                    }
                    catch (SqlException ex)
                    {
                        var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
                        Diagnostic.Error(R.ID, LOG, id, ex.Message);
                        throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
                    }
                }
            }

            /// <summary>
            /// Select top.
            /// </summary>
            /// <param name="limit">Limit query</param>
            /// <param name="dbase">Default database</param>
            /// <param name="sql">query</param>
            /// <param name="values">parameters of query</param>
            /// <returns></returns>
            public static Model.Resultset2 Top(int limit, string dbase, string sql, params dynamic[] values)
            {
                var auto = String.IsNullOrEmpty(dbase);

                using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { auto }))
                {
                    if (!auto && client.ClientType != KCore.C.Database.ClientType.SAPClient)
                        client.Connect(dbase);

                    try
                    {
                        client.Connect(dbase);
                        Factory_v1.Scripts.Prepare(client.DataInfo.DBaseType, false, ref sql, values);

                        Scripts.Top(limit, ref sql);

                        if (client.DoQuery(sql, values))
                        {
                            var res = new Model.Resultset2("sap");
                            for (int i = 0; i < client.FieldCount; i++)
                                res.AddColumn(client.Field(i).Text);

                            while (client.Next(limit))
                                for (int i = 0; i < client.FieldCount; i++)
                                    res.AddData(client.Field(i).Text, client.Field(i).Value);

                            res.Property.Title = Factory.MyTags.GetHeaderTitle(ref sql);

                            var foo = Factory.MyTags.GetHeaderSwitch(ref sql);
                            res.Property.Switch = (!String.IsNullOrEmpty(foo), foo);
                            res.Property.Switch = (!String.IsNullOrEmpty(foo), foo);
                            res.Property.Search = Factory.MyTags.IsSearch(ref sql);

                            if (res.Property.Switch.transfor)
                                res.SwithRowAndColumns();

                            return res;
                        }
                        else
                            return null;
                    }
                    catch (SqlException ex)
                    {
                        var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
                        Diagnostic.Error(R.ID, LOG, id, ex.Message);
                        throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
                    }
                }
            }

            /// <summary>
            /// Top1 - Return line result
            /// </summary>
            /// <param name="sql"></param>
            /// <param name="values"></param>
            /// <returns></returns>
            public static Dictionary<string, Dynamic> GetLine(string dbase, string sql, params dynamic[] values)
            {
                var auto = String.IsNullOrEmpty(dbase);

                using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { auto }))
                {
                    if (!auto && client.ClientType != KCore.C.Database.ClientType.SAPClient)
                        client.Connect(dbase);
                    try
                    {
                        Factory_v1.Scripts.Prepare(client.DataInfo.DBaseType, false, ref sql, values);


                        Scripts.Top(1, ref sql);

                        if (client.DoQuery(sql, values))
                            return client.Fields();
                        else
                            return null;
                    }
                    catch (SqlException ex)
                    {
                        var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
                        Diagnostic.Error(R.ID, LOG, id, ex.Message);
                        throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
                    }
                }
            }

            /// <summary>
            /// Return the first column and line
            /// </summary>
            /// <param name="dbase">Define database</param>
            /// <param name="sql">Query</param>
            /// <param name="values">Parameters of query</param>
            /// <returns></returns>
            public static KCore.Dynamic Get(string dbase, string sql, params dynamic[] values)
            {
                var auto = String.IsNullOrEmpty(dbase);

                using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { auto }))
                {
                    if (!auto && client.ClientType != KCore.C.Database.ClientType.SAPClient)
                        client.Connect(dbase);
                    try
                    {
                        Factory_v1.Scripts.Prepare(client.DataInfo.DBaseType, false, ref sql, values);

                        Scripts.Top(1, ref sql);

                        if (client.DoQuery(sql, values))
                            return client.Field(0);
                        else
                            return Dynamic.Empty;
                    }
                    catch (SqlException ex)
                    {
                        var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
                        Diagnostic.Error(R.ID, LOG, id, ex.Message);
                        throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
                    }
                }
            }

            public static IBaseClient Client(string sql, params dynamic[] values)
            {
                var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true });

                try
                {
                    var res = Dynamic.Empty;

                    Factory_v1.Scripts.Prepare(client.DataInfo.DBaseType, false, ref sql, values);
                    client.DoQuery(sql, values);


                    return client;
                }
                catch (SqlException ex)
                {
                    var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
                    Diagnostic.Error(R.ID, LOG, id, ex.Message);
                    throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
                }

            }
        }
        public static class Save
        {
            public static dynamic[] Set<T>(T model) where T : KCore.Base.BaseTable_v1
            {
                var exists = Factory_v1.Result.Exists(model);

                using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
                {
                    var sql = String.Empty;
                    dynamic[] pks = null;

                    try
                    {

                        if (exists)
                            sql = Scripts.Update.Model(model);
                        else
                            sql = Scripts.Insert.Model(model, out pks);

                        client.NoQuery(sql);

                        return pks;
                    }
                    catch (SqlException ex)
                    {
                        var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
                        Diagnostic.Error(R.ID, LOG, id, ex.Message);
                        throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
                    }
                }
            }

            public static bool Set(string sql, params dynamic[] values)
            {

                using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
                {
                    Scripts.Prepare(client.DataInfo.DBaseType, false, ref sql, values);

                    try
                    {
                        return client.NoQuery(sql);
                    }
                    catch (SqlException ex)
                    {
                        var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
                        Diagnostic.Error(R.ID, LOG, id, ex.Message);
                        throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
                    }
                }
            }


        }

        //public static class Create
        //{
        //    public static bool Database(string database)
        //    {
        //        using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
        //        {
        //            try
        //            {
        //                if (!client.HasDatabase(database))
        //                {
        //                    var sql = KCore.DB.Scripts.SQL1.CreateDatebase(client.DataInfo.ServerType, database);
        //                    return client.NoQuery(sql);
        //                }
        //                else
        //                    return true;
        //            }
        //            catch (SqlException ex)
        //            {
        //                var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
        //                Diagnostic.Error(R.ID, LOG, id, ex.Message);
        //                throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
        //            }
        //        }
        //    }

        //    public static bool Table(string database, string table, bool pkString = false)
        //    {
        //        using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
        //        {
        //            try
        //            {
        //                if (!client.HasTable(database, table))
        //                {
        //                    var sql = KCore.DB.Scripts.SQL1.CreateTable(client.DataInfo.ServerType, database, table, pkString);
        //                    return client.NoQuery(sql);
        //                }
        //                else
        //                    return true;
        //            }
        //            catch (SqlException ex)
        //            {
        //                var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
        //                Diagnostic.Error(R.ID, LOG, id, ex.Message);
        //                throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
        //            }
        //        }
        //    }

        //    public static bool Column(string database, string table, string colName, KCore.C.Database.ColumnType colType, bool request = false, int size = -1)
        //    {
        //        using (var client = (IBaseClient)Activator.CreateInstance(__client, new object[] { true }))
        //        {
        //            try
        //            {
        //                if (!client.HasColumn(database, table, colName))
        //                {
        //                    var sql = KCore.DB.Scripts.SQL1.AddColumn(client.DataInfo.ServerType, database, table, colName, colType, request, size);
        //                    return client.NoQuery(sql);
        //                }
        //                else
        //                    return true;
        //            }
        //            catch (SqlException ex)
        //            {
        //                var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
        //                Diagnostic.Error(R.ID, LOG, id, ex.Message);
        //                throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
        //            }
        //        }
        //    }
        //}
    }
}