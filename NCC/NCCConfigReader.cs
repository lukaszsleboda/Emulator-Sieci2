using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

namespace NCC
{
    class NCCConfigReader
    {
        public class CCModel
        {
            public string name { get; set; }
            public string Ip { get; set; }
            public int Port { get; set; }
        }

        public class NCCneighborModel
        {
            public string Name { get; set; }
            public string IP { get; set; }
            public int Port { get; set; }
        }

        public class NCCModel
        {
            public string DevName { get; set; }
            public string DomainName { get; set; }
            public string IpAddress { get; set; }
            public int Port { get; set; }
            public string dirFilename { get; set; }
            public CCModel CCModel { get; set; }
            public NCCneighborModel NCCneighbor { get; set; }
        }


        public static void LoadConfig(NCC ncc, String filename)
        {
            var jsonFile = File.ReadAllText(filename);
            NCCModel nccModel = JsonSerializer.Deserialize<NCCModel>(jsonFile);

            ncc.devName = nccModel.DevName;
           // Console.WriteLine(ncc.devName);
            ncc.domainName = nccModel.DomainName;
           // Console.WriteLine(ncc.domainName);
            ncc.NCCEndPoint = new IPEndPoint(IPAddress.Parse(nccModel.IpAddress), nccModel.Port);
          //  Console.WriteLine(ncc.NCCEndPoint);


            ncc.dirFilename = nccModel.dirFilename;
         //   Console.WriteLine(ncc.dirFilename);

            ncc.nccNeighbour = new NCCNeighbour(nccModel.NCCneighbor.Name, new IPEndPoint(IPAddress.Parse(nccModel.NCCneighbor.IP.ToString()),nccModel.NCCneighbor.Port));
          //  Console.WriteLine(ncc.nccNeighbour.Name);


          //  Console.WriteLine("***");
          //  Console.WriteLine(nccModel.CCModel.name);
         //   Console.WriteLine(nccModel.CCModel.Port);
          //  Console.WriteLine(nccModel.CCModel.Ip.ToString());

            ncc.ccEndPoint = new IPEndPoint(IPAddress.Parse(nccModel.CCModel.Ip.ToString()), nccModel.CCModel.Port);
          //  Console.WriteLine(ncc.ccEndPoint);





        }

    }
}
