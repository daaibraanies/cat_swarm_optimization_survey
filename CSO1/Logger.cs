using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace CSO1
{
    public static class Logger
    {
        private const string filePath = @"C:\Users\leshc\Desktop\Файлы для портфолио\CSO1\CSO1\LOGS.txt";
        public enum Importance
        {
            Info,
            Warrning,
            Fatal,
            SYSTEM
        };
        private static string _datestamp;


        public static void LogIt(string message, Importance imp = Importance.Info)
        {
            //_datestamp = DateTime.Now.ToString();
            using (StreamWriter logWriter = new StreamWriter(filePath, true,Encoding.UTF8))
            {
                logWriter.WriteLine
                    (
                            "[" + imp + "]" +
                            " " + message + " " 
                            
                    );
                logWriter.Close();
            }
        }

    }
}
