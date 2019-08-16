using System;
using System.Collections.Generic;
using System.Linq;

namespace K.DB.Scripts
{
    public static class MyTags
    {
        private static string LOG => typeof(MyTags).Name;


        /// <summary>
        /// Replace the tag !! to Namespace resource.
        /// </summary>
        /// <param name="val">value to replace the namesapce</param>
        /// <returns>Marks used to replace</returns>
        public static string Namespace(ref string val)
        {
            var tag = GetHeaderValue(C.Tags.NAMESPACE_NAME, ref val) ?? C.Tags.NAMESPACE_MARKS;

            val = val
                .Replace($"U_{tag}", $"U_{K.Core.R.Company.Namespace}")
                .Replace($"U{tag}", $"U_{K.Core.R.Company.Namespace}")
                .Replace($"[{tag}]", $"U_{K.Core.R.Company.Namespace}")
                .Replace($"@{tag}", $"@{K.Core.R.Company.Namespace}");

            return tag;
        }
        /// <summary>
        /// Tranform internal tag to valid values of the base model.
        /// Example: {%[object_name].[property]}
        /// </summary>
        /// <param name="val"></param>
        /// <param name="dic"></param>
        public static void InternalTag<T>(ref string val, T[] models) where T : K.Core.Base.IBaseModel
        {
            if (models == null || models.Length < 1)
                return;

            var tagOpen = GetHeaderValue(C.Tags.INTERNAL_NAMEOPEN, ref val) ?? C.Tags.INTERNAL_OPEN;
            // var tagSplit = GetHeaderValue(C.Tags.INTERNAL_NAMESPLIT, ref val) == null ? C.Tags.INTERNAL_SPLIT : GetHeaderValue(C.Tags.INTERNAL_NAMESPLIT, ref val)[0];
            var tagClose = GetHeaderValue(C.Tags.INTERNAL_NAMECLOSE, ref val) ?? C.Tags.INTERNAL_CLOSE;

            foreach (var model in models)
                foreach (var value in model.ToSelect())
                    val = val.Replace($"{tagOpen}{value.Text}{tagClose}", value.Value == null ? "" : value.Value.ToString());


        }

        #region Query Manager Parameters

        /// <summary>
        /// Replace the input tag SAP ([%0]) to values
        /// </summary>
        /// <param name="sql">SQL to change</param>
        /// <param name="values">Values to input</param>
        /// <returns>
        /// Case the result is 0, then the quantity of sap parameters is the same of values.
        /// Case negative, it 's because has more values then parameters;
        /// or positive, it has more parameters then values.
        /// </returns>
        public static int SAPParams(ref string sql, params dynamic[] values)
        {
            
            var regex = new System.Text.RegularExpressions.Regex($@"\{C.Tags.SAP_OPEN}[0-9]\{C.Tags.SAP_CLOSE}");
            var count = regex.Matches(sql).Count;

            for (int i = 0; i < values.Length && i < count; i++) 
            {
                var a = values[i];
                sql = sql.Replace($"{C.Tags.SAP_OPEN}{i}{C.Tags.SAP_CLOSE}", a.ToString());
            }

            return count - values.Length;

        }

        [Obsolete("ReplaceSAPParams", true)]
        public static void SetQMParams(ref string sql, params dynamic[] values)
        {
            for (int i = 0; i < values.Length; i++)
                sql = sql.Replace($"{C.Tags.SAP_OPEN}{i}{C.Tags.SAP_CLOSE}", values[i]);
        }

