using KCore.Base;
using KCore.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace KCore.DB.Factory
{
    public static class Connection
    {
        public static string LOG => typeof(Connection).Name;

        private static Type _client;// { get; internal set;}

        /// <summary>
        /// Get the client database connection.
        /// </summary>
        /// <param name="dbase">Database name</param>
        /// <returns>Client connection</returns>
        internal static IBaseClient GetClient(string dbase)
        {
            var auto = String.IsNullOrEmpty(dbase);

            var client = (IBaseClient)Activator.CreateInstance(_client, new object[] { auto });
            if (!auto && client.ClientType != KCore.C.Database.ClientType.SAPClient)
                client.Connect(dbase);

            return client;
        }
        

        #region Set client
        /// <summary>
        /// Set the connection default
        /// </summary>
        public static void SetClient(Type client)
        {
            _client = client;
            

            // Retro compatible
#pragma warning disable CS0618 // Type or member is obsolete
            KCore.DB.Factory_v1.SetClient(client);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public static void SetClient(DataInfo dataInfo)
        {
            // Retro compatible
#pragma warning disable CS0618 // Type or member is obsolete
            KCore.DB.Factory_v1.SetClient(dataInfo);
#pragma warning restore CS0618 // Type or member is obsolete

            KCore.Base.IBaseClient client;
            switch (dataInfo.DBaseType)
            {
                case KCore.C.Database.DBaseType.Hana:
                    client = new KCore.DB.Clients.HanaClient(); break;
                case KCore.C.Database.DBaseType.MSQL:
                    client = new KCore.DB.Clients.MSQLClient(); break;
                default:
                    throw new NotImplementedException();
            }

            try
            {
                client.Connect(dataInfo);
                SetClient(client.GetType());
            }
            finally
            {
                client.Dispose();
            }                        
        }
        #endregion
    }
}
