using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using API;

namespace TestsPoligon
{
    public class PolygonReader
    {
        
        public class NetworkDeviceModel
        {
            public string Name { get; set; }
            public string IP { get; set; }
            public int Port { get; set; }
            public string DeviceType { get; set; }
            public List<int> Links { get; set; }
            public List<int> SNPs { get; set; }

            public NetworkDeviceModel() { }
            public NetworkDeviceModel(NetworkDeviceModel n)
            {
                this.Name = n.Name;
                this.IP = n.IP;
                this.Port = n.Port;
                this.DeviceType = n.DeviceType;
                this.Links = n.Links;
                this.SNPs = n.SNPs;
            }
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

        public class RouterModel
        {
            public string Name { get; set; }
            public List<Interface> Interfaces { get; set; }
        }

        public class RCModel
        {
            public string Name { get; set; }
            public List<RouterModel> Routers { get; set; }
        }

        public class LinkModel
        {
            public int LinkID { get; set; }
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

        public class ControlModel
        {
            public string DevName { get; set; }
            public string SubnetName { get; set; }
            public string IP { get; set; }
            public int Port { get; set; }
            public CCModel CCModel { get; set; }
            public NCCModel NCCModel { get; set; }
            public RCModel RCModel { get; set; }
            public LRMModel LRMModel { get; set; }
        }

        public static void LoadConfig(Polygon conn, String filename)
        {
            var jsonFile = File.ReadAllText(filename);

            ControlModel controlModel = JsonSerializer.Deserialize<ControlModel>(jsonFile);


            conn.LinksList = new List<Link>();
            foreach (LinkModel link in controlModel.LRMModel.Links)
            {
                conn.LinksList.Add(new Link(link.LinkID, link.SNP1, link.SNP2, link.actualBandwidth, link.maxBandwidth, link.isAlive, link.length));

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
                    for (int i = 0; i < len - 1; i++)
                    {
                        for (int j = i + 1; j < len; j++)
                        {
                            conn.NetworkDevicesList.Add(new NetworkDevice(device.SNPs[i], device.SNPs[j], device.Name, IPAddress.Parse(device.IP), device.Port, device.DeviceType));
                        }
                    }
                }
                if (device.DeviceType == NetworkDevTypes.HOST_TYPE)
                {
                    conn.RCInTable.Add(new IPEndPoint(IPAddress.Parse(device.IP), device.Port), device.SNPs[0]);
                }
            }

        }
        /*
        public static void LoadConfig(Polygon conn, String filename)
        {
            var jsonFile = File.ReadAllText(filename);
            ControlModel controlModel = JsonSerializer.Deserialize<ControlModel>(jsonFile);

            conn.NetworkDevicesList = new List<NetworkDevice>();
            conn.RCInTable = new Dictionary<IPEndPoint, int>();
            conn.devices = new List<NetworkDeviceModel>();
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
        /*
                if (device.DeviceType == NetworkDevTypes.ROUTER_TYPE || device.DeviceType == NetworkDevTypes.SUBNETWORK_TYPE)
                {
                    int len = device.SNPs.Count;
                    for (int i = 0; i < len - 1; i++)
                    {
                        for (int j = i + 1; j < len; j++)
                        {
                            conn.NetworkDevicesList.Add(new NetworkDevice(device.SNPs[i], device.SNPs[j], device.Name, IPAddress.Parse(device.IP), device.Port, device.DeviceType));
                        }
                    }
                }
                if (device.DeviceType == NetworkDevTypes.HOST_TYPE)
                {
                    conn.RCInTable.Add(new IPEndPoint(IPAddress.Parse(device.IP), device.Port), device.SNPs[0]);
                }
            }

            conn.LinksList = new List<Link>();

            foreach (LinkModel link in controlModel.LRMModel.Links)
            {
                conn.LinksList.Add(new Link(link.LinkID, link.SNP1, link.SNP2, link.actualBandwidth, link.maxBandwidth, link.isAlive, link.length));
            }
        }
        */
    }
}
