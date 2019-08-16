using System;
using System.Collections.Generic;
using System.Text;

namespace K.DB.Scripts
{
    public interface ISelect
    {
        string ByPKey<T>(T model) where T : K.Core.Base.BaseTable;
        string ByPKey<T>(params dynamic[] and) where T : K.Core.Base.BaseTable, new();
    }
}
