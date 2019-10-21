namespace KCore.DB.Model
{
    public sealed class AttributeTable : KCore.DB.Base.InternalCodeTable
    {

        public string Code => $"{TableInfo.DBase}.{table}.{column}";
        public string table;
        public string column;
        public string source;
        public string description;
        public string formatString;
        public char typeID;
        public KCore.C.Database.TypeID TypeID
        {
            get
            {
                return (KCore.C.Database.TypeID)typeID;
            }
            set
            {
                typeID = (char)value;
            }
        }
        public AttributeTable() : base("Attributes", new string[] { "Code" }, false)
        {

        }

    }
}
