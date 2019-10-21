using System;
using System.Collections.Generic;
using System.Linq;

namespace KCore.DB.Scripts
{
    public class MSQLUpdate : IUpdate
    {
        public string Model<T>(T model) where T : KCore.Base.BaseTable_v1
        {
            if (model.Fields == null)
                throw new NotImplementedException("This object is not loaded");

            var res = KCore.DB.Factory.Result.Top(1, model.TableInfo.DBase, (new MSQLSelect()).ByVPKey(model));

            if (res == null)
                return null;

            var columns = new List<String>();
            var data = new List<dynamic>();


            foreach (var p in KCore.Reflection.FilterOnlyGetProperties(model))
            {

                var value = p.GetValue(model) ?? String.Empty;
                


                if (res.HasColumn(p.Name)
                   // && !(model.TableInfo.PKey.Where(t => t == p.Name).Any())
                    && value.ToString().ToUpper() != res[p.Name, 0].ToString().ToUpper())
                {
                    if (String.IsNullOrEmpty(value.ToString()) && KCore.DB.Factory.Properties.Column.Required(model, p.Name))
                        value = null;

                    data.Add(value);
                    columns.Add("[" + p.Name + "] = {" + columns.Count + "}");
                }
            }

            var sql = $@"
UPDATE   [{model.TableInfo.Name}] 
SET      {String.Join(",", columns.ToArray())}
WHERE    ";

            var where = new string[model.TableInfo.PKey.Count()];

            for (int i = 0; i < model.TableInfo.PKey.Count(); i++)
            {
                var foo = model.TableInfo.PKey[i];
                where[i] += $" [{foo}] = '{model.Fields[foo]}' ";
            }

            sql += String.Join(" AND ", where);

            MSQLFix.Format(ref sql, data.ToArray());


            if (columns.Count < 1 || where.Length < 1)
                return null;

            return sql;
        }
    }
}
