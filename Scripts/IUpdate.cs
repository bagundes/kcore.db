using KCore.DB.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace KCore.DB.Scripts
{
    public interface IUpdate
    {
        string Model<T>(T model) where T : KCore.Base.BaseTable;
    }
}
