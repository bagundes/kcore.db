using System;

namespace KCore.DB
{
    public static class Parameters
    {

        public static KCore.Dynamic Get(int code, string dbase, string name, string var1, string var2, string var3, dynamic @default)
        {

            var hvar1 = String.IsNullOrEmpty(var1) ? "--" : "!-";
            var hvar2 = String.IsNullOrEmpty(var2) ? "--" : "!-";
            var hvar3 = String.IsNullOrEmpty(var3) ? "--" : "!-";

            var sql = Content.queries_general.TSPARAM0001_DBase_Name_Var123_BplId;

            var res = KCore.DB.Factory.Result.Get(dbase,
                sql,
                code,
                name,
                var1,
                var2,
                var3,
                hvar1,
                hvar2,
                hvar3);
            if (!res.IsEmpty())
                return res;
            else
                return KCore.Dynamic.From(@default);
        }

        public static KCore.Dynamic Get(int code, string dbase, string name, string var1, dynamic @default)
        {
            return Get(code, dbase, name, var1, null, null, @default);
        }

        public static KCore.Dynamic Get(int code, string dbase, string name, dynamic @default)
        {
            return Get(code, dbase, name, null, null, null, @default);
        }

        public static void Set(Model.Parameters param)
        {
            //var foo = param;
            //KCore.DB.Factory.Models.Set(ref foo);

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
