using KCore.DB.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCore.DB.Scripts
{
    public class MSQLUpdate : IUpdate
    {
        public string Model<T>(T model) where T : KCore.Base.BaseTable
        {
            var res = KCore.DB.Factory.Result.Top(1, (new MSQLSelect()).ByPKey(model));
            var columns = new List<String>();
            var data = new List<dynamic>();

            data.Add(DateTime.Now);
            columns.Add("[Updated]  = {" + columns.Count + "}");

            foreach (var p in KCore.Reflection.FilterOnlySetProperties(model))
            {

                var value = p.GetValue(model) ?? String.Empty;

                if (!(model.TabInfo.PKey.Where(t => t == p.Name).Any())
                    && value.ToString().Equals(res[p.Name, 0].ToString(), StringComparison.CurrentCultureIgnoreCase))
                {
                    data.Add(value);
                    columns.Add("[" + p.Name + "] = {" + columns.Count + "}");
                }
            }

            var sql = $@"
UPDATE   [{model.TabInfo.DataSource}]..[{model.TabInfo.Table}] 
SET      {String.Join(",", columns.ToArray())}
WHERE    ";





            var where = new string[model.TabInfo.PKey.Count()];

            for (int i = 0; i < model.TabInfo.PKey.Count(); i++)
                where[i] += $" [{ model.TabInfo.PKey[i]}] = '{model.GetPKeyValue(i)}' ";

            sql += String.Join(" AND ", where);

            MSQLFix.Format(ref sql, data.ToArray());

            return sql;
        }
    }
}
