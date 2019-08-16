using KCore;
using KCore.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace KCore.DB.Clients
{
    public sealed partial class MSQLClient : KCore.Base.BaseClass, KCore.Base.IBaseClient
    {

        private static DataInfo DefDataInfo;
        private SqlConnection Conn;
        private SqlDataReader DataReader;
        private SqlCommand Command;
        private int Cursor;
        public string LastCommand { get; private set; }
        public bool IsFirstLine { get; private set; }
        public int FieldCount { get; internal set; }
        public DataInfo DataInfo { get; private set; }

        private bool hasLine;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="autoconn">Connect using the global connection string?</param>
        public MSQLClient(bool autoconn = false)
        {
            if (autoconn)
                Connect();
        }

        #region Connection
        /// <summary>
        /// Connect especific database.
        /// The connection string must be set.
        /// </summary>
        /// <param name="dbase">Database</param>
        public void Connect(string dbase)
        {
            var dataInfo = DefDataInfo.Clone();
            dataInfo.Schema = dbase;
            Connect(dbase);
        }

        /// <summary>
        /// Connect using the string connection default
        /// </summary>
        public void Connect()
        {
            if (DefDataInfo == null)
                throw new KDBException(LOG, C.MessageEx.FatalError1_1, "The default connection string is not define");
            else
                Connect(DefDataInfo);
        }

        /// <summary>
        /// Connect the Microsof SQLServer
        /// </summary>
        /// <param name="cred">Credential to connect</param>
        /// <param name="def">Is connection default?</param>
        /// <param name="dbase">Which database?</param>
        public void Connect(Credential2 cred)
        {

            var dataInfo = new DataInfo(
                cred.Host,
                null,
                cred.User,
                cred.GetPasswd(),
                cred.GetProperty("Port"),
                cred.GetProperty("Driver"),
                KCore.C.Database.ServerType.Hana);

            Connect(dataInfo);

        }


        /// <summary>
        /// Create a connect
        /// </summary>
        /// <param name="dataInfo">Connection data</param>
        public void Connect(DataInfo dataInfo)
        {
            if (dataInfo.Default || DefDataInfo == null)
                DefDataInfo = dataInfo;

            try
            {
                if (Conn != null)
                {
                    Dispose();
                    Conn.Open();
                }
                else
                {
                    Conn = new SqlConnection(dataInfo.ToConnString());
                    Conn.Open();
                }

                DataInfo = dataInfo;
                return;
            }
            catch (SqlException ex)
            {
                var id = KCore.Diagnostic.Track(LOG, ex);
                KCore.Diagnostic.Error(R.ID, LOG, id, ex.Message);

                throw new KDBException(LOG, C.MessageEx.ErrorDabaseConnection2_2, $"{dataInfo.ToConnString(true)}", ex.Message);
            }
        }

        #endregion

        public void Dispose()
        {
            Clear();

            if (Conn != null)
                Conn.Close();
        }
        private void Clear()
        {
            if (DataReader != null && !DataReader.IsClosed)
                DataReader.Close();

            if (Command != null)
                Command.Dispose();

            IsFirstLine = false;
            FieldCount = 0;
            Cursor = 0;
        }

        #region Execute
        public bool DoQuery(string sql, params dynamic[] values)
        {
            Clear();

            Factory.Scripts.Prepare(false, ref sql, values);
            this.LastCommand = sql;
            this.Command = new SqlCommand(sql, Conn);
            this.DataReader = Command.ExecuteReader();

            IsFirstLine = DataReader.HasRows;

            if (DataReader.HasRows)
            {
                DataReader.Read();
                FieldCount = DataReader.FieldCount;
            }

            hasLine =  DataReader.HasRows;

            return hasLine;

        }
        public bool NoQuery(string sql, params dynamic[] values)
        {
            if (DataReader != null && !DataReader.IsClosed)
                DataReader.Close();

            LastCommand = sql;

            var command = new SqlCommand(sql, Conn);
            return command.ExecuteNonQuery() > 0;
        }
        public bool Procedure(string name, params object[] values)
        {
            throw new NotImplementedException();
        }
        public bool Next(int limit = -1)
        {
            if (!hasLine)
                return false;

            Cursor++;
            IsFirstLine = Cursor == 1;

            if (Cursor > limit && limit > 0)
            {
                var id = KCore.Diagnostic.Track(LOG, LastCommand);
                KCore.Diagnostic.Warning(R.ID, LOG, id, $"The Query was limited to {limit} lines");

                DataReader.Close();
                return false;
            }

            if (DataReader.IsClosed)
                return false;

            if (Cursor == 1)
                return true;

            if (DataReader.Read())
                return true;
            else
            {
                DataReader.Close();
                return false;
            }
        }
        #endregion

        #region Result

        [Obsolete("Temporary")]
        public Dictionary<string, Dynamic> Fields(bool upper = true)
        {
            var res = new Dictionary<string, Dynamic>();

            for (int c = 0; c < FieldCount; c++)
            {
                var col = upper ? DataReader.GetName(c).ToUpper() : DataReader.GetName(c);
                res.Add(col, new Dynamic(DataReader.GetValue(c), DataReader.GetName(c)));
            }

            return res;
        }
        public Dynamic Field(object index)
        {
            if (index.GetType().Name.ToLower().StartsWith("int"))
                return new Dynamic(DataReader.GetValue((int)index), DataReader.GetName((int)index));
            else
                return new Dynamic(DataReader[index.ToString()], index.ToString());
        }

        public Dynamic[] Line1()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, Dynamic> Line2()
        {
            throw new NotImplementedException();
        }


        public T[] Column<T>(object index)
        {
            var t = new List<T>();

            while (Next())
                t.Add(Field(index).Value);

            return t.ToArray();
        }
        #endregion

        #region Properties
        public int Version()
        {
            DoQuery("SELECT CAST(SUBSTRING(@@VERSION, CHARINDEX('-', @@VERSION,0) + 2, CHARINDEX('.', @@VERSION,0) - CHARINDEX('-', @@VERSION,0) - 2) as int)");
            return Field(0).ToInt();
        }

        public bool HasDatabase(string name)
        {
            var sql = $"SELECT name FROM master.dbo.sysdatabases WHERE name = '{name}'";

            return DoQuery(sql);
        }

        public bool HasTable(string database, string table)
        {
            var sql = "SELECT top 1 1 FROM " + database + ".INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG = {0} AND TABLE_NAME = {1}";

            return DoQuery(sql, database, table);
        }

        public bool HasColumn(string database, string table, string column)
        {
            var sql = "SELECT TOP 1 1 FROM " + database + @".INFORMATION_SCHEMA.COLUMNS
WHERE  TABLE_CATALOG = {0}
    AND TABLE_NAME = {1}
    AND COLUMN_NAME = {2}";

            return DoQuery(sql, database, table, column);
        }

        public ColumnStruct[] Columns(string dsource, string table)
        {
            var list = new List<ColumnStruct>();
            DoQuery(Content.queries_general.LOCAL_COLUMNS_4_DBASE_TABLE, dsource, table);
            while (Next())
            {
                list.Add(new ColumnStruct(
                    dsource,
                    table,
                    Field("Name").ToString(),
                    MSQLClient.GetColumnType(Field("Type").ToString()),
                    Field("Request").ToBool(),
                    null,
                    Field("Size").ToInt(),
                    Field("PK").ToBool()));;
            }

            return list.ToArray();
        }

        #endregion
    }

    public sealed partial class MSQLClient
    {
        public static string LOG => typeof(MSQLClient).Name;
        public static KCore.C.Database.ColumnType GetColumnType(string type)
        {
            switch (type)
            {
                case "nvarchar": return KCore.C.Database.ColumnType.Text;
                case "datetime": return KCore.C.Database.ColumnType.DateTime;
                case "int": return KCore.C.Database.ColumnType.Number;
                case "char": return KCore.C.Database.ColumnType.Char;
                default: throw new KDBException(LOG, C.MessageEx.FatalError1_1, type);
            }
        }

        public static string GetColumnType(KCore.C.Database.ColumnType type)
        {
            switch (type)
            {
                case KCore.C.Database.ColumnType.Text: return "nvarchar";
                case KCore.C.Database.ColumnType.DateTime: return "datetime";
                case KCore.C.Database.ColumnType.Number: return "int";
                case KCore.C.Database.ColumnType.Char: return "char";
                default: throw new KDBException(LOG, C.MessageEx.FatalError1_1, type);
            }
        }
    }
}
