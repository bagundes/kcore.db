using KCore.Base;
using KCore.DB.Clients;
using KCore.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace KCore.DB.Factory.Properties
{
    /// <summary>
    /// Get information and attibuttes in the column.
    /// </summary>
    public static class Column
    {
        public static string LOG => typeof(Column).Name;

        #region Cache System

        /// <summary>
        /// Load Columns type in cache;
        /// </summary>
        /// <param name="dbase"></param>
        /// <param name="table"></param>
        private static async System.Threading.Tasks.Task LoadAsync(string dbase, string table)
        {

            if (KCore.Stored.Cache.ColumnsStruct == null || 
                !KCore.Stored.Cache.ColumnsStruct.Where(t => t.DBase.Equals(dbase, StringComparison.InvariantCultureIgnoreCase) 
                && t.Table.Equals(table, StringComparison.InvariantCultureIgnoreCase)).Any())
            {
                var ColList = NoCache.Structure(dbase, table);

                KCore.Stored.Cache.LoadColumnsStruct(new List<ColumnStruct>(ColList));
            }
        }

        public static bool Required<T>(T model, string colName) where T : KCore.Base.BaseTable_v1
        {
            return Required(model.TableInfo.DBase, model.TableInfo.Name, colName).Result;
        }
        public static async System.Threading.Tasks.Task<bool> Required(string dsource, string table, string colName)
        {
            await LoadAsync(dsource, table);
            var ColList = KCore.Stored.Cache.ColumnsStruct;

            return ColList.Where(t => t.DBase.Equals(dsource, StringComparison.InvariantCultureIgnoreCase)
                && t.Table.Equals(table, StringComparison.InvariantCultureIgnoreCase)
                && t.Name.Equals(colName, StringComparison.InvariantCultureIgnoreCase)).Select(t => t.Required).FirstOrDefault();
        }

        public static async System.Threading.Tasks.Task<bool> Exists(string dsource, string table, string colName)
        {
            await LoadAsync(dsource, table);
            var ColList = KCore.Stored.Cache.ColumnsStruct;

            return ColList.Where(t => t.DBase.Equals(dsource, StringComparison.InvariantCultureIgnoreCase)
                && t.Table.Equals(table, StringComparison.InvariantCultureIgnoreCase)
                && t.Name.Equals(colName, StringComparison.InvariantCultureIgnoreCase)).Any();
        }

        public static string[] GetList(string dbase, string table)
        {
            LoadAsync(dbase, table);
            var ColList = KCore.Stored.Cache.ColumnsStruct;
            return ColList
                .Where(t => t.DBase.Equals(dbase, StringComparison.InvariantCultureIgnoreCase)
                        && t.Table.Equals(table, StringComparison.InvariantCultureIgnoreCase))
                .Select(t => t.Name).ToArray();

        }

        public static ColumnStruct[] GetList<T>(T model) where T : KCore.Base.BaseTable_v1
        {
            LoadAsync(model.TableInfo.DBase, model.TableInfo.Name);
            var ret = new List<ColumnStruct>();
            var ColList = KCore.Stored.Cache.ColumnsStruct;
            var columns = ColList
                .Where(t => t.DBase.Equals(model.TableInfo.DBase, StringComparison.InvariantCultureIgnoreCase)
                        && t.Table.Equals(model.TableInfo.Name, StringComparison.InvariantCultureIgnoreCase)).ToArray();


            foreach (var col in KCore.Reflection.FilterOnlySetProperties(model))
            {
                var foo = columns.Where(t => t.Name.Equals(col.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (foo != null)
                    ret.Add(foo);
            }

            return ret.ToArray();
        }
        #endregion

        public static class NoCache
        {
            public static bool Exists(string dbase, string table, string column)
            {
                var sql = Factory.Scripts.Select.HasColumn(dbase, table, column);
                return Factory.Result.Exist(dbase, sql);
            }

            public static ColumnStruct[] Structure(string dbase, string table)
            {
                var list = new List<ColumnStruct>();
                using(var client = Connection.GetClient(dbase))
                {
                    var dbaseType = client.DataInfo.DBaseType;
                    if (dbaseType != KCore.C.Database.DBaseType.MSQL)
                        throw new NotImplementedException($"It need to implement to {dbaseType.ToString()}");

                    if(!client.DoQuery(Content.queries_msql.LOCAL_COLUMNS_4_DBASE_TABLE, dbase, table))
                        return null;
                    else
                    {
                        while(client.Next())
                        {
                            ColumnStruct column;

                            switch(client.DataInfo.DBaseType)
                            {
                                case KCore.C.Database.DBaseType.MSQL:
                                    column = new ColumnStruct(
                                        client.Field("DBase").ToString(),
                                        client.Field("Table").ToString(),
                                        client.Field("Name").ToString(),
                                        MSQLClient.GetColumnType(client.Field("Type").ToString()),
                                        client.Field("Request").ToBool(),
                                        null,
                                        client.Field("Size").ToInt(),
                                        client.Field("PK").ToBool());
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                            list.Add(column);
                        }


                        return list.ToArray();
                    }
                    
                }
            }
        }
    }
}
