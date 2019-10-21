namespace KCore.DB.Scripts
{
    public interface ICreate
    {
        //void AddColumn(string colName, KCore.C.Database.ColumnType colType, bool request, int? size = null);
        void AddColumnRequere(string colName, KCore.C.Database.ColumnType colType, dynamic def = null, int? size = null);
        void AddColumnNoRequere(string colName, KCore.C.Database.ColumnType colType, dynamic def = null, int? size = null);
        void ConstraintPK(params string[] columns);
        void Create();
    }
}
