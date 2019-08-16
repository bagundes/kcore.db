using System;
using System.Collections.Generic;
using System.Text;

namespace K.DB.Model
{
    public sealed class AttributeTable : K.DB.Base.InternalCodeTable
    {

        public string Code => $"{TabInfo.DataSource}.{table}.{column}";
        public string table;
        public string column;
        public string source;
        public string description;
        public string formatString;
        public char typeID;
        public K.Core.C.Database.TypeID TypeID
        {
            get
            {
                return (K.Core.C.Database.TypeID)typeID;
            }
            set
            {
                typeID = (char)value;
            }
        }
        public AttributeTable() : base("Attributes", new string[]{ "Code" }, false)
        {        
            
        }

    }
}
