using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

namespace NCC.Components
{
    public class Host
    {
        public string Name { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
    }

    public class Dirmodel
    {
        public List<Host> Hosts { get; set; }
    }

    class DirectoryReader
    {
        public static void LoadConfig(Directory directory, String filename)
        {
            var jsonFile = File.ReadAllText(filename);
            Dirmodel dirModel = JsonSerializer.Deserialize<Dirmodel>(jsonFile);

            directory.hostsDic = new Dictionary<String, IPEndPoint>();

            foreach(Host host in dirModel.Hosts)
            {
                directory.hostsDic.Add(host.Name, new IPEndPoint(IPAddress.Parse(host.IP.ToString()), host.Port));
                
            }

            //Console.WriteLine("DIRECTORY:");
            //foreach (KeyValuePair<string, IPEndPoint> entry in directory.hostsDic)
            //{
            //    Console.WriteLine($"{entry.Key} : {entry.Value}");
            //}


        }
    }


}
