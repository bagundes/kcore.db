namespace KCore.DB.Base
{
    public abstract class InternalCodeTable : KCore.Base.BaseTable_v1
    {
        public string code;

        public InternalCodeTable(string table, string[] pKey, bool ai) : base(KCore.R.DataSource, table, pKey)
        {
        }
    }

    public abstract class InternalIDTable : KCore.Base.BaseTable_v1
    {
        public int id;
        public InternalIDTable(string table, string[] pKey, bool ai) : base(KCore.R.DataSource, table, pKey)
        {
        }
    }
}
