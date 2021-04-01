using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEngine
{
    public static class Debug
    {
        public static void Log(string title, string msg)
        {
            if (title != "")
            {
                Console.WriteLine($"[{title}] {msg}");
            } else
            {
                Console.WriteLine($"{msg}");
            }
        }
        public static void Log(string msg)
        {
            Log("", msg);
        }
    }
}
