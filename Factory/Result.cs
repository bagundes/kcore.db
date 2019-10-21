using KCore.Base;
using KCore.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace KCore.DB.Factory
{
    public static class Result
    {
        public static string LOG => typeof(Result).Name;

        /// <summary>
        /// Return if the query has line or not.
        /// </summary>
        /// <param name="dbase">Database</param>
        /// <param name="sql">query</param>
        /// <param name="values">values of query</param>
        /// <returns>Has line or not</returns>
        public static bool Exist(string dbase, string sql, params dynamic[] values)
        {
            using (var client = Connection.GetClient(dbase))
            {         
                try
                {
                    Scripts.Prepare(false, ref sql, values);
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
            using (var client = Connection.GetClient(dbase))
            { 
                try
                {
                    Scripts.Prepare(false, ref sql, values);

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
            
            limit = limit < 1 ? 1000 : limit;

            using (var client = Connection.GetClient(dbase))
            {

                try
                {
                    var rs = new ResultSet3();
                    Scripts.Prepare(false, ref sql, values);

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

        /// <summary>
        /// Create the list of select object. The method will read only 2 
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="encrypt"></param>
        /// <param name="dbase"></param>
        /// <param name="sql"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static Select_v2[] Select(int limit, bool encrypt, string dbase, string sql, params dynamic[] values)
        {
            using (var client = Connection.GetClient(dbase))
            {
                try
                {
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
                catch (Exception ex)
                {
                    var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
                    Diagnostic.Error(R.ID, LOG, id, ex.Message);
                    throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
                }
            }
        }
    }
}
