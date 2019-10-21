using System;
using System.Collections.Generic;

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
            if (values.Length < 1) return;

            #region Definitions
            var descr = manipulation ? "''''" : "''";
            var open = C.SQLSyntax.TextValue + (manipulation ? "''" : "'");
            var close = manipulation ? "''" : "'";
            var whereIndex = sql.IndexOf("WHERE", StringComparison.InvariantCultureIgnoreCase);
            if (sql.Contains("UPDATE")) whereIndex = sql.IndexOf("SET", StringComparison.InvariantCultureIgnoreCase);
            #endregion


            for (int i = 0; i < values.Length; i++)
            {
                var pos = sql.IndexOf("{" + i + "}");
                if (pos < whereIndex) continue;

                if (values[i] == null)
                {
                    values[i] = "null";
                    continue;
                }

                Dynamic dyn;

                if (values[i].GetType().IsArray)
                {
                    if(values[i].GetType() == typeof(KCore.Model.Select_v1))
                    {
                        var foo = new List<string>();
                        foreach(var sel in values[i])
                        {
                            var bar = (KCore.Model.Select_v1)sel;
                            foo.Add(bar.Value);
                        }

                        dyn = $"{String.Join("','", foo.ToArray())}";
                    }
                    else
                        dyn = $"{String.Join("','", values[i])}";
                }
                else
                {
                    dyn = values[i];
                }



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


                if (dyn.ToString() == C.SQLSyntax.CommentedLine)
                    values[i] = $"{dyn}";
                else if (dyn.ToString() == C.SQLSyntax.UncommentedLine)
                    values[i] = $"";
                else if (values[i].GetType().IsArray)
                    values[i] = $"'{dyn}'";
                else
                    values[i] = $"{open}{(dyn.ToString()).Replace("'", descr)}{close}";
            }

            sql = String.Format(sql, values);
        }
    }
}
