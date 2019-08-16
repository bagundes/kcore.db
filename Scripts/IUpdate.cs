using K.DB.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace K.DB.Scripts
{
    public interface IUpdate
    {
        string Model<T>(T model) where T : K.Core.Base.BaseTable;
    }
}
