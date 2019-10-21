namespace KCore.DB
{
    public class Init : KCore.Base.IBaseInit
    {

        public bool Configure()
        {
            return true; // throw new NotImplementedException();
        }

        public bool Construct()
        {
            //var database = KCore.R.DataSource;
            //// Login control
            //var attributes = "Attributes";
            //var create = Factory_v1.Scripts.Create(database, attributes, true);
            //create.AddColumnRequere("Table", KCore.C.Database.ColumnType.Text);
            //create.AddColumnRequere("Column", KCore.C.Database.ColumnType.Text);
            //create.AddColumnRequere("Source", KCore.C.Database.ColumnType.Text);
            //create.AddColumnRequere("Description", KCore.C.Database.ColumnType.Text);
            //create.AddColumnRequere("TypeID", KCore.C.Database.ColumnType.Char);
            //create.AddColumnNoRequere("FormatString", KCore.C.Database.ColumnType.Text);
            //create.AddColumnNoRequere("EN_IE", KCore.C.Database.ColumnType.Text);
            ////create.ConstraintPK("Table", "Column" ,"Source");

            //create.Create();

            return true;
        }

        public bool Dependencies()
        {
            KCore.Config.Init.Execute(new KCore.Init());
            return true;// throw new NotImplementedException();
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
            KCore.R.RegisterProject(R.ID, R.Project.Name);
            return true; // throw new NotImplementedException();
        }
    }
}
