using System;
using System.Collections.Generic;
using System.Text;

namespace KCore.DB.Factory.Properties
{
    public static class Table
    {
        public static bool Exists(string dbase, string table)
        {
            using (var client = Factory.Connection.GetClient(dbase))
            {

                return client.HasTable(dbase, table);
            }
        }
    }
}
