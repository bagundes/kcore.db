using System;
using System.Collections.Generic;
using System.Text;

namespace KCore.DB.Scripts
{
    public interface ISelect
    {
        string ByPKey<T>(T model) where T : KCore.Base.BaseTable;
        string ByPKey<T>(params dynamic[] and) where T : KCore.Base.BaseTable, new();
    }
}
