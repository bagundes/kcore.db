using KCore.Base;
using KCore.DB.Scripts;
using System;
using System.Collections.Generic;
using System.Text;

namespace KCore.DB.Factory
{
    public static class Scripts
    {
        public static string LOG => typeof(Scripts).Name;

        /// <summary>
        /// Execute many actions to prepare the query.
        /// </summary>
        /// <param name="manipulation"></param>
        /// <param name="sql"></param>
        /// <param name="values"></param>
        public static void Prepare(bool manipulation, ref string sql, params object[] values)
        {
            var dbaseType = Properties.DataBase.DBaseType();
            MyTags.CommentLine(ref sql, dbaseType);
            MyTags.Namespace(ref sql);

            

            switch (dbaseType)
            {
                case KCore.C.Database.DBaseType.MSQL: DB.Scripts.MSQLFix.Format(ref sql, values, manipulation); break;
                case KCore.C.Database.DBaseType.Hana: DB.Scripts.HanaFix.Format(ref sql, values, manipulation); break;
                default: throw new NotImplementedException();
            }
        }

        public static void Top(int limit, ref string sql)
        {
            var dbaseType = Properties.DataBase.DBaseType();

            switch (dbaseType)
            {
                case KCore.C.Database.DBaseType.MSQL: DB.Scripts.MSQLFix.Top(limit, ref sql); break;
                case KCore.C.Database.DBaseType.Hana: DB.Scripts.HanaFix.Top(limit, ref sql); break;
                default: throw new NotImplementedException();
            }
        }

        public static ICreate Create(string database, string table, bool pkString)
        {
            var dbaseType = Properties.DataBase.DBaseType();
                switch (dbaseType)
                {
                    case KCore.C.Database.DBaseType.MSQL: return new MSQLCreate(database, table, pkString);
                    default: throw new NotImplementedException();
                }

        }

        public static ISelect Select
        {
            get
            {
                var dbaseType = Properties.DataBase.DBaseType();
                switch (dbaseType)
                {
                    case KCore.C.Database.DBaseType.MSQL: return new MSQLSelect();
                    case KCore.C.Database.DBaseType.Hana: return new HanaSelect();
                    default: throw new NotImplementedException();
                }
            }
        }

        public static IInsert Insert
        {
            get
            {
                using (var client = Connection.GetClient(null))
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
                using (var client = Connection.GetClient(null))
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
}
