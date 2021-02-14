using System;
using System.Collections.Generic;
using System.Text;
using API;
using System.Net;

namespace TestsPoligon
{
    public class Polygon
    {

        public List<NetworkDevice> NetworkDevicesList { get; set; }
        public Dictionary<IPEndPoint, int> RCInTable { get; set; }
        public List<Link> LinksList { get; set; }
        public List<PolygonReader.NetworkDeviceModel> devices { get; set; }
        public Polygon(String filename)
        {
            PolygonReader.LoadConfig(this, filename);
        }
    }
}