        public static void SetQMParams(ref string sql, params K.Core.Model.Select[] values)
        {
            for (int i = 0; i < values.Length; i++)
                sql = sql.Replace($"{C.Tags.SAP_OPEN}{i}{C.Tags.SAP_CLOSE}", values[i].Value);
        }
        /// <summary>
        /// Return the SAP parameters in query manager.
        /// Example: [0%|SELECT TOP 10 DocEntry, CardCode FROM OINV]
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static K.Core.Model.Select[] GetQMParams(ref string sql)
        {
            // TODO @blf - I need to transform [%0] to {0}
            // It's not need to read the header becasue the tag is default.
            var tagOpen = C.Tags.SAP_OPEN;
            var tagSplit = C.Tags.SAP_SPLIT;
            var tagClose = C.Tags.SAP_CLOSE;

            var param = new List<K.Core.Model.Select>();
            var indexes = new List<string>(); // temporary solution
            for (int p = 0; p < sql.Length; p++)
            {

                if (sql[p] != tagOpen[0])
                    continue;

                if (!sql.Substring(p, tagOpen.Length).Equals(tagOpen))
                    continue;

                var tagOpenIndex = p;
                var tagCloseIndex = sql.IndexOf(tagClose, tagOpenIndex);
                var tag = sql.Substring(tagOpenIndex + tagOpen.Length, tagCloseIndex - tagOpenIndex - tagOpen.Length);

                if (!tag.Contains(tagSplit.ToString()) && !tag.Contains("select"))
                {
                    param.Add(new K.Core.Model.Select(tag, null));
                    indexes.Add(tag);
                    continue;
                }

                var vals = tag.Split(tagSplit);

                if (param.Where(t => t.Value.Equals(vals[0]) && t.Default == true).Any())
                    continue;

                var index = indexes.IndexOf(vals[0]);

                if (index >= 0)
                {
                    param[index].Text = vals[1];
                    param[index].Default = true;
                }
                else
                {
                    param.Add(new K.Core.Model.Select(vals[0], vals[1], true));
                    indexes.Add(vals[0]);
                }

                p = tagCloseIndex;
            }

            return param.ToArray();
        }
        #endregion

