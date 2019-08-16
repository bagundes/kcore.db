using KCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace KCore.DB.Scripts
{
    public static class MSQLFix
    {
        public static void Top(int limit, ref string sql)
        {
            if (!sql.Contains(" TOP "))
            {
                var index = sql.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);
                sql = sql.Insert(index + "SELECT".Length, $" TOP {limit} ");
            }
        }

        public static void Format(ref string sql, dynamic[] values, bool manipulation = false)
        {
            if (values.Length < 1)
                return;

            var descr = manipulation ? "''''" : "''";
            var open = "N" + (manipulation ? "''" : "'");
            var close = manipulation ? "''" : "'";
            var whereIndex = sql.IndexOf("WHERE", StringComparison.InvariantCultureIgnoreCase);

            if (sql.Contains("UPDATE"))
                whereIndex = sql.IndexOf("SET", StringComparison.InvariantCultureIgnoreCase); ;

            for (int i = 0; i < values.Length; i++)
            {
                var pos = sql.IndexOf("{" + i + "}");
                if (pos < whereIndex)
                    continue;

                if (values[i] == null)
                {
                    values[i] = "null";
                    continue;
                }

                Dynamic dyn = values[i];


                switch (dyn.ForceType())
                {
                    case TypeCode.DateTime:
                        // Date initial: 01/01/1901
                        if (dyn.ToDateTime() == (new DateTime()))
                        {
                            values[i] = "null";
                            continue;
                        }
                        else
                            values[i] = $"{open}{(dyn.ToDateTime()).ToString("yyyy-MM-ddTHH:mm:ss")}{close}";
                        continue;
                }

                values[i] = $"{open}{(dyn.ToString()).Replace("'", descr)}{close}";
            }

            sql = String.Format(sql, values);
        }
    }
}
