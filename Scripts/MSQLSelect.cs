using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K.DB.Scripts
{
    /// <summary>
    /// Create the scripts to execute in SQL Server
    /// </summary>
    public class MSQLSelect : K.Core.Base.BaseClass, ISelect
    {
        public string ByPKey<T>(T model) where T : K.Core.Base.BaseTable
        {
            var columns = K.DB.Properties.Columns.ColumnsList(model).Select(t => t.Name).ToArray();
            var where = new string[model.TabInfo.PKey.Length];
            var sql = $@"SELECT {"[" + String.Join("],[", columns) + "]"} FROM {model.TabInfo.DataSource}..{model.TabInfo.Table} WHERE ";
            

            for (int i = 0; i < model.TabInfo.PKey.Length; i++)
                where[i] += $" { model.TabInfo.PKey[i]} = '{model.GetPKeyValue(i)}' ";

            sql += String.Join(" AND ", where);

            return sql;
        }

        public string ByPKey<T>(params dynamic[] and) where T : K.Core.Base.BaseTable, new()
        {
            var model = new T();
            var columns = K.DB.Properties.Columns.ColumnsList(model).Select(t => t.Name).ToArray();
            var where = new string[and.Length];
            var sql = $@"SELECT {"[" + String.Join("],[", columns) + "]"} FROM {model.TabInfo.DataSource}..{model.TabInfo.Table}";

            if (and.Length < 1)
                return sql;
            else
                sql += " WHERE ";

            


            for (int i = 0; i < and.Length; i++)
                where[i] += and[i];

            sql += String.Join(" AND ", where);

            return sql;
        }
    }
}
