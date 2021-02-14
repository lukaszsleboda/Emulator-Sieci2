using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Host
{
    class HostConfigReader
    {

        public class CPCC
        {
            public string Name { get; set; }
            public string NCCIp { get; set; }
            public int NCCPort { get; set; }
        }

        public class HostModel
        {
            public string HostName { get; set; }
            public string IpAddress { get; set; }
            public int Port { get; set; }
            public string CloudIP { get; set; }
            public int CloudPort { get; set; }
            public List<string> HostList { get; set; }
            public CPCC CPCC { get; set; }
        }

        public static void LoadConfig(Host host, String filename)
        {
            var jsonFile = File.ReadAllText(filename);
            HostModel hostModel = JsonSerializer.Deserialize<HostModel>(jsonFile);

            host.hostName = hostModel.HostName;
            host.EndPoint = new IPEndPoint(IPAddress.Parse(hostModel.IpAddress.ToString()), hostModel.Port);
            host.CloudEndPoint = new IPEndPoint(IPAddress.Parse(hostModel.CloudIP.ToString()), hostModel.CloudPort);
            host.HostList = new List<String>(hostModel.HostList);
            host.cpcc.cpccName = hostModel.CPCC.Name;
            host.cpcc.NCCendPoint = new IPEndPoint(IPAddress.Parse(hostModel.CPCC.NCCIp.ToString()), hostModel.CPCC.NCCPort);
        }

    }
}
