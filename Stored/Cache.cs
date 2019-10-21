using KCore.DB.Model;

namespace KCore.DB.Stored
{
    public static class Cache
    {
        private static AttributeTable[] attributes;
        public static AttributeTable[] AttributesValues
        {
            get
            {
                if (Factory_v1.DataInfo.DBaseType == KCore.C.Database.DBaseType.Hana)
                {
                    Debug.Save("@bfagundes - Hana doesn't support cache attributes");
                    attributes = new AttributeTable[0];
                }
                else
                {
                    if (attributes == null)
                        attributes = Factory_v1.Result.Models<AttributeTable>();
                }


                return attributes;
            }
        }

    }
}
