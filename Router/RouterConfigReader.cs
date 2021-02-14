using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Router
{
    class RouterConfigReader
    {
        public class RouterModel
        {
            public string Name { get; set; }
            public string IpAddress { get; set; }
            public int Port { get; set; }
            public string CloudIP { get; set; }
            public int CloudPort { get; set; }
        }

        public static void LoadConfig(Router router, String filename)
        {
            var jsonFile = File.ReadAllText(filename);
            RouterModel routerModel = JsonSerializer.Deserialize<RouterModel>(jsonFile);

            router.EndPoint = new IPEndPoint(IPAddress.Parse(routerModel.IpAddress), routerModel.Port);
            router.CableCloudEndPoint = new IPEndPoint(IPAddress.Parse(routerModel.CloudIP), routerModel.CloudPort);

            router.Name = routerModel.Name;

            router.FIB = new List<FIBRow>();

        }
    }
}
