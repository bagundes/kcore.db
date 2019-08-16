using K.Core;
using K.Core.Base;
using K.Core.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using static K.Core.C.Database;

namespace K.DB.Properties
{
    public static class Columns
    {
        public static string LOG => typeof(Columns).Name;

        public static List<ColumnStruct> ColList { get; internal set; }


        /// <summary>
        /// Load Columns type in cache;
        /// </summary>
        /// <param name="dsource"></param>
        /// <param name="table"></param>
        private static void Load(string dsource, string table)
        {
            if (ColList == null || !ColList.Where(t => t.DBase.Equals(dsource, StringComparison.InvariantCultureIgnoreCase)
                    && t.Table.Equals(table, StringComparison.InvariantCultureIgnoreCase)).Any())
            {
                ColList = ColList ?? new List<ColumnStruct>();
                using (var client = (IBaseClient)Activator.CreateInstance(DB.Factory.__client, new object[] { true }))
                {
                    try
                    {
                        ColList.AddRange(client.Columns(dsource, table));
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

        public static bool HasColumn(string dsource, string table, string colName)
        {
            Load(dsource, table);

            return ColList.Where(t => t.DBase.Equals(dsource, StringComparison.InvariantCultureIgnoreCase)
                && t.Table.Equals(table, StringComparison.InvariantCultureIgnoreCase)
                && t.Name.Equals(colName, StringComparison.InvariantCultureIgnoreCase)).Any();
        }

        public static string[] ColumnsList(string dsource, string table)
        {
            Load(dsource, table);
            return ColList
                .Where(t => t.DBase.Equals(dsource, StringComparison.InvariantCultureIgnoreCase)
                        && t.Table.Equals(table, StringComparison.InvariantCultureIgnoreCase))
                .Select(t => t.Name).ToArray();

        }

        public static ColumnStruct[] ColumnsList<T>(T model) where T : K.Core.Base.BaseTable
        {
            Load(model.TabInfo.DataSource, model.TabInfo.Table);
            var ret = new List<ColumnStruct>();

            var columns = ColList
                .Where(t => t.DBase.Equals(model.TabInfo.DataSource, StringComparison.InvariantCultureIgnoreCase)
                        && t.Table.Equals(model.TabInfo.Table, StringComparison.InvariantCultureIgnoreCase)).ToArray();


            foreach (var col in K.Core.Reflection.GetFields(model))
            {
                var foo = columns.Where(t => t.Name.Equals(col.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (foo != null)
                    ret.Add(foo);
            }
                

            return ret.ToArray();
        }
    }
}
