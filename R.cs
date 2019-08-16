using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;

namespace K.DB
{
    public static class R
    {
        public static string ParamDB = "teamsoft";

        private static System.Reflection.Assembly Assembly => System.Reflection.Assembly.GetExecutingAssembly();
        private static ResourceManager resx;

        public static string Language = K.Core.R.Language;
        public static int ID => Project.ID;
        public static string[] Resources => Assembly.GetManifestResourceNames();
        public static ResourceManager Resx
        {
            get
            {
                if (resx == null)
                    resx = new ResourceManager($"{typeof(R).Namespace}.Content.Location_{R.Project.Language}", Assembly);

                return resx;
            }
        }
        public static class Project
        {
            public static string Language => R.Language;
            public static string Name => "Kv2";
            public static string Namespace => "KV";
            public static Version Version => R.Assembly.GetName().Version;
            public static int ID => Version.Major;
        }
        public static class Security
        {
            public static string MasterKey => K.Core.R.Security.MasterKey;
            public const int Expire = K.Core.R.Security.Expire;
        }        
    }
}
