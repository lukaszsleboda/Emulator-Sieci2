using System;
using System.Net;
using System.Net.Sockets;
namespace API
{
    

    public class NetworkDevice
    {
        

        //public Tuple<int,int> DeviceID { get; set; }
        public Tuple<int, int> SNPs { get; set; }
        public String Name { get; set; }
        public String DeviceType { get; set; }
        public IPEndPoint EndPoint { get; set; }


        public NetworkDevice(int Snp1, int Snp2, String Name, IPAddress IP, int Port, String devtype)
        {
            this.SNPs = new Tuple<int, int>(Snp1, Snp2);
            this.Name = Name;
            this.EndPoint = new IPEndPoint(IP, Port);
            this.setType(devtype);
        }

        public void setRouter()
        {
            this.DeviceType = NetworkDevTypes.ROUTER_TYPE;
        }
        public bool isRouter()
        {
            if (this.DeviceType == NetworkDevTypes.ROUTER_TYPE) { return true; }
            else { return false; }
        }
        public void setSubnetwork()
        {
            this.DeviceType = NetworkDevTypes.SUBNETWORK_TYPE;
        }
        public bool isSubnetwork()
        {
            if(this.DeviceType == NetworkDevTypes.SUBNETWORK_TYPE) { return true; }
            else { return false; }
        }

        public void setType(String type)
        {
            if (type == NetworkDevTypes.ROUTER_TYPE)
            {
                this.DeviceType = NetworkDevTypes.ROUTER_TYPE;
            }
            else if( type == NetworkDevTypes.SUBNETWORK_TYPE)
            {
                this.DeviceType = NetworkDevTypes.SUBNETWORK_TYPE;
            }
            else if (type == NetworkDevTypes.HOST_TYPE)
            {
                this.DeviceType = NetworkDevTypes.HOST_TYPE;
            }
        }
    }
}
