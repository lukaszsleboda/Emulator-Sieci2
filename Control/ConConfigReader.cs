using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using API;

namespace Control
{
    public class ConConfigReader
    {


        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
        public class NeighbourControlModel
        {
            public string Name { get; set; }
            public string IP { get; set; }
            public int Port { get; set; }
        }

        public class NetworkDeviceModel
        {
            public string Name { get; set; }
            public string IP { get; set; }
            public int Port { get; set; }
            public string DeviceType { get; set; }
            public List<int> Links { get; set; }
            public List<int> SNPs { get; set; }
        }

        public class CCModel
        {
            public string Name { get; set; }
            public List<NetworkDeviceModel> NetworkDevices { get; set; }
        }

        public class NCCModel
        {
            public string Name { get; set; }
            public string IP { get; set; }
            public int Port { get; set; }
        }

        public class Interface
        {
            public int SNP { get; set; }
            public int LinkID { get; set; }
        }

        public class Router
        {
            public string Name { get; set; }
            public List<Interface> Interfaces { get; set; }
        }

        public class RCModel
        {
            public string Name { get; set; }
            public List<Router> Routers { get; set; }
        }

        public class LinkModel
        {
            public int LinkName { get; set; }
            public int SNP1 { get; set; }
            public int SNP2 { get; set; }
            public int actualBandwidth { get; set; }
            public int maxBandwidth { get; set; }
            public List<object> usingLambdas { get; set; }
            public int length { get; set; }
            public bool isAlive { get; set; }
        }

        public class LRMModel
        {
            public string Name { get; set; }
            public List<LinkModel> Links { get; set; }
        }
        /*
        public class distanceModel
        {
            public String host1 { get; set; }
            public String host2 { get; set; }
            public int odleglosc { get; set; }
        }
        public class distancesModel
        {
            public List<distanceModel> distances { get; set; }
        }
        */

        public class ControlModel
        {
            public string DevName { get; set; }
            public string SubnetName { get; set; }
            public string IP { get; set; }
            public int Port { get; set; }
            public List<NeighbourControlModel> NeighbourControlModels { get; set; }
            public CCModel CCModel { get; set; }
            public NCCModel NCCModel { get; set; }
            public RCModel RCModel { get; set; }
            public LRMModel LRMModel { get; set; }
            //public distancesModel Distances { get; set; }
        }




        public static void LoadConfig(Control conn, String filename)
        {
            var jsonFile = File.ReadAllText(filename);

            ControlModel controlModel = JsonSerializer.Deserialize<ControlModel>(jsonFile);

            conn.cc = new Components.CallControler.CC();
            conn.lrm = new Components.LinkResourceManager.LRM();
            conn.rc = new Components.RouteControler.RC();

            conn.devName = controlModel.DevName;
            conn.DomainName = controlModel.SubnetName;

            conn.ControlEndPoint = new IPEndPoint(IPAddress.Parse(controlModel.IP.ToString()), controlModel.Port);

            conn.cc.Name = controlModel.CCModel.Name;
            conn.lrm.Name = controlModel.LRMModel.Name;
            conn.rc.devName = controlModel.RCModel.Name;

            //conn.rc.Name = ontrolModel.RCModel.Name;
            conn.NCCpoint = new IPEndPoint(IPAddress.Parse(controlModel.NCCModel.IP), controlModel.NCCModel.Port);

            conn.LinksList = new List<Link>();
            foreach (LinkModel link in controlModel.LRMModel.Links)
            {
                conn.LinksList.Add(new Link(link.LinkName, link.SNP1, link.SNP2, link.actualBandwidth, link.maxBandwidth, link.isAlive, link.length));
                    
            }

            conn.NetworkDevicesList = new List<NetworkDevice>();
            conn.RCInTable = new Dictionary<IPEndPoint, int>();
            foreach (NetworkDeviceModel device in controlModel.CCModel.NetworkDevices)
            {
                /*
                ///TODO Subnetwork niczym się nie różni od Routera. Oznacza to, że trzeba do NetworkDevice dodać nową kolumnę
                ///     Nowa kolumna -> SNPp, SNPk oznaczająca numer SNP odpowiadający numerowi Linka
                ///     Trzeba zachować w JSONie kolejność wpisywania SNP i Linków, żeby się zgadzało
                if(device.DeviceType == NetworkDevTypes.SUBNETWORK_TYPE)
                {
                    conn.NetworkDevicesList.Add(new NetworkDevice(device.Links[0], device.Links[1], device.Name, IPAddress.Parse(device.IP), device.Port, device.DeviceType));
                }
                */
                if (device.DeviceType == NetworkDevTypes.ROUTER_TYPE || device.DeviceType == NetworkDevTypes.SUBNETWORK_TYPE)
                {
                    int len = device.SNPs.Count;
                    for(int i=0; i<len-1; i++)
                    {
                        for(int j=i+1; j<len; j++)
                        {
                            conn.NetworkDevicesList.Add(new NetworkDevice(device.SNPs[i], device.SNPs[j], device.Name, IPAddress.Parse(device.IP), device.Port, device.DeviceType));
                        }
                    }
                }
                if( device.DeviceType == NetworkDevTypes.HOST_TYPE)
                {
                    conn.RCInTable.Add(new IPEndPoint(IPAddress.Parse(device.IP), device.Port), device.SNPs[0]);
                }
            }

            conn.Controls = new Dictionary<string, IPEndPoint>();
            foreach(NeighbourControlModel control in controlModel.NeighbourControlModels)
            {
                conn.Controls.Add(control.Name, new IPEndPoint(IPAddress.Parse(control.IP), control.Port));
            }

            /*
            conn.distances = new Dictionary<Tuple<IPAddress, IPAddress>, int>();
            foreach(distanceModel element in controlModel.Distances.distances)
            {
                //conn.distances.Add(new Tuple<IPAddress, IPAddress>(IPAddress.Parse(element.host1), IPAddress.Parse(element.host2)), element.odleglosc);
                //conn.distances.Add(new Tuple<IPAddress, IPAddress>(new IPAddress(IPAddress.Parse(element.host1), IPAddress.Parse(element.host2)), element.odleglosc);
            }
            */
            LoadDistances(conn);
        }

        public static void LoadDistances(Control conn)
        {
            conn.distances = new Dictionary<Tuple<IPAddress, IPAddress>, int>();
            conn.distances.Add(new Tuple<IPAddress, IPAddress>(IPAddress.Parse("127.0.0.11"), IPAddress.Parse("127.0.0.12")), 120);
            conn.distances.Add(new Tuple<IPAddress, IPAddress>(IPAddress.Parse("127.0.0.12"), IPAddress.Parse("127.0.0.11")), 120);
            conn.distances.Add(new Tuple<IPAddress, IPAddress>(IPAddress.Parse("127.0.0.11"), IPAddress.Parse("127.0.0.13")), 230);
            conn.distances.Add(new Tuple<IPAddress, IPAddress>(IPAddress.Parse("127.0.0.13"), IPAddress.Parse("127.0.0.11")), 230);
            conn.distances.Add(new Tuple<IPAddress, IPAddress>(IPAddress.Parse("127.0.0.11"), IPAddress.Parse("127.0.0.14")), 210);
            conn.distances.Add(new Tuple<IPAddress, IPAddress>(IPAddress.Parse("127.0.0.14"), IPAddress.Parse("127.0.0.11")), 210);
            conn.distances.Add(new Tuple<IPAddress, IPAddress>(IPAddress.Parse("127.0.0.12"), IPAddress.Parse("127.0.0.13")), 230);
            conn.distances.Add(new Tuple<IPAddress, IPAddress>(IPAddress.Parse("127.0.0.13"), IPAddress.Parse("127.0.0.12")), 230);
            conn.distances.Add(new Tuple<IPAddress, IPAddress>(IPAddress.Parse("127.0.0.12"), IPAddress.Parse("127.0.0.14")), 210);
            conn.distances.Add(new Tuple<IPAddress, IPAddress>(IPAddress.Parse("127.0.0.14"), IPAddress.Parse("127.0.0.12")), 210);
            conn.distances.Add(new Tuple<IPAddress, IPAddress>(IPAddress.Parse("127.0.0.13"), IPAddress.Parse("127.0.0.14")), 120);
            conn.distances.Add(new Tuple<IPAddress, IPAddress>(IPAddress.Parse("127.0.0.14"), IPAddress.Parse("127.0.0.13")), 120);
        }

    }
}
