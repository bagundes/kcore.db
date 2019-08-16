using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace KCore.DB.Model
{
    /// <summary>
    /// Resultset with diplay attributes
    /// </summary>
    public sealed partial class Resultset2
    {
        public List<Data> Columns { get; private set; } = new List<Data>();
        public List<Line> Lines { get; private set; } = new List<Line>();


        /// <summary>
        /// Information about the result and header
        /// </summary>
        public Properties Property { get; private set; } = new Properties();

        private string source;
        private int poscol;

        public Resultset2(string source = null)
        {
            this.source = source ?? "sap";
            this.Property.SpecialTag = C.Tags.SPECIAL_TAG.ToLower();
        }



        public void AddColumn(Data data)
        {
            if (data.value is null)
                data.value = $"Column {Columns.Count()}";

            AddColumn(data.value, data.display);
        }

        /// <summary>
        /// Add column name
        /// </summary>
        /// <param name="column"></param>
        /// <param name="display"></param>
        public void AddColumn(string column, string display = null)
        {
            String columName;
            String tag;

            // Update the column display
            if(display == null)
                display = Stored.Cache.AttributesValues.Where(t => t.code == $"{source}.{column}".ToLower()).Select(t => t.description).FirstOrDefault() ?? column;


            var sptag = Scripts.MyTags.HasSpecialTag(column, out columName, out tag);

            if(sptag)
            {
                display = columName;
                Property.SpecialTag = tag;
            }

            var name = column;
            var index = $"{(sptag ? tag : "col" ) }{(Columns.Count() + 1).ToString("000")}";
            display = display ?? column;

            Columns.Add(new Data(index, name, display, sptag));
        }

        public void AddData(Data data)
        {
            AddData(data.index, data.value, data.display);
        }
        /// <summary>
        /// Add data values
        /// </summary>
        /// <param name="column"></param>
        /// <param name="data"></param>
        /// <param name="display"></param>
        public void AddData(string column, dynamic data, string display = null)
        {
            poscol++;
            
            var foo = Stored.Cache.AttributesValues.Where(t => t.code == $"{source}.{column}".ToLower()).FirstOrDefault();


            // Is it a special field?
            if(Property.HasSpecialTag && Columns.Where(t => t.value == column && t.specialTag).Any())
            { 
                var linkto = Scripts.MyTags.LinkTo(data);

                if (linkto != null)
                {
                    if (foo == null)
                        display = Format(linkto.Display, KCore.C.Database.TypeID.None, null);
                    else
                        display = Format(linkto.Display, foo.TypeID, foo.formatString);
                }
                if (poscol > Columns.Count || Lines.Count < 1)
                {
                    poscol = 1;
                    var line = new Line();
                    line.Data.Add(new Data($"{Property.SpecialTag}{(poscol).ToString("000")}", linkto, display, true));
                    Lines.Add(line);
                }
                else
                {
                    var p = Lines.Count;
                    Lines[p - 1].Data.Add(new Data($"{Property.SpecialTag}{(poscol).ToString("000")}", linkto, display, true));
                }
            }
            else
            {
                if (display == null)
                {
                    if (foo == null)
                        display = Format(data, KCore.C.Database.TypeID.None, null);
                    else
                        display = Format(data, foo.TypeID, foo.formatString);
                }

                if (poscol > Columns.Count || Lines.Count < 1)
                {
                    poscol = 1;
                    var line = new Line();
                    line.Data.Add(new Data($"col{(poscol).ToString("000")}", data, display, false));
                    Lines.Add(line);
                } else
                {
                    var p = Lines.Count;
                    Lines[p - 1].Data.Add(new Data($"col{(poscol).ToString("000")}", data, display, false));
                }
            }
        }

        /// <summary>
        /// Format display info
        /// </summary>
        /// <param name="value"></param>
        /// <param name="typeId"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private string Format(dynamic value, KCore.C.Database.TypeID typeId, string formatString = null)
        {

            switch (typeId)
            {
                case KCore.C.Database.TypeID.Number:
                    return KCore.Dynamic.From(value).ToNumberString(formatString);
                case KCore.C.Database.TypeID.Alpha:
                    return KCore.Dynamic.From(value).ToString();
                case KCore.C.Database.TypeID.Price:
                    return KCore.Dynamic.From(value).ToPriceString(KCore.C.Language.en_IE);
                case KCore.C.Database.TypeID.Date:
                    return KCore.Dynamic.From(value).ToDateString(formatString);
                default:
                    return KCore.Dynamic.From(value).ToString();
            }
        }

        public void SwithRowAndColumns()
        {
            var result2 = new Resultset2();

            var coltotal = Columns.Count;
            var rowtotal = Lines.Count;

            var columns = new Data[rowtotal];
            var lines = new Line[coltotal];



            for (int col = 0; col < coltotal; col++)
            {
                #region Headers
                for (int line = -1; line < rowtotal; line++)
                {
                    if (col == 0 && line == -1) // header
                    {
                        result2.AddColumn(Columns[col]);
                        continue;
                    } else if (col == 0 && line >= 0)
                    {
                        result2.AddColumn(Lines[line].Data[col]);
                        continue;
                    }
                        else if (line == -1)
                    {
                        result2.AddData(Columns[col]);
                    } else
                    {
                        result2.AddData(Lines[line].Data[col]);
                    }
                }
                #endregion
            }


            // Columns
            //for (int x = 0; x < coltotal; x++)
            //{
            //    if (x == 0)
            //        result2.AddColumn(Columns[x]); //result2.AddColumn(Columns[x].value, Columns[x].display);
            //    else
            //        result2.AddData(Columns[x].value, Columns[x].value, Columns[x].display);
            //}

            //// Lines
            //for (int y = 0; y < rowtotal; y++)
            //{
            //    for (int x = 0; x < coltotal; x++)
            //    {
            //        if (x == 0)
            //            result2.AddColumn(Lines[y].Data[x]); //result2.AddColumn(Lines[y].Data[x].value, Lines[y].Data[x].display);
            //        else
            //            result2.AddData(Lines[y].Data[x]); //result2.AddData(Lines[y].Data[x].value, Lines[y].Data[x].value, Lines[y].Data[x].display);
                    
            //    }                
            //}

            Columns = result2.Columns;
            Lines = result2.Lines;

        }
        public object Response()
        {
            string tag = !Property.HasSpecialTag ? String.Empty : Property.SpecialTag;
            var properties = new { specialtag = tag,
                title = Property.Title,
                swtch = new { title = Property.Switch.title, enabled = Property.Switch.transfor },
                search = Property.Search
            };


            var cols = new Dictionary<string, string>();
            var lines = new List<Dictionary<string, dynamic>>();

            if (properties.search)
                cols.Add("search", "search");

            foreach (var col in Columns)
            {
                var name = col.index;
                var value = ((string)col.display).StartsWith(tag, StringComparison.OrdinalIgnoreCase) ? ((string)col.display).Substring(tag.Length) : ((string)col.display);
                cols.Add(name, value);
            }
            
            
            for (int i = 0; i < Lines.Count; i++)
            {
                var line = new Dictionary<string, object>();
                foreach (var col in Lines[i].Data)
                {
                    if (properties.search && !col.specialTag)
                    {
                        if (!line.ContainsKey("search"))
                            line.Add("search", col.display + "|" + col.value);
                        else
                            line["search"] += "|" + col.display + "|" + col.value;
                    }

                    line.Add(col.index, col);
                }

                lines.Add(line);
            }

            return new { columns = cols, values = lines, properties };
        }

        #region classes
        public sealed class Data
        {
            public string index;
            public dynamic value;
            public string display;
            public bool specialTag;

            public Data(string index, dynamic value, string display = null, bool stag = false)
            {
                this.specialTag = stag;
                this.index = index;
                this.value = value;
                this.display = display;
            }
        }

        public sealed class Line
        {
            public List<Data> Data { get; set; } = new List<Data>();

            public void Add(Data data)
            {
                var line = new Line();
                line.Data.Add(data);
            }
        }

        public sealed class Properties
        {
            private string specialTag;
            public bool Search = false;
            public string SpecialTag { get { return specialTag; } set { specialTag = value.ToLower(); } }
            public bool HasSpecialTag => !String.IsNullOrEmpty(SpecialTag);

            public (bool transfor, string title) Switch;
            public string Title { get; set; }
        }
        #endregion

    }

    public sealed partial class Resultset2
    {
        /// <summary>
        /// Save the result in the file
        /// </summary>
        /// <param name="dest"></param>
        /// <returns></returns>
        public void SaveToFile(string dest, string delimited = ";", bool addColumName = false)
        {
            var lines = new List<string>();

            // fix
            switch(delimited)
            {
                case "\\t": delimited = "\t"; break;
                case "\\n": delimited = "\n"; break;
            }


            foreach(var line in Lines)
            {
                var val = String.Empty;
                foreach(var data in line.Data)
                    val += $"{data.value}{delimited}";

                lines.Add(val.Substring(0, val.Length - delimited.Length));
            }

            KCore.Shell.File.Save(lines.ToArray(), dest, true, true);

        }
    }
}
