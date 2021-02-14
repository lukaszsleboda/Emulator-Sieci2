using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Host.Components;

using API.BytesCoder;
using API.Protocols.Reader;
using API.Protocols.ControlProtocol.Reader;
using API.Protocols.ControlProtocol.Actions;
using API.Protocols.TransportProtocol;
using API.LOGS;
using API;

namespace Host
{
    public class Host
    {

        public UdpClient udp;

        public IPEndPoint EndPoint { get; set; }
        public IPEndPoint CloudEndPoint { get; set; }
        public String hostName { get; set; }
        public List<string> HostList { get; set; }
        public CPCC cpcc { get; set; }
        public Dictionary<int, ConnectionCache> connections {get; set;}


        public Host(String filename)
        {
            cpcc = new CPCC();
            HostConfigReader.LoadConfig(this, filename);
            Console.WriteLine($"IP:{EndPoint}");
            Console.WriteLine("Possible Hosts:");
            foreach(String hst in HostList)
            {
                Console.WriteLine(hst);
            }
            asyncStart();
        }

        public void asyncStart()
        {
            udp = new UdpClient(EndPoint);

            listener();
            while (true)
            {
                Console.WriteLine("1-> Set connection\n2-> Send packet\n3-> CallTeardown\n4-> Show connections");
                String x = Console.ReadLine();
                if (x == "1")
                {
                    Console.WriteLine("Destination host: ");
                    String tmp = Console.ReadLine();
                    Console.WriteLine("Bandwidth: ");
                    String tmp2 = Console.ReadLine();
                    cpcc.CallRequestRequest(hostName,tmp, int.Parse(tmp2), udp);
                }
                if (x == "2")
                {
                    foreach (KeyValuePair<int, CPCCCache> entry in cpcc.connectionsList)
                    {
                        if (entry.Value.FromHost == hostName)
                        {
                            Console.WriteLine($"{entry.Key}: {entry.Value.FromHost} -> {entry.Value.ToHost}  {entry.Value.bandwitdh.ToString()}");
                        }
                    }
                    Console.WriteLine("Your data: ");
                    String data = Console.ReadLine();
                    Console.WriteLine("Pick connection");
                    int tmp3 = int.Parse(Console.ReadLine());
                    String toHost = cpcc.connectionsList[tmp3].ToHost;
                    int bandwidth = cpcc.connectionsList[tmp3].bandwitdh;
                    int req_lbd = cpcc.connectionsList[tmp3].req_lambdas;
                    int first_lbd = cpcc.connectionsList[tmp3].first_lambda;
                    HostActions.SendPacket(hostName, cpcc.connectionsList[tmp3].ToHost, bandwidth.ToString(), req_lbd.ToString(), first_lbd.ToString(), data, udp, EndPoint, CloudEndPoint);
                }
                if (x == "4")
                {
                    foreach (KeyValuePair<int, CPCCCache> entry in cpcc.connectionsList)
                    {
                   
                            Console.WriteLine($"{entry.Key}: {entry.Value.FromHost} -> {entry.Value.ToHost}  {entry.Value.bandwitdh.ToString()}");
                        
                    }
                }

              /*  if (x == "3")
                {
                    String h1 = "H1";
                    String h2 = "H2";
                    String  bndwidth = "12";
                    String l1 = "5";
                    String l2 = "5";
                    Tuple<int, int> lambdas = new Tuple<int, int>(12, 23);
                    String data = "Test message";
                    HostActions.SendPacket(h1, h2, bndwidth, l1,l2,data,udp,EndPoint, CloudEndPoint);
                }
                if (x == "4")
                {
                    
                }
              */
                if(x=="3")
                {
                    foreach (KeyValuePair<int, CPCCCache> entry in cpcc.connectionsList)
                    {

                        Console.WriteLine($"{entry.Key}: {entry.Value.FromHost} -> {entry.Value.ToHost}  {entry.Value.bandwitdh.ToString()}");
                    }
                    Console.WriteLine("Pick link: ");
                    String select = Console.ReadLine();
                    try
                    {
                        int select2 = Convert.ToInt32(select);

                        if (cpcc.connectionsList.ContainsKey(select2))
                        {
                            CPCCCache cache = cpcc.connectionsList[select2];
                            String guid = cache.guid;
                            String toHost = cache.ToHost;
                            String fromHost = cache.FromHost;
                            cpcc.CallRequestRequest(cache.FromHost, cache.ToHost, cache.bandwitdh, udp, true, select2, cache.guid);
                        }

                    }
                    catch
                    {

                    }
                }
            }
        }

        public void listener()
        {
            Task.Run(async () =>
            {
                using (var updClient = udp)
                {
                    while (true)
                    {
                        var result = await updClient.ReceiveAsync();
                        byte[] resultBytes = result.Buffer;

                        //Tworzymy Reader protokołów
                        ProtocolReader protocol = ProtocolReader.fromString(ByteCoder.fromBytes(resultBytes));
                        

                        if(protocol.isControlProtocl())
                        {
                            //Console.WriteLine("CONTROL PROTOCOL!");
                            ControlPlain(protocol);
                        }

                        else if (protocol.isTransportProtocol())
                        {
                            TransportPlain(protocol);
                        }
                    }
                }
            });
        }



