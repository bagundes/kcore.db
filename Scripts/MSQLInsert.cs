using System;
using System.Collections.Generic;

namespace KCore.DB.Scripts
{
    public class MSQLInsert : IInsert
    {
        public string Model<T>(T table, out dynamic[] pks) where T : KCore.Base.BaseTable_v1
        {
            var pks1 = new List<dynamic>();
            var columns = new List<String>();
            var foo = new List<string>();
            var data = new List<dynamic>();
            var hash = KCore.Security.Hash.MD5(DateTime.Now);

            foreach (var col in DB.Factory.Properties.Column.GetList(table))
            {
                var value = KCore.Reflection.GetValue(table, col.Name);

                // Fix - When field is DocEntry
                if(col.Name.ToUpper() == "DOCENTRY" )
                {
                    value = KCore.DB.Factory.Result.Get(table.TableInfo.DBase,$"SELECT MAX({col.Name}) FROM [{table.TableInfo.Name}]").ToInt(0) + 1;
                    KCore.Reflection.SetValue(table, col.Name, (int)value);
                }
                else if (value == null && col.PK /*&& table.TableInfo.AutoIncrement*/)
                {
                    var ai = KCore.DB.Factory_v1.Result.First($"SELECT MAX({col}) FROM [{table.TableInfo.Name}]");
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
            //data.Add(DateTime.Now);
            //columns.Add($"[Created]");
            //foo.Add("{" + (foo.Count) + "}");

            // Add ObjectClass
            //var foobar = KCore.Reflection.HasPropertyOrField(table, "ObjClass");
            //if (foobar > 0)
            //{
            //    data.Add(KCore.Reflection.GetValue(table, "ObjClass"));
            //    columns.Add($"[ObjClass]");
            //    foo.Add("{" + (foo.Count) + "}");
            //}




            var sql = $@"
INSERT INTO [{table.TableInfo.Name}] 
({String.Join(",", columns.ToArray())}) 
VALUES  ({String.Join(",", foo.ToArray())})";

            Factory_v1.Scripts.Prepare(KCore.C.Database.DBaseType.MSQL, false, ref sql, data.ToArray());
            pks = pks1.Count > 0 ? pks1.ToArray() : null;
            return sql;
        }
    }
}
