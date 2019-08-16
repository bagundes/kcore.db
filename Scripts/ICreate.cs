using System;
using System.Collections.Generic;
using System.Text;

namespace K.DB.Scripts
{
    public interface ICreate
    {
        //void AddColumn(string colName, K.Core.C.Database.ColumnType colType, bool request, int? size = null);
        void AddColumnRequere(string colName, K.Core.C.Database.ColumnType colType, dynamic def = null, int? size = null);
        void AddColumnNoRequere(string colName, K.Core.C.Database.ColumnType colType, dynamic def = null, int? size = null);
        void ConstraintPK(params string[] columns);
        void Create();
    }
}
