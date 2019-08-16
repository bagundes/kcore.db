using KCore.DB.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCore.DB.Scripts
{
    public class MSQLInsert : IInsert
    {
        public string Model<T>(T table, out dynamic[] pks) where T : KCore.Base.BaseTable
        {
            var pks1 = new List<dynamic>();
            var columns = new List<String>();
            var foo = new List<string>();
            var data = new List<dynamic>();
            var hash = KCore.Security.Hash.MD5(DateTime.Now);

            foreach (var col in DB.Properties.Columns.ColumnsList(table))
            {
                var value = KCore.Reflection.GetValue(table, col.Name);

                // Primary Key
                if (value == null && col.PK && table.TabInfo.AutoIncrement)
                {
                    var ai = KCore.DB.Factory.Result.First($"SELECT MAX({col}) FROM [{table.TabInfo.Table}]");
                    string bar;
                    if (ai.IsEmpty())
                        bar = "1";
                    else if (ai.IsNumber())
                        bar = (ai.ToInt() + 1).ToString();
                    else
                        bar = hash;

                    KCore.Reflection.SetValue(table, col.Name, bar);
                    value = bar;
                    pks1.Add(value);
                }

                if (value != null)
                {
                    data.Add(value);
                    columns.Add($"[{col.Name}]");
                    foo.Add("{" + foo.Count + "}");
                }
            }

            // Created date
            data.Add(DateTime.Now);
            columns.Add($"[Created]");
            foo.Add("{" + (foo.Count) + "}");

            // Add ObjectClass
            var foobar = KCore.Reflection.HasPropertyOrField(table, "ObjClass");
            if(foobar > 0)
            {
                data.Add(KCore.Reflection.GetValue(table, "ObjClass"));
                columns.Add($"[ObjClass]");
                foo.Add("{" + (foo.Count) + "}");
            }




            var sql = $@"
INSERT INTO [{table.TabInfo.DataSource}]..[{table.TabInfo.Table}] 
({String.Join(",", columns.ToArray())}) 
VALUES  ({String.Join(",", foo.ToArray())})";

            Factory.Scripts.Prepare(false, ref sql, data.ToArray());
            pks = pks1.Count > 0 ? pks1.ToArray() : null;
            return sql;
        }
    }
}