        ///******************************
        ///********CONTROL PLAIN*********
        ///******************************
        public void ControlPlain(ProtocolReader protocol)
        {
            //Tworzymy Reader warstwy zarządzania
            connections = new Dictionary<int, ConnectionCache>();
            ControlProtocolReader ControlProtocol = ControlProtocolReader.EncodeDataFromString(protocol.Data);
         //   Console.WriteLine("HAVE CONTROL PROTOCOL!");
          //  Console.WriteLine(ControlProtocol.CommandType);

            if (ControlActionsReader.isCallAcceptRequest(ControlProtocol))
            {
                String data = ControlProtocol.Data;
                String[] dataString = data.Split('&');
                String h1Name = dataString[0];
                String h2Name = dataString[1];
                int bandwidth = int.Parse(dataString[2]);

                if (ControlProtocol.Action == StaticActions.ALLOCATE)
                {

                    CPCCCache cache = new CPCCCache();
                    cache.guid = ControlProtocol.ID();
                    cache.FromHost = h1Name;
                    cache.ToHost = h2Name;
                    cache.bandwitdh = bandwidth;
                    cpcc.connectionsList.Add(cpcc.counter, cache);
                    cpcc.counter++;
                    cpcc.CallAcceptResponse(this, ControlProtocol);

                    String requetsMessgae = $"<CallAcceptIndicator> {h1Name} -> {h2Name} {bandwidth.ToString()}Gb/s ";
                    Logs.ControlLOG(cpcc.cpccName, requetsMessgae, Colors.CPCC);

                }
                else if(ControlProtocol.Action == StaticActions.DEALLOCATE)
                {
                    foreach (KeyValuePair<int, CPCCCache> entry in cpcc.connectionsList)
                    {
                        if(entry.Value.guid == ControlProtocol.ID())
                        {
                            cpcc.connectionsList.Remove(entry.Key);
                            String requetsMessgae = $"<CallTeardownIndicator> {h1Name} -> {h2Name} {bandwidth.ToString()}Gb/s ";
                            Logs.ControlLOG(cpcc.cpccName, requetsMessgae, Colors.CPCC);

                            cpcc.CallAcceptResponse(this, ControlProtocol);


                            break;

                        }
                    }
                }




                
            }
            else if (ControlActionsReader.isCallTeardown(ControlProtocol))
            {

            }
            else if (ControlActionsReader.isCallRequestResponse(ControlProtocol))
            {




                String data = ControlProtocol.Data;
                String[] dataString = data.Split('&');
                String h1Name = dataString[0];
                String h2Name = dataString[1];
                String bandwidth = dataString[2];
                String req_lbd = dataString[3];
                String first_lbd = dataString[4];


                if (ControlProtocol.Action == StaticActions.ALLOCATE)
                {


                    cpcc.connectionsList[cpcc.counter].first_lambda = int.Parse(dataString[4]);
                    cpcc.connectionsList[cpcc.counter].req_lambdas = int.Parse(dataString[3]);
                    String callresponseLog = $"<CallRequestResponse> {h1Name} -> {h2Name}  {bandwidth}";
                    Logs.ControlLOG(cpcc.cpccName, callresponseLog, Colors.CPCC);
                    //HostActions.SendPacket(h1Name, h2Name, bandwidth, req_lbd, first_lbd, data, udp, EndPoint, CloudEndPoint);
                    cpcc.counter++;
                }
                else if (ControlProtocol.Action == StaticActions.DEALLOCATE)
                {
                    String callresponseLog = $"<CallTeradownResponse> {h1Name} -> {h2Name}  {bandwidth}";
                    Logs.ControlLOG(cpcc.cpccName, callresponseLog, Colors.CPCC);

                    int tmp = -1;
                    foreach (KeyValuePair<int, CPCCCache> entry in cpcc.connectionsList)
                    {
                        if (entry.Value.guid == ControlProtocol.ID())
                        {
                            tmp = entry.Key;
                        }
                    }
                    if (tmp != -1)
                    {
                        cpcc.connectionsList.Remove(tmp);
                    }

                }
                //cpcc.CR.Response(h1Name, h2Name, bandwidth, udp);
            }

        }


        ///********************************
        ///********TRANSPORT PLAIN*********
        ///********************************
        public void TransportPlain(ProtocolReader protocol)
        {
            TransportProtocolReader transportProtocol = TransportProtocolReader.EncodeDataFromString(protocol.Data);

            String logsMessage = $"[PACKET GOT] {transportProtocol.SenderName} -> {transportProtocol.DestName} of {transportProtocol.bandwidth}Gb/s [Required_lambdas: {transportProtocol.required_lambdas} starts_at: {transportProtocol.firstLambda}]: {transportProtocol.Data}";
            Logs.TransportLOG(hostName, logsMessage, Colors.HOST);
        }

    }
}
