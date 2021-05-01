using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GEngine
{
    public static class Loader
    {
        public static bool TryParseIni(string fileLocation, out Dictionary<string, string> result)
        {
            result = null;
            if (!File.Exists(fileLocation))
            {
                return false;
            }
            string[] ini = File.ReadAllLines(fileLocation);
            result = new Dictionary<string, string>();
            Regex dataPairPattern = new Regex(@"(.+)\s?=\s?(.+)", RegexOptions.None);
            foreach (string line in ini)
            {
                Match match = dataPairPattern.Match(line);
                if (match.Success)
                {
                    result.Add(match.Groups[1].Value.Trim(' ', '"'), match.Groups[2].Value.Trim(' ', '"'));
                } else
                {
                    //not a valid line
                    Debug.Log($"'{line}' is not a valid line.", "ParseIni");
                }
            }
            return true;
        }
    }
}
