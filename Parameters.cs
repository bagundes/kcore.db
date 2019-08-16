using System;
using System.Collections.Generic;
using System.Text;

namespace K.DB
{
    public static class Parameters
    {

        public static K.Core.Dynamic Get(int code, string dbase, string name, string var1, string var2, string var3, dynamic @default)
        {

            var hvar1 = String.IsNullOrEmpty(var1) ? "--" : "";
            var hvar2 = String.IsNullOrEmpty(var2) ? "--" : "";
            var hvar3 = String.IsNullOrEmpty(var3) ? "--" : "";

            var sql = Content.queries_general.PARAM_GETVALUE_6_DBase_Name_Var123_BplId;


            sql = String.Format(sql,
                Factory.DataInfo.Schema,
                code,
                name,
                var1,
                var2,
                var3,
                hvar1,
                hvar2,
                hvar3);

            var res = Factory.Result2.Get(dbase, sql);
            if (res != null)
                return res;
            else
                return Core.Dynamic.From(@default);
        }

        public static K.Core.Dynamic Get(int code, string dbase, string name, string var1, dynamic @default)
        {
            return Get(code, dbase, name, var1, null, null, @default);
        }

        public static K.Core.Dynamic Get(int code, string dbase, string name, dynamic @default)
        {
            return Get(code, dbase, name, null, null, null, @default);
        }

        public static void AddProject(int code, string nspace, string name)
        {
            throw new NotImplementedException();
            /**
             * INSERT INTO [{0}]..[@TS_PARAM0] (
	 Code
	,Name
	,U_Project
	,DocEntry	
	,[Object]
	,UserSign
	,CreateDate
	,CreateTime
	,UpdateDate
	,UpdateTime)
VALUES (
	 {1}
	,'{2}'
	,'{3}'
	,(SELECT ISNULL(MAX(DocEntry),0) + 1 FROM [{0}]..[@TS_PARAM0])
	,'PARAM'
	,1
	,cast(getdate() as date)
	,1300
	,cast(getdate() as date)
	,1300)
             */
        }
    }
}
