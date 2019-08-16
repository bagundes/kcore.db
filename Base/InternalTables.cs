using System;
using System.Collections.Generic;
using System.Text;

namespace KCore.DB.Base
{
    public abstract class InternalCodeTable : KCore.Base.BaseTable
    {
        public string code;

        public InternalCodeTable(string table, string[] pKey, bool ai) : base(KCore.R.DataSource, table, pKey, ai)
        {
        }
    }

    public abstract class InternalIDTable : KCore.Base.BaseTable
    {
        public int id;
        public InternalIDTable(string table, string[] pKey, bool ai) : base(KCore.R.DataSource, table, pKey, ai)
        {
        }
    }
}
