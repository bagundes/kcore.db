using System;
using System.Linq;
using KCore.Base;

namespace KCore.DB.Scripts
{
    /// <summary>
    /// Create the scripts to execute in SQL Server
    /// </summary>
    public class HanaSelect : KCore.Base.BaseClass, ISelect
    {
        public string ByPKey<T>(T model) where T : KCore.Base.BaseTable_v1
        {
            var columns = KCore.DB.Factory.Properties.Column.GetList(model).Select(t => t.Name).ToArray();
            var where = new string[model.TableInfo.PKey.Length];
            var sql = $@"SELECT {"[" + String.Join("],[", columns) + "]"} FROM {model.TableInfo.Name} WHERE ";


            for (int i = 0; i < model.TableInfo.PKey.Length; i++)
                where[i] += $" { model.TableInfo.PKey[i]} = '{model.GetPKeyValue(i)}' ";

            sql += String.Join(" AND ", where);

            return sql;
        }

        public string ByPKey<T>(params dynamic[] and) where T : KCore.Base.BaseTable_v1, new()
        {
            var model = new T();
            var columns = KCore.DB.Factory.Properties.Column.GetList(model).Select(t => t.Name).ToArray();
            var where = new string[and.Length];
            var sql = $@"SELECT {"[" + String.Join("],[", columns) + "]"} FROM {model.TableInfo.Name}";

            if (and.Length < 1)
                return sql;
            else
                sql += " WHERE ";




            for (int i = 0; i < and.Length; i++)
                where[i] += and[i];

            sql += String.Join(" AND ", where);

            return sql;
        }

        public string ByPKeyBefore<T>(T model) where T : BaseTable_v1
        {
            throw new NotImplementedException();
        }

        public string ByVPKey<T>(T model) where T : BaseTable_v1
        {
            throw new NotImplementedException();
        }

        public string HasColumn(string dbase, string table, string column)
        {
            throw new NotImplementedException();
        }

        public string Where<T>(string dbase, string where) where T : BaseTable_v1, new()
        {
            throw new NotImplementedException();
        }
    }
}
