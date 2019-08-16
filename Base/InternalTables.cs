using System;
using System.Collections.Generic;
using System.Text;

namespace K.DB.Base
{
    public abstract class InternalCodeTable : K.Core.Base.BaseTable
    {
        public string code;

        public InternalCodeTable(string table, string[] pKey, bool ai) : base(K.Core.R.DataSource, table, pKey, ai)
        {
        }
    }

    public abstract class InternalIDTable : K.Core.Base.BaseTable
    {
        public int id;
        public InternalIDTable(string table, string[] pKey, bool ai) : base(K.Core.R.DataSource, table, pKey, ai)
        {
        }
    }
}
