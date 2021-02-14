using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Linq;
using API.LOGS;
namespace NCC.Components
{
    public class Directory
    {
        public String DomainName { get; set; }

        public String devName { get; set; }
           
        public Dictionary<String,IPEndPoint> hostsDic { get; set; }


        public Directory(String filename, String domainName)
        {
            DirectoryReader.LoadConfig(this, filename);
            this.devName = "DIR";
            this.DomainName = domainName;
        }

        public bool isInMyDomain(String host)
        {
            try
            {
                String domain = host.Substring(2, 4);
                if(DomainName == domain) { return true; }
                else { return false; }
            }
            catch(Exception e)
            {
                Console.WriteLine("DICTIONARY OUT OF RANGE DOMAIN SEARCH");
                Console.WriteLine(e);
                Console.WriteLine("DICTIONARY OUT OF RANGE DOMAIN SEARCH");
            }
            return false;
            
        }

        public IPEndPoint getIPByName(String name)
        {
            String messageInd = $"<DirectoryRequestIndicator>: {name}";
            Logs.ControlLOG(devName, messageInd, Colors.DIRECTORY);

            IPEndPoint endPoint = hostsDic[name];
            String message = $"<DirectoryRequestResponse>: {name} -> {endPoint}";
            Logs.ControlLOG(devName, message, Colors.DIRECTORY);
            return endPoint;
        }

        public String getNameByIP(IPEndPoint endPoint)
        {
            String messageInd = $"<DirectoryRequestIndicator>: {endPoint}";
            Logs.ControlLOG(devName, messageInd, Colors.DIRECTORY);
            String key = "";
            bool is_found = false;
            while(!is_found)
            {
               // Console.WriteLine("In while");
                foreach (KeyValuePair<String, IPEndPoint> entry in hostsDic)
                {
                    if(entry.Value.Equals(endPoint))
                    {
                        key = entry.Key;
                        is_found = true;
                        break;
                    }
                }
            }

            //Console.WriteLine($"[TEST log]: got name {key} based on ip:{endPoint}");
            String message = $"<DirectoryRequestResponse>: {endPoint} -> {key}";

            Logs.ControlLOG(devName, message, Colors.DIRECTORY);
            return key;
        }
    }
}
