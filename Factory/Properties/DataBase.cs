using System;
using System.Collections.Generic;
using System.Text;

namespace KCore.DB.Factory.Properties
{
    public static class DataBase
    {
        /// <summary>
        /// Return database version
        /// </summary>
        /// <returns></returns>
        public static int Version()
        {
            using(var client = Connection.GetClient(null))
                return client.Version();
            
        }

        /// <summary>
        /// Return type of client.
        /// </summary>
        /// <returns>SQLClient/HanaClient/...</returns>
        public static KCore.C.Database.ClientType ClientType()
        {
            using (var client = Connection.GetClient(null))
                return client.ClientType;
        }

        /// <summary>
        /// Return which type of database.
        /// </summary>
        /// <returns>MSQL/HANA/...</returns>
        public static KCore.C.Database.DBaseType DBaseType()
        {
            using (var client = Connection.GetClient(null))
                return client.DataInfo.DBaseType;
        }
    }
}
