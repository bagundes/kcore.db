namespace KCore.DB.Scripts
{
    public interface IInsert
    {
        string Model<T>(T model, out dynamic[] pks) where T : KCore.Base.BaseTable_v1;
    }
}
