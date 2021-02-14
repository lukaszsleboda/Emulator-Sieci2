using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using API.Protocols.TransportProtocol;
using API.Protocols.Reader;
using API.BytesCoder;
using API.LOGS;


namespace CableCloud
{
    public class CableCloud
    {
        public IPEndPoint CableCloudEndPoint { get; set; }

        public IPEndPoint NextEndPoint { get; set; }

        public IPEndPoint LastEndPoint { get; set; }

        public String devName { get; set; }

        
        // Port (MPLS)
        public Dictionary<int, Cable> ConnectedCableList { get; set; }

        // IP and port (UDP)
        public Dictionary<String, IPEndPoint> IPlist { set; get; }

        UdpClient udp;


        public class Cable
        {
            public int OutPort { get; set; }
            public String OutNodeName { get; set; }
            public Boolean isAvailable { get; set; }
            public int Capacity { get; set; }

            public Cable(String Name, int Port, int Capacity)
            {
                this.OutNodeName = Name;
                this.OutPort = Port;
                this.isAvailable = true;
                this.Capacity = Capacity;
                
            }

        }
        

        public CableCloud(String filename)
        {
            CableCloudConfigReader.LoadCableCloudConfig(this, filename);
            Console.WriteLine($"Cloud: {CableCloudEndPoint.Address}:{CableCloudEndPoint.Port}");
            Console.Title = "Cable Cloud";
            this.devName = "CableCloud";

            asyncStart();

        }


        public void asyncStart()
        {
            udp = new UdpClient(CableCloudEndPoint);

            listener();
            while (true)
            {
                UserInterface();
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


                        if (protocol.isTransportProtocol())
                        {
                            TransportProtocolReader transportProtocol = TransportProtocolReader.EncodeDataFromString(protocol.Data);

                            String logMessage = $"[GOT PACKET] fromPort: {transportProtocol.Port}";
                            Logs.TransportLOG(devName: devName, message: logMessage);

                            var InPort = transportProtocol.Port;
                            var cable = ConnectedCableList[InPort];

                            if (ConnectedCableList[InPort].isAvailable == false)
                            {
                                String outNodeName = ConnectedCableList[ConnectedCableList[InPort].OutPort].OutNodeName;
                                String notAvaliableMessage = $"[ERROR] Cable Unavaliable {ConnectedCableList[InPort].OutNodeName}-{outNodeName}";
                                Logs.TransportLOG(devName: devName, message: notAvaliableMessage);
                            }
                            else
                            {
                                transportProtocol.Port = cable.OutPort;
                                var outNodeName = cable.OutNodeName;
                                var outIPEndPoint = IPlist[outNodeName];

                                
                                IPEndPoint destination = outIPEndPoint;

                                String messageToSend = transportProtocol.ToStringWithProtocolType();
                                byte[] message_bytes = ByteCoder.toBytes(messageToSend);

                                udp.Send(message_bytes, message_bytes.Length, destination);

                                String logMsg2 = $"[SEND PACKET] toPort:{cable.OutPort}";
                                Logs.TransportLOG(devName: devName, message: logMsg2);
                            }
                        }
                        /*
                        else if (package.isTerminateOrder())
                        {
                            Environment.Exit(0);
                        }
                        package = null;
                        */
                    }
                }
            });
        }


        public void UserInterface()
        {


            Console.WriteLine("\n\n---------Available options---------\n 1 -> Shows list of cables \n 2 -> Select cable to kill\n 3 -> Select cable to repare\n");
            String Command = Console.ReadLine();

            switch (Command)
            {
                case "1":
                    for (int i = 0; i < ConnectedCableList.Count; i++)
                    {
                        Console.WriteLine("NodeIn Name: {0}, Port in: {1}, NodeOut Name: {2}, Port out: {3}, Capacity: {4} Gb/s, is Available: {5} ", ConnectedCableList[ConnectedCableList.ElementAt(i).Value.OutPort].OutNodeName, ConnectedCableList.ElementAt(i).Key, ConnectedCableList.ElementAt(i).Value.OutNodeName, ConnectedCableList.ElementAt(i).Value.OutPort, ConnectedCableList.ElementAt(i).Value.Capacity, ConnectedCableList.ElementAt(i).Value.isAvailable);
                        i++;
                    }
                    break;

                case "2":
                    try
                    {
                        Console.WriteLine("Type port of cable:");
                        String PortToKill = Console.ReadLine();
                        ConnectedCableList[int.Parse(PortToKill)].isAvailable = false;
                        var portToKill_2 = ConnectedCableList[int.Parse(PortToKill)];
                        ConnectedCableList[portToKill_2.OutPort].isAvailable = false;
                        String outNodeName = ConnectedCableList[ConnectedCableList[int.Parse(PortToKill)].OutPort].OutNodeName;
                        String notAvaliableMessage = $"[INFO] Connection between {ConnectedCableList[int.Parse(PortToKill)].OutNodeName}-{outNodeName} destroyed";
                        Logs.TransportLOG(devName: devName, message: notAvaliableMessage);
                        //Logs.LogsCableCloudKillCable(ConnectedCableList[int.Parse(PortToKill)].OutNodeName, ConnectedCableList[ConnectedCableList[int.Parse(PortToKill)].OutPort].OutNodeName);

                    }
                    catch
                    {
                        Console.WriteLine("This port number doesn't exist");
                    }
                    break;

                case "3":
                    try
                    {
                        Console.WriteLine("Type port of cable:");
                        String PortToKill = Console.ReadLine();
                        ConnectedCableList[int.Parse(PortToKill)].isAvailable = true;
                        var portToKill_2 = ConnectedCableList[int.Parse(PortToKill)];
                        ConnectedCableList[portToKill_2.OutPort].isAvailable = true;

                        String outNodeName = ConnectedCableList[ConnectedCableList[int.Parse(PortToKill)].OutPort].OutNodeName;
                        String msgx = $"[INFO] Connection between {ConnectedCableList[int.Parse(PortToKill)].OutNodeName}-{outNodeName} repared";
                        Logs.TransportLOG(devName: devName, message: msgx);
                    }
                    catch
                    {
                        Console.WriteLine("This port number doesn't exist");
                    }
                    break;
               
                default:
                    Console.WriteLine("Wrong command");
                    break;
            }
        }
    }
}
