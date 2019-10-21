using KCore.Base;
using KCore.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace KCore.DB.Scripts
{
    public class MSQLCreate : KCore.Base.BaseClass, ICreate
    {
        private string database;
        private string table;
        private List<ColumnStruct> columns = new List<ColumnStruct>();
        private List<string> pkeys = new List<string>();
        public MSQLCreate(string database, string table, bool pkString)
        {
            this.database = database;
            this.table = table;

            var col = pkString ? "Code" : "ID";
            var colType = pkString ? KCore.C.Database.ColumnType.Text : KCore.C.Database.ColumnType.Number;
            AddColumnRequere(col, colType, null, 500);
            AddColumnRequere("ObjClass", KCore.C.Database.ColumnType.Text, "none", 100);
            ConstraintPK(col);

        }

        //public void AddColumn(string colName, KCore.C.Database.ColumnType colType, bool request, int? size = null)
        //{
        //    columns.Add(new ColumnStruct(database, table, colName, colType, request, null, size));
        //}

        public void AddColumnNoRequere(string colName, KCore.C.Database.ColumnType colType, dynamic def = null, int? size = null)
        {
            columns.Add(new ColumnStruct(database, table, colName, colType, false, def, size));
        }

        public void AddColumnRequere(string colName, KCore.C.Database.ColumnType colType, dynamic def = null, int? size = null)
        {
            columns.Add(new ColumnStruct(database, table, colName, colType, true, def, size));
        }

        public void ConstraintPK(params string[] columns)
        {
            pkeys.AddRange(columns);
        }

        public void Create()
        {
            if (pkeys.Count < 1)
                ConstraintPK(columns[0].Name);

            AddColumnRequere("Created", KCore.C.Database.ColumnType.DateTime);
            AddColumnRequere("Updated", KCore.C.Database.ColumnType.DateTime);

            using (var client = (IBaseClient)Activator.CreateInstance(Factory_v1.__client, new object[] { true }))
            {
                try
                {
                    if (!client.HasDatabase(database))
                    {
                        var sql = $"CREATE DATABASE {database}";
                        client.NoQuery(sql);
                    }

                    if (!client.HasTable(database, table))
                    {
                        var col = new string[columns.Count];

                        for (int i = 0; i < columns.Count; i++)
                            col[i] = ColumnCommand(columns[i]);

                        var sql = $"CREATE TABLE {database}..{table.ToString()}({String.Join(",", col)})";
                        client.NoQuery(sql);

                        var sql1 = $"ALTER TABLE {database}..{table} ADD CONSTRAINT PK_{table.ToUpper()}_{DateTime.Now.ToString("yyMMdd")} PRIMARY KEY CLUSTERED ([{String.Join("],[", pkeys)}])";
                        client.NoQuery(sql1);

                    }
                    else
                    {
                        for (int i = 1; i < columns.Count(); i++)
                        {
                            if (!DB.Factory.Properties.Column.NoCache.Exists(database, table, columns[i].Name))
                            {
                                var sql = ColumnCommand(columns[i]);
                                client.NoQuery($"ALTER TABLE {database}..{table} ADD {sql}");
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    var id = Diagnostic.Track(LOG, client.LastCommand, ex.StackTrace);
                    Diagnostic.Error(R.ID, LOG, id, ex.Message);
                    throw new KDBException(LOG, C.MessageEx.ErrorExecuteQuery4_1, id);
                }
            }
        }

        private string ColumnCommand(ColumnStruct colStrunct)
        {
            var required = colStrunct.Required;

            if (pkeys.Where(t => t.Equals(colStrunct.Name, StringComparison.InvariantCultureIgnoreCase)).Any())
                required = true;

            var command = String.Empty;

            if (colStrunct.ColType == KCore.C.Database.ColumnType.Text)
                command = $@" [{colStrunct.Name}] {Clients.MSQLClient.GetColumnType(colStrunct.ColType)}({colStrunct.Size}) {(required ? "not" : "")} null";
            else
                command = $@" [{colStrunct.Name}] {Clients.MSQLClient.GetColumnType(colStrunct.ColType)} {(required ? "not" : "")} null";

            if (colStrunct.Default != null)
                command += $" DEFAULT('{colStrunct.Default}')";

            return command;
        }
    }
}
