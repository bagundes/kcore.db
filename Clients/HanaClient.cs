using K.Core;
using K.Core.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Odbc;

namespace K.DB.Clients
{
    public sealed partial class HanaClient : K.Core.Base.BaseClass, K.Core.Base.IBaseClient
    {

        private static DataInfo DefDataInfo;
        //private static OdbcConnectionStringBuilder ConnectionString;
        private OdbcConnection Conn;
        private OdbcDataReader DataReader;
        private OdbcCommand Command;
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
        public HanaClient(bool autoconn = false)
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
            Connect(dataInfo);
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
                cred.GetProperty("Port").ToInt(),
                cred.GetProperty("Driver").ToString(),
                Core.C.Database.ServerType.Hana);

            dataInfo.Default = cred.GetProperty("Default").ToBool();

            //var ConnectionString = new OdbcConnectionStringBuilder(dataInfo.ToConnString());

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
                    Conn = new  OdbcConnection(dataInfo.ToConnString());
                    Conn.Open();
                }

                DataInfo = dataInfo;

                return;
            }
            catch (SqlException ex)
            {
                var id = K.Core.Diagnostic.Track(LOG, ex);
                K.Core.Diagnostic.Error(R.ID, LOG, id, ex.Message);

                throw new KDBException(LOG, C.MessageEx.ErrorDabaseConnection2_2, $"{DataInfo.URL()}", ex.Message);
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
            this.Command = new OdbcCommand(sql, Conn);
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

            var command = new OdbcCommand(sql, Conn);
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
                var id = K.Core.Diagnostic.Track(LOG, LastCommand);
                K.Core.Diagnostic.Warning(R.ID, LOG, id, $"The Query was limited to {limit} lines");

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
                    HanaClient.GetColumnType(Field("Type").ToString()),
                    Field("Request").ToBool(),
                    null,
                    Field("Size").ToInt(),
                    Field("PK").ToBool()));;
            }

            return list.ToArray();
        }

        #endregion
    }

    public sealed partial class HanaClient
    {
        public static string LOG => typeof(HanaClient).Name;
        public static K.Core.C.Database.ColumnType GetColumnType(string type)
        {
            switch (type)
            {
                case "nvarchar": return K.Core.C.Database.ColumnType.Text;
                case "datetime": return K.Core.C.Database.ColumnType.DateTime;
                case "int": return K.Core.C.Database.ColumnType.Number;
                case "char": return Core.C.Database.ColumnType.Char;
                default: throw new KDBException(LOG, C.MessageEx.FatalError1_1, type);
            }
        }

        public static string GetColumnType(K.Core.C.Database.ColumnType type)
        {
            switch (type)
            {
                case K.Core.C.Database.ColumnType.Text: return "nvarchar";
                case K.Core.C.Database.ColumnType.DateTime: return "datetime";
                case K.Core.C.Database.ColumnType.Number: return "int";
                case K.Core.C.Database.ColumnType.Char: return "char";
                default: throw new KDBException(LOG, C.MessageEx.FatalError1_1, type);
            }
        }
    }
}
