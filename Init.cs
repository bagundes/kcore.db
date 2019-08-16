using System;

namespace K.DB
{
    public class Init : K.Core.Base.IBaseInit
    {

        public bool Configure()
        {
            return true; // throw new NotImplementedException();
        }

        public bool Construct()
        {
            var database = K.Core.R.DataSource;
            // Login control
            var attributes = "Attributes";
            var create = Factory.Scripts.Create(database, attributes, true);
            create.AddColumnRequere("Table", K.Core.C.Database.ColumnType.Text);
            create.AddColumnRequere("Column", K.Core.C.Database.ColumnType.Text);
            create.AddColumnRequere("Source", K.Core.C.Database.ColumnType.Text);
            create.AddColumnRequere("Description", K.Core.C.Database.ColumnType.Text);
            create.AddColumnRequere("TypeID", K.Core.C.Database.ColumnType.Char);
            create.AddColumnNoRequere("FormatString", K.Core.C.Database.ColumnType.Text);
            create.AddColumnNoRequere("EN_IE", K.Core.C.Database.ColumnType.Text);
            //create.ConstraintPK("Table", "Column" ,"Source");

            create.Create();

            return true;
        }

        public bool Destruct()
        {
            return true;// throw new NotImplementedException();
        }

        public bool Populate()
        {
            return true; // throw new NotImplementedException();
        }

        public bool Register()
        {
            return true; // throw new NotImplementedException();
        }
    }
}
