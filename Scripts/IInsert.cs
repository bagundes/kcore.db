using KCore.DB.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace KCore.DB.Scripts
{
    public interface IInsert
    {
        string Model<T>(T model, out dynamic[] pks) where T : KCore.Base.BaseTable;
    }
}
