using System;
using System.Collections.Generic;

namespace KCore.DB
{
    public static class QManager
    {
        public static string GetMyQManager<T>(int code, T[] models, params dynamic[] parameters) where T : KCore.Base.IBaseModel_v1
        {
            var sql = MyQString(code);

            if (models != null && models.Length > 0)
                KCore.DB.Factory.MyTags.InternalTag1(ref sql, models);
            if (parameters != null && parameters.Length > 0)
                KCore.DB.Factory.MyTags.SAPParams(ref sql, parameters[0]);

            return sql;
        }
        /// <summary>
        /// Return the list of Queries saved in OUQR
        /// </summary>
        /// <param name="encrypt">Encrypt the values?</param>
        /// <param name="like">Type like</param>
        /// <param name="dbase">database name</param>
        /// <param name="clear">remove the like description in the qname</param>
        /// <param name="intnlkey">add internal key in the qname</param>
        /// <returns></returns>
        public static KCore.Model.Select_v2[] GetList(bool encrypt, string like, string dbase = null, bool clear = true, bool intnlkey = true)
        {
            var qstr = $"%{KCore.DB.C.Tags.HEADER_NAME}:{like}";
            var res = new List<KCore.Model.Select_v2>();

            var sels = Factory_v1.Result2.Select(50, encrypt, dbase, Content.queries_general.OUQR_01LISTOFQUERIES_2_QNAME_QSTRING, like, qstr);
            if (clear || intnlkey)
            {
                like = like.Replace("%", "");

                foreach (var sel in sels)
                {
                    if (clear)
                        if (sel.text.StartsWith(like))
                            sel.text = sel.text.Replace(like, "").Substring(2).Trim();

                    if (intnlkey)
                        sel.text = $"({sel.value}) {sel.text}";
                }
            }

            return sels;

        }

        public static string QString(int intrnalKey, string dbase)
        {
            return QString<KCore.Base.IBaseModel>(intrnalKey, dbase, null);
        }

        [Obsolete]
        public static string QString1<T>(int intrnalKey, string dbase, T[] models) where T : KCore.Base.IBaseModel_v1
        {
            var sql = $"SELECT \"QString\" FROM OUQR WHERE \"IntrnalKey\" = '{intrnalKey}'";
            if (models != null)
                KCore.DB.Factory.MyTags.InternalTag1(ref sql, models);


            sql = Factory_v1.Result2.Get(dbase, sql).ToString();
            KCore.DB.Factory.MyTags.InternalTag1(ref sql, models);
            return sql;
        }

        public static string QString<T>(int intrnalKey, string dbase, T[] models) where T : KCore.Base.IBaseModel
        {
            var sql = $"SELECT \"QString\" FROM OUQR WHERE \"IntrnalKey\" = '{intrnalKey}'";
            if (models != null)
                KCore.DB.Factory.MyTags.InternalTag(ref sql, models);


            sql = Factory_v1.Result2.Get(dbase, sql).ToString();
            KCore.DB.Factory.MyTags.InternalTag(ref sql, models);
            return sql;
        }

        

        public static string MyQString(int code, string dbase = null)
        {
            dbase = String.IsNullOrEmpty(dbase) ? "" : $"[{dbase}]";
            var sql = $"select U_QSTRING FROM {dbase}..[@!!_qmanager] WHERE Code = '{code}'";
            return Factory_v1.Result.First(sql).ToString();
        }

        public static KCore.DB.Model.Resultset2 Execute<T>(int code, string dbase, T[] models, params object[] values) where T : KCore.Base.IBaseModel_v1
        {
            var sql = QString1(code, dbase, models);
            return Factory_v1.Result2.Top(1000, dbase, sql, values);
        }

        public static KCore.DB.Model.Resultset2 Execute(int code, string dbase, params object[] values)
        {
            var sql = QString(code, dbase);
            return Factory_v1.Result2.Top(1000, dbase, sql, values);
        }



        /// <summary>
        /// Return the query manager string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="intrnalKey">internal number query manager</param>
        /// <param name="models">My personal tag</param>
        /// <param name="parameters">SAP parameters</param>
        /// <returns></returns>
        public static string QString<T>(int intrnalKey, string database, T[] models, params object[] parameters) where T : KCore.Base.IBaseModel_v1
        {
            var sql = QString(intrnalKey, database);

            if (models != null && models.Length > 0)
                KCore.DB.Factory.MyTags.InternalTag1(ref sql, models);
            if (parameters != null && parameters.Length > 0)
                KCore.DB.Factory.MyTags.SAPParams(ref sql, parameters[0]);

            return sql;
        }
        //public static KCore.DB.Model.Resultset2 Execute<T>(int intrnalKey, T[] models, params object[] parameters) where T : KCore.Base.IBaseModel
        //{
        //    var sql = QString<T>(intrnalKey, models, parameters);
        //    return Factory.Result2.Top(500, sql);

        //}

        //public static KCore.DB.Model.ResultSet Execute(int intrnalKey, params object[] parameters)
        //{
        //    return Execute(intrnalKey, null, parameters);
        //}

        public static bool LinkToExecute(Model.LinkTo linkto)
        {
            if (linkto.Type != C.LinkTo.Type.PRC)
                throw new NotImplementedException($"linkto.type: {linkto.Type.ToString()} is not implemeted");

            var params1 = new List<string>();


            for (int i = 0; i < linkto.Parameters.Length; i++)
                params1.Add("{" + i + "}");

            var sql = $"{linkto.Value} {String.Join(",", params1)}";

            return Factory_v1.Save.Set(sql, linkto.GetParametersValues());

        }
    }
}