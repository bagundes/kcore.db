﻿using System;
using System.Resources;

namespace KCore.DB
{
    public static class R
    {

        public static string ParamDB = "teamsoft";

        private static System.Reflection.Assembly Assembly => System.Reflection.Assembly.GetExecutingAssembly();
        private static ResourceManager resx;

        public static string Language = KCore.R.Language;
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
            public static string Name => "KCore Database extension";
            public static string Namespace => "KC";
            public static Version Version => R.Assembly.GetName().Version;
            public static int ID => Version.Major;
        }
        public static class Security
        {
            public static string MasterKey => KCore.R.Security.MasterKey;
            public static int Expire = KCore.R.Security.Expire;
        }
    }
}
