namespace KCore.DB.Scripts
{
    public interface ISelect
    {
        string ByPKey<T>(T model) where T : KCore.Base.BaseTable_v1;
        string ByPKey<T>(params dynamic[] and) where T : KCore.Base.BaseTable_v1, new();
        string Where<T>(string dbase, string where) where T : KCore.Base.BaseTable_v1, new();
        string ByVPKey<T>(T model) where T : KCore.Base.BaseTable_v1;
        string ByPKeyBefore<T>(T model) where T : KCore.Base.BaseTable_v1;

        string HasColumn(string dbase, string table, string column);
    }
}
