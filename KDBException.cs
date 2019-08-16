using System;
using System.Collections.Generic;
using System.Resources;
using System.Runtime.Serialization;
using System.Text;

namespace KCore.DB
{
    public class KDBException : KCore.Base.BaseException
    {
        protected override ResourceManager Resx => R.Resx;
        protected override int Id => R.ID;
        public KDBException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public KDBException(string log, C.MessageEx code, params dynamic[] values) : base(log, code, values)
        {
        }

        public KDBException(string log, C.MessageEx code, Exception innerException) : base(log, code, innerException)
        {
        }
    }
}
