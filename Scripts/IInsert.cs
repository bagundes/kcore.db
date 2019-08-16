using K.DB.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace K.DB.Scripts
{
    public interface IInsert
    {
        string Model<T>(T model, out dynamic[] pks) where T : K.Core.Base.BaseTable;
    }
}
