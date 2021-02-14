using System;
using System.IO;

namespace API.LOGS
{
    public class SaveToFile
    {
        public SaveToFile()
        {
        }

        public static void WriteToFile(String log, String protocolType, String nodeName)
        {
            string path = $"./logs/{protocolType}/{nodeName}.txt";
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(log);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(log);
                }
            }

        }
    }
}
