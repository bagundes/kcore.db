using System;
using System.Collections.Generic;
using System.Linq;

namespace KCore.DB.Model
{
    public class ResultSet
    {
        public string Title { get; set; }
        public bool Search { get; set; } = false;

        public List<Model.LinkTo> Options { get; set; } = new List<LinkTo>();

        public List<Column> Columns = new List<Column>();
        /// <summary>
        /// Data: [Line][Column:Data]
        /// </summary>
        public List<Line> Data = new List<Line>();
        public bool HasSpecialColumn { get; protected set; } = false;
        public string SpecialColumnTag { get; protected set; }


        /// <summary>
        /// Result["Column"][line]
        /// </summary>
        private Dictionary<string, List<KCore.Dynamic>> Array = new Dictionary<string, List<KCore.Dynamic>>();

        public dynamic this[string column, int line]
        {
            get
            {
                return Array[column][line];
            }
        }



        /// <summary>
        /// Add or update a column in the Data
        /// </summary>
        /// <param name="column">Column name</param>
        /// <param name="value">Value</param>
        /// <param name="line">Line position (-1: addline)</param>
        public void AddData(string column, dynamic value, int line = -1)
        {
            if (Factory.MyTags.HasSpecialTag(ref column))
            {
                HasSpecialColumn = true;
                value = Factory.MyTags.LinkTo(value);
            }

            if (line < 0)
            {
                line = Data.Count - 1;

                if (line < 0 || Data[line].Columns.Count >= Columns.Count)
                    Data.Add(new Line(column, value));
                else
                    Data[line].AddColumn(column, value);
            }
            else
            {
                Data[line].Columns[GetColumnIndex(column)] = new Column(column, value, null);
            }

            if (Array.ContainsKey(column))
            {
                var lastLine = Array[column].Count;
                if (lastLine > line)
                {
                    if (value.GetType().Equals(typeof(KCore.Dynamic)))
                        Array[column][line] = value;
                    else
                        Array[column][line] = $"Object:{value.GetType()}";
                }

                else
                {
                    if (value.GetType().Equals(typeof(KCore.Dynamic)))
                        Array[column].Add(value);
                    else
                        Array[column].Add($"Object:{value.GetType()}");
                }

            }
            else
            {
                var foo = new List<KCore.Dynamic>();

                foo.Add(new KCore.Dynamic(value));
                Array.Add(column, foo);
            }

        }

        public KCore.Model.Select_v1[] ToSelect(bool encrypt = false)
        {
            var select = new List<KCore.Model.Select_v1>();



            foreach (var line in Data)
            {
                var value = line.Columns[0].Value;
                var text = line.Columns.Count > 1 ? line.Columns[1].Value : value;

                select.Add(new KCore.Model.Select_v1(value, text, encrypt));
            }

            return select.ToArray();
        }

        /// <summary>
        /// Add or update the column
        /// </summary>
        /// <param name="column">Column name</param>
        /// <param name="value">Column value</param>
        public void AddColumn(string column, string value = null)
        {
            String columName;
            String tag;


            var index = GetColumnIndex(column);
            var specialTag = Factory.MyTags.HasSpecialTag(column, out columName, out tag);

            if (specialTag)
            {
                HasSpecialColumn = true;
                SpecialColumnTag = tag;
            }

            column = column.ToLower();
            value = value ?? columName;

            if (index >= 0)
                Columns[index].Value = value;
            else
                Columns.Add(new Column(column, value, null));
        }

        public int GetColumnIndex(string column)
        {
            if (Columns.Count > 0)
            {
                var foo = Columns.Select((item, index) => (
                    Name: item.Name,
                    Position: index
                )).Where(i => i.Name == column)
                    .FirstOrDefault()
                    .Position;

                return Columns[foo].Name.Equals(column) ? foo : -1;
            }
            else
                return -1;
        }

        public object Response()
        {
            string tag = String.IsNullOrEmpty(SpecialColumnTag) ? String.Empty : SpecialColumnTag.ToLower();
            var properties = new { specialtag = tag, title = Title, options = Options };


            var cols = new Dictionary<string, string>();
            foreach (var col in Columns)
            {
                var name = col.Name.ToLower();
                var value = ((string)col.Value).StartsWith(tag, StringComparison.OrdinalIgnoreCase) ? ((string)col.Value).Substring(tag.Length) : ((string)col.Value);
                cols.Add(name, value);
            }


            var lines = new List<Dictionary<string, dynamic>>();

            for (int l = 0; l < Data.Count; l++)
            {
                var line = new Dictionary<string, object>();
                foreach (var col in Data[l].Columns)
                    line.Add(col.Name.ToLower(), col.Value);


                lines.Add(line);
            }

            return new { columns = cols, values = lines, properties };
        }
    }

    public class Column
    {
        public string Name;
        public dynamic Value;
        public string Type;

        public Column(string column, KCore.Dynamic value, string type)
        {
            Name = column;
            Value = value.Value;
            Type = type;
        }

        public Column(string column, dynamic value, string type)
        {
            Name = column;
            Value = value;
            Type = type;
        }

    }

    public class Line
    {
        public List<Column> Columns = new List<Column>();

        public void AddColumn(string column, dynamic value)
        {
            Columns.Add(new Column(column, value, null));
        }

        public Line(List<Column> columns)
        {
            Columns = columns;
        }

        public Line(Column column)
        {
            Columns.Add(column);
        }

        public Line(string column, Object value)
        {
            Columns.Add(new Column(column, value, null));
        }
    }
}

