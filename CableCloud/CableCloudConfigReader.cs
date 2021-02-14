using System;
using System.IO;
using System.Configuration;
using System.Collections.Specialized;
using System.Xml;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace CableCloud
{
    public class CableCloudConfigReader
    {
        public CableCloudConfigReader()
        {

        } // ip ipendpoint

        public class CableModel
        {
            public int PortIn { get; set; }
            public String NodeOutName { get; set; }
            public int Port2Out { get; set; }
            public int Capacity { get; set; }

        }

        public class IPListModel
        {
            public String NodeName {get; set;}
            public String IpAddress { get; set; }
            public int Port { get; set; }
        }
        

        public class CableCloudModel
        {
            public String IpAddress { get; set; }
            public int Port { get; set; }
            public List<CableModel> cableList { get; set; }
            public List<IPListModel> ipList { get; set; }

        }

        public static void LoadCableCloudConfig(CableCloud cableCloud, String filename)
        {
            var jsonFile = File.ReadAllText(filename);
            CableCloudModel cableCloudModel = JsonSerializer.Deserialize<CableCloudModel>(jsonFile);

            cableCloud.CableCloudEndPoint = new IPEndPoint(IPAddress.Parse(cableCloudModel.IpAddress.ToString()), cableCloudModel.Port);

            cableCloud.ConnectedCableList = new Dictionary<int,CableCloud.Cable>();

            cableCloud.IPlist = new Dictionary<string, IPEndPoint>();

             
            foreach (CableModel element in cableCloudModel.cableList)
            {
                
                cableCloud.ConnectedCableList.Add(element.PortIn, new CableCloud.Cable(element.NodeOutName, element.Port2Out, element.Capacity));
            }

            foreach (IPListModel element in cableCloudModel.ipList)
            {

                cableCloud.IPlist.Add(element.NodeName, new IPEndPoint(IPAddress.Parse(element.IpAddress), element.Port));

            }

        }
    }
}
