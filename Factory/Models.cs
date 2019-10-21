using KCore.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace KCore.DB.Factory
{
    public static class Models
    {
        public static string LOG => typeof(Models).Name;

        public static void Set<T>(ref T model) where T : KCore.Base.BaseTable_v1
        {
            using (var client = Connection.GetClient(model.TableInfo.DBase))
            {
                //if (client.ClientType == KCore.C.Database.ClientType.SAPClient)
                //    throw new NotImplementedException($"Factory2.Model.SetTable->{client.ClientType.ToString()}");

                
                String sql;
                if (model.IsUpdate)
                    sql = Scripts.Select.ByPKeyBefore(model);
                else
                    sql = Scripts.Select.ByPKey(model);

                var update = Result.Exist(model.TableInfo.DBase, sql);

                if (update)
                {
                    // Fix the pk
                    model.UpdatePK();
                    sql = Scripts.Update.Model(model);
                }
                else
                {
                    dynamic[] pk;
                    sql = Scripts.Insert.Model(model, out pk);
                }

                if (!String.IsNullOrEmpty(sql))
                    client.NoQuery(sql);
            }
        }

        /// <summary>
        /// Get list of models using values in primary key properties.
        /// </summary>
        /// <typeparam name="T">Model type Base table</typeparam>
        /// <param name="model">model with primary key values</param>
        /// <returns></returns>
        public static T[] GetList<T>(T model) where T : KCore.Base.BaseTable_v1, new()
        {
            var sql = Scripts.Select.ByPKey(model);
            var list = new List<T>();

            var res = Factory.Result.Top(0, model.TableInfo.DBase, sql);
            for (int line = 0; line < res.LinesTotal; line++)
            {
                var m = new T();
                m.Load(res.GetFields(line));

                list.Add(m);
            }

            return list.ToArray();
        }

        /// <summary>
        /// Get list of models using values in primary key properties.
        /// </summary>
        /// <param name="where">Specify condition</param>
        /// <returns></returns>
        public static T[] GetList<T>(string dbase, string where) where T : KCore.Base.BaseTable_v1, new()
        {
            var sql = Scripts.Select.Where<T>(dbase, where);
            var list = new List<T>();

            var res = Factory.Result.Top(0, dbase, sql);
            for (int line = 0; line < res.LinesTotal; line++)
            {
                var m = new T();
                m.TableInfo.ChangeDatabase(dbase);
                m.Load(res.GetFields(line));

                list.Add(m);
            }

            return list.ToArray();
        }
    }
}
