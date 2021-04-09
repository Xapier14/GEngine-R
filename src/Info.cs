using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;

namespace GEngine
{
    public static class Info
    {
        private static Assembly _assem = Assembly.GetExecutingAssembly();
        private static FileVersionInfo _ver = FileVersionInfo.GetVersionInfo(_assem.Location);
        private const string _product = "GEngine Re:";
        private const string _build = "Development Build";
        public static int MajorVersion
        {
            get
            {
                return _ver.FileMajorPart;
            }
        }
        public static int MinorVersion
        {
            get
            {
                return _ver.FileMinorPart;
            }
        }
        public static string VersionString
        {
            get
            {
                return $"{_product} v{MajorVersion}.{MinorVersion} - {_build}";
            }
        }
    }
}
