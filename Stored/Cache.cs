using KCore.DB.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace KCore.DB.Stored
{
    public static class Cache
    {
        private static AttributeTable[] attributes;
        public static AttributeTable[] AttributesValues
        {
            get
            {
                if (Factory.DataInfo.ServerType == KCore.C.Database.ServerType.Hana)
                {
                    Debug.WriteLine("@bfagundes - Hana doesn't support cache attributes");
                    attributes = new AttributeTable[0];
                } else
                {
                    if (attributes == null)
                        attributes = Factory.Result.Models<AttributeTable>();
                }


                return attributes;
            }
        }

    }
}
