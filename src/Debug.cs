using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEngine
{
    public static class Debug
    {
        public static bool WriteToFile = false;
        public static void Log(string title, string msg)
        {
            StreamWriter sw = null;
            DateTime now = DateTime.Now;
            if (WriteToFile)
            {
                try
                {
                    sw = File.AppendText("log.txt");
                } catch
                {
                    //sw.Close();
                }
            }
            if (title != "")
            {
                Console.WriteLine($"[{title}] {msg}");
                if (sw != null) sw.WriteLine($"[{now.ToString()}] [{title}] {msg}");

            } else
            {
                Console.WriteLine($"{msg}");
                if (sw != null) sw.WriteLine($"[{now.ToString()}] {msg}");
            }
            if (sw != null) sw.Close();
        }
        public static void Log(string msg)
        {
            Log("", msg);
        }
    }
}
