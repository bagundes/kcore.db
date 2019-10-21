namespace KCore.DB.Scripts
{
    public interface IUpdate
    {
        string Model<T>(T model) where T : KCore.Base.BaseTable_v1;
    }
}
