using KCore.Base;
using KCore.DB.Scripts;
using KCore.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace KCore.DB
{
    [Obsolete("Use Factory namespace", true)]
    public static class Factory_v2
    {
        public static string LOG => typeof(Factory_v2).Name;

        private static Type _client;// { get; internal set;}
        internal static IBaseClient GetClient(string dbase)
        {
            var auto = String.IsNullOrEmpty(dbase);

            var client = (IBaseClient)Activator.CreateInstance(_client, new object[] { auto });
            if (!auto && client.ClientType != KCore.C.Database.ClientType.SAPClient)
                client.Connect(dbase);

            return client;
        }
        public static KCore.C.Database.ClientType ClientType;
        #region Set client
        /// <summary>
        /// Set the connection default
        /// </summary>
        public static void SetClient(Type client)
        {
            _client = client;
            ClientType = GetClient(null).ClientType;
            // Retro compatible
#pragma warning disable CS0618 // Type or member is obsolete
            KCore.DB.Factory_v1.SetClient(client);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public static void SetClient(DataInfo dataInfo)
        {
            // Retro compatible
#pragma warning disable CS0618 // Type or member is obsolete
            KCore.DB.Factory_v1.SetClient(dataInfo);
#pragma warning restore CS0618 // Type or member is obsolete

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
            ClientType = client.ClientType;
            SetClient(client.GetType());

            client.Dispose();
        }
        #endregion

        /// <summary>
        /// Prepare the comand to execute.
        /// </summary>
        internal static class Scripts
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
                using (var client = (IBaseClient)Activator.CreateInstance(_client, new object[] { true }))
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
                using (var client = (IBaseClient)Activator.CreateInstance(_client, new object[] { true }))
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
                using (var client = (IBaseClient)Activator.CreateInstance(_client, new object[] { true }))
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
                    using (var client = (IBaseClient)Activator.CreateInstance(_client, new object[] { true }))
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
                    using (var client = (IBaseClient)Activator.CreateInstance(_client, new object[] { true }))
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
                    using (var client = (IBaseClient)Activator.CreateInstance(_client, new object[] { true }))
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


        #region Result
        public static class Result
        {
            /// <summary>
            /// Return if the query has line or not.
            /// </summary>
            /// <param name="dbase">Database</param>
            /// <param name="sql">query</param>
            /// <param name="values">values of query</param>
            /// <returns>Has line or not</returns>
            public static bool Exist(string dbase, string sql, params dynamic[] values)
            {
                var auto = String.IsNullOrEmpty(dbase);

                using (var client = (IBaseClient)Activator.CreateInstance(_client, new object[] { auto }))
                {
                    if (!auto && client.ClientType != KCore.C.Database.ClientType.SAPClient)
                        client.Connect(dbase);

                    try
                    {
                        Scripts.Prepare(client.DataInfo.DBaseType, false, ref sql, values);
                        Scripts.Top(1, ref sql);

                        return client.DoQuery(sql, values);
                    }
                    catch (Exception ex)
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

                using (var client = (IBaseClient)Activator.CreateInstance(_client, new object[] { auto }))
                {
                    if (!auto && client.ClientType != KCore.C.Database.ClientType.SAPClient)
                        client.Connect(dbase);
                    try
                    {
                        Scripts.Prepare(client.DataInfo.DBaseType, false, ref sql, values);

                        Scripts.Top(1, ref sql);

                        if (client.DoQuery(sql, values))
                            return client.Field(0);
                        else
                            return Dynamic.Empty;
                    }
                    catch (Exception ex)
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
            /// <param name="limit">Limit query. 0 as limit default</param>
            /// <param name="dbase">Default database</param>
            /// <param name="sql">query</param>
            /// <param name="values">parameters of query</param>
            /// <returns></returns>
            public static ResultSet3 Top(int limit, string dbase, string sql, params dynamic[] values)
            {
                var auto = String.IsNullOrEmpty(dbase);
                limit = limit < 1 ? 1000 : limit;
                
                using (var client = (IBaseClient)Activator.CreateInstance(_client, new object[] { auto }))
                {
                    if (!auto && client.ClientType != KCore.C.Database.ClientType.SAPClient)
                        client.Connect(dbase);

                    try
                    {
                        var rs = new ResultSet3();
                        Factory_v2.Scripts.Prepare(client.DataInfo.DBaseType, false, ref sql, values);

                        Scripts.Top(limit, ref sql);

                        if (client.DoQuery(sql, values))
                        {

                            for (int i = 0; i < client.FieldCount; i++)
                                rs.AddColumn(client.Field(i).Text);

                            while (client.Next(limit))
                                for (int i = 0; i < client.FieldCount; i++)
                                    rs.AddData(client.Field(i).Value);                                    

                            return rs;
                        }
                        else
                            return null;
                    }
                    catch (Exception ex)
                    {
                        var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
                        Diagnostic.Error(R.ID, LOG, id, ex.Message);
                        throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
                    }
                }
            }
        }
        #endregion

        #region Models
        public static class Model
        {
            public static void Set<T>(ref T model) where T : KCore.Base.BaseTable_v1
            {
                using (var client = (IBaseClient)Activator.CreateInstance(_client, new object[] { true }))
                {
                    if (client.ClientType == KCore.C.Database.ClientType.SAPClient)
                        throw new NotImplementedException($"Factory2.Model.SetTable->{client.ClientType.ToString()}");

                    if (client.ClientType != KCore.C.Database.ClientType.SAPClient)
                        client.Connect(model.TableInfo.DBase);

                    String sql;
                    if(model.IsUpdate)
                        sql = Scripts.Select.ByPKeyBefore(model);
                    else
                        sql = Scripts.Select.ByPKey(model);

                    var update = Result.Exist(model.TableInfo.DBase, sql);
                    
                    if(update)
                    {
                        // Fix the pk
                        model.UpdatePK();
                        sql = Scripts.Update.Model(model);
                    } else
                    {
                        dynamic[] pk;
                        sql = Scripts.Insert.Model(model, out pk);
                    }

                    if(!String.IsNullOrEmpty(sql))
                        client.NoQuery(sql);
                }
            }

            public static T[] GetList<T>(T model) where T : KCore.Base.BaseTable_v1, new()
            {
                var sql = Scripts.Select.ByPKey(model);
                var list = new List<T>();

                var res = Factory_v2.Result.Top(0, model.TableInfo.DBase, sql);
                for (int line = 0; line < res.LinesTotal; line++)
                {
                    var m = new T();
                    m.Load(res.GetFields(line));

                    list.Add(m);
                }

                return list.ToArray();
            }
        }
        #endregion
    }
}