        /// <summary>
        /// Load the LinkTo tag and change and remove the tag in the value.
        /// </summary>
        /// <param name="val">Value with tag</param>
        /// <returns></returns>
        public static Model.LinkTo LinkTo(string val)
        {
            val = val.Trim();

            var tagOpen = GetHeaderValue(C.Tags.SPECIAL_NAMEOPEN, ref val) ?? C.Tags.SPECIAL_OPEN;
            var tagSplit = GetHeaderValue(C.Tags.SPECIAL_NAMESPLIT, ref val) == null ? C.Tags.SPECIAL_SPLIT : GetHeaderValue(C.Tags.SPECIAL_NAMESPLIT, ref val)[0];
            var tagClose = GetHeaderValue(C.Tags.SPECIAL_NAMECLOSE, ref val) ?? C.Tags.SPECIAL_CLOSE;
            var tagParamSplit = GetHeaderValue(C.Tags.SPECIAL_NAMEPARAMSPLIT, ref val) == null ? C.Tags.SPECIAL_PARAMSPLIT : GetHeaderValue(C.Tags.SPECIAL_NAMEPARAMSPLIT, ref val)[0];
            var tagInput = C.Tags.SPECIAL_INPUT;

            if (!val.StartsWith(tagOpen) || !val.EndsWith(tagClose))
                return null;

            // Removing the open and close tag
            val = val.Substring(1, val.Length - 2);


            if (!val.StartsWith(C.Tags.SPECIAL_LINKTO))
                throw new KDBException(LOG, C.MessageEx.LinkToIncorrectFormat3_1, val);


            var values = val.Split(tagSplit);

            var linkto = new Model.LinkTo();

            foreach (var value in values)
            {
                var index = value.IndexOf(':');
                if (index < 1)
                    throw new KDBException(LOG, C.MessageEx.LinkToIncorrectFormat3_1, val);

                var tag = value.Substring(0, index);
                K.Core.Dynamic val1 = value.Substring(index + 1);

                switch (tag)
                {
                    case C.Tags.SPECIAL_LINKTO:
                        linkto.Type = (C.LinkTo.Type)val1.GetEnumByName<C.LinkTo.Type>(); break;
                    case C.Tags.SPECIAL_VALUE:
                        linkto.Value = val1.ToString(); break;
                    case C.Tags.SPECIAL_PARAMETERS:
                        // Has input parameter?
                        var input = val1.ToString().Contains(tagInput);

                        if (input)
                        {
                            var list = new List<K.Core.Model.Select>();

                            foreach (var param in val1.Split(tagParamSplit))
                            {
                                if (param.ToString().StartsWith(tagInput))
                                {
                                    var foo = param.Substring(tagInput.Length, param.Length - tagInput.Length - 1).Split('`');
                                    if (foo.Length > 1)
                                        list.Add(new K.Core.Model.Select(foo[0].ToLower(), foo[1], input));
                                    else
                                        list.Add(new K.Core.Model.Select(foo[0].ToLower(), null, input));
                                }
                                else
                                    list.Add(new K.Core.Model.Select(param));
                            }

                            linkto.Parameters = list.ToArray();
                        }
                        else
                        {
                            linkto.Parameters = K.Core.Model.Select.Split(val1.ToString(), tagParamSplit);
                        }
                        break;
                    case C.Tags.SPECIAL_DISPLAY:
                        linkto.Display = val1.ToString(); break;
                    case C.Tags.SPECIAL_OPENAS:
                        linkto.OpenAs = (C.LinkTo.OpenAs)val1.GetEnumByIndex<C.LinkTo.OpenAs>(); break;
                    default:
                        K.Core.Diagnostic.Warning(R.ID, LOG, 0, $"The tag {tag} not exists");
                        break;
                }
            }

            return linkto;
        }
        /// <summary>
        /// Get parameter on Header
        /// </summary>
        /// <param name="tag">Tag to find</param>
        /// <param name="sql">string with header</param>
        /// <returns></returns>
        public static string GetHeaderValue(string tag, ref string sql)
        {
            var pos = sql.IndexOf(tag);
            if (pos > -1)
            {
                var foo = pos + tag.Length + 1;
                var bar = sql.IndexOf(Environment.NewLine, foo);
                bar = bar < foo ? sql.IndexOf("\r", foo) : bar;
                var foobar = sql.Substring(foo, bar - foo);


                var strBuilder = new System.Text.StringBuilder(sql);
                strBuilder[pos] = '!';
                sql = strBuilder.ToString();

                return foobar.Trim();
            }
            else
                return null;
        }

        public static string GetHeaderTitle(ref string sql)
        {
            return GetHeaderValue(C.Tags.HEADER_TITLE, ref sql);
        }

        public static string GetHeaderSwitch(ref string sql)
        {
            return GetHeaderValue(C.Tags.SWITCH_ROW_COL, ref sql);
        }

        public static bool IsSearch(ref string sql)
        {
            return GetHeaderValue(C.Tags.SEARCH, ref sql) == "1";
        }

        /// <summary>
        /// Verify if the value has special tag
        /// </summary>
        /// <param name="value">Value to verify</param>
        /// <param name="newvalue">Value without tag</param>
        /// <param name="tag">Tag name</param>
        /// <returns></returns>
        public static bool HasSpecialTag(string value, out string newvalue, out string tag)
        {
            value = value.Trim();
            var specialTag = GetHeaderValue(C.Tags.SPECIAL_NAMETAG, ref value) ?? C.Tags.SPECIAL_TAG;
            var ret = value.StartsWith(specialTag, StringComparison.OrdinalIgnoreCase);

            newvalue = ret ? value.Substring(specialTag.Length) : value;
            tag = specialTag.ToLower();

            return ret;
        }
        /// <summary>
        /// Verify if the value has special tag
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool HasSpecialTag(ref string value)
        {
            String newval;
            String tag;
            var res = HasSpecialTag(value, out newval, out tag);

            if (res) value = newval;

            return res;
        }
    }
}
