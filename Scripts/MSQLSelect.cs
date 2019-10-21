using System;
using System.Linq;
using KCore.Base;

namespace KCore.DB.Scripts
{
    /// <summary>
    /// Create the scripts to execute in SQL Server
    /// </summary>
    public class MSQLSelect : KCore.Base.BaseClass, ISelect
    {
        #region create select using primary key
        /// <summary>
        /// Create sql command using pk as condition.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public string ByPKey<T>(T model) where T : KCore.Base.BaseTable_v1
        { 
            var columns = KCore.DB.Factory.Properties.Column.GetList(model).Select(t => t.Name).ToArray();
            var sql = $@"SELECT {"[" + String.Join("],[", columns) + "]"} FROM [{model.TableInfo.Name}] WHERE ";
            var where = new string[model.TableInfo.PKey.Length];
            
            if (model.GetPKeyValue() == null) model.UpdatePK();

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

        /// <summary>
        /// Create select query using the data saved in Fields properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public string ByPKeyBefore<T>(T model) where T : BaseTable_v1
        {
            if (!model.IsUpdate)
                throw new NotSupportedException($"The model is not loaded by factory");

            var columns = KCore.DB.Factory.Properties.Column.GetList(model).Select(t => t.Name).ToArray();
            var sql = $@"SELECT {"[" + String.Join("],[", columns) + "]"} FROM [{model.TableInfo.Name}] WHERE ";
            var where = new string[model.VirtualPK.Length];

            if (model.GetPKeyValue() == null)
                model.UpdatePK();


            for (int i = 0; i < model.VirtualPK.Length; i++)
            {
                var val = model.Fields.Where(t => t.Key.ToUpper() == model.VirtualPK[i].ToUpper()).Select(t => t.Value).FirstOrDefault();

                if (val != null)
                    where[i] += $" { model.VirtualPK[i]} = '{val}' ";
            }
            sql += String.Join(" AND ", where);

            return sql;
        }

        public string ByVPKey<T>(T model) where T : KCore.Base.BaseTable_v1
        {
            var columns = KCore.DB.Factory.Properties.Column.GetList(model).Select(t => t.Name).ToArray();
            var where = new string[model.VirtualPK.Length];
            var sql = $@"SELECT {"[" + String.Join("],[", columns) + "]"} FROM [{model.TableInfo.Name}] WHERE ";

            if (model.GetPKeyValue() == null)
                model.UpdatePK();


            for (int i = 0; i < model.VirtualPK.Length; i++)
            {
                var val = model.GetVirtualPKeyValue(i);
                if (val != null)
                    where[i] += $" { model.VirtualPK[i]} = '{val}' ";
            }
            sql += String.Join(" AND ", where);

            return sql;
        }
        #endregion

        /// <summary>
        /// Query to verify if column exists
        /// </summary>
        /// <param name="dbase"></param>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public string HasColumn(string dbase, string table, string column)
        {
            var sql = $@"
SELECT   TOP 1 1 
FROM     [{dbase}]" + @".INFORMATION_SCHEMA.COLUMNS
WHERE    TABLE_CATALOG = {0}
    AND  TABLE_NAME = {1}
    AND  COLUMN_NAME = {2}";

            return String.Format(sql, dbase, table, column);
        }

        public string Where<T>(string dbase, string where) where T : BaseTable_v1, new()
        {
            var model = new T();
            var columns = KCore.DB.Factory.Properties.Column.GetList(dbase, model.TableInfo.Name).Select(t => t).ToArray();
            var sql = $@"SELECT {"[" + String.Join("],[", columns) + "]"} FROM [{model.TableInfo.Name}] WHERE ";
            sql += where;

            return sql;
        }
    }
}
