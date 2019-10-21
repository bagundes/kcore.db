namespace KCore.DB
{
    public static class C
    {
        public static class SQLSyntax
        {
            public const string CommentedLine = "--";
            public const string UncommentedLine = "!-";
            public const string TextValue = "N";
            public const string CommentMSQL = "/*msql*/";
            public const string CommentHana = "/*hana*/";
        }

        internal static class Tags
        {
            #region Tag to change the Namespace company @!!
            public const string NAMESPACE_NAME = "NAMESPACE";
            public const string NAMESPACE_MARKS = "!!";
            #endregion

            #region Tag to replace the [%0] "SAP Tag"
            public const string SAP_NAME = "SAPTAG";
            public const string SAP_OPEN = "[%";
            public const char SAP_SPLIT = '|';
            public const string SAP_CLOSE = "]";
            #endregion

            #region Tag to replace the model values {%model.property%}
            public const string INTERNAL_NAMEOPEN = "INTERNALTAGOPEN";
            //public const string INTERNAL_NAMESPLIT = "INTERNALTAGSPLIT";
            public const string INTERNAL_NAMECLOSE = "INTERNALTAGCLOSE";
            public const string INTERNAL_OPEN = "{%";
            //public const char INTERNAL_SPLIT = '|';
            public const string INTERNAL_CLOSE = "%}";
            #endregion

            #region Parameters in the Header
            public const string HEADER_TITLE = "HEADERTITLE";
            public const string HEADER_OPTIONS = "HEADEROPTIONS";
            public const string HEADER_NAME = "HEADERNAME";
            public const string SWITCH_ROW_COL = "SWITCHROWSANDCOLUMNS";
            public const string SEARCH = "SEARCH";
            #endregion

            #region Tag to special event [LinkTo:QM|...|...]
            public const string SPECIAL_NAMETAG = "SPECIALTAG";
            public const string SPECIAL_NAMEOPEN = "SPECIALOPEN";
            public const string SPECIAL_NAMESPLIT = "SPECIALSPLIT";
            public const string SPECIAL_NAMECLOSE = "SPECIALCLOSE";
            public const string SPECIAL_NAMEPARAMSPLIT = "SPECIALPARAMSPLIT";
            public const string SPECIAL_NAMEOPENAS = "SPECIALOPENAS";
            public const string SPECIAL_INPUT = ">>";

            public const string SPECIAL_TAG = "__TAG";
            public const string SPECIAL_OPEN = "[";
            public const string SPECIAL_CLOSE = "]";
            public const string SPECIAL_LINKTO = "LinkTo";
            public const string SPECIAL_VALUE = "Value";
            public const string SPECIAL_PARAMETERS = "Params";
            public const string SPECIAL_DISPLAY = "Display";
            public const char SPECIAL_SPLIT = '|';
            public const char SPECIAL_PARAMSPLIT = ';';
            public const string SPECIAL_OPENAS = "Open";
            #endregion

        }
        public enum MessageEx
        {
            FatalError1_1 = 1,
            ErrorDabaseConnection2_2 = 2,
            LinkToIncorrectFormat3_1 = 3,
            ErrorExecuteQuery4_1 = 4,
        }

        public class LinkTo
        {
            public enum Type
            {
                QM = 100,
                URL = 101,
                TEXT = 102,
                PRC = 103,
            }

            public enum OpenAs
            {
                Default = 0,
                Modal = 1,
                NewTab = 2,
            }
        }
    }
}
