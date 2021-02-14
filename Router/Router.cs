using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using API.BytesCoder;
using API.LOGS;
using API.Protocols.Reader;
using API.Protocols.TransportProtocol;
using API.Protocols.ControlProtocol.Reader;
using API.Protocols.ControlProtocol.Actions;

namespace Router
{
    class Router
    {
        public IPEndPoint EndPoint { get; set; }
        public IPEndPoint CableCloudEndPoint { get; set; }
        public String Name { get; set; }

        public List<FIBRow> FIB { get; set; }

        UdpClient udp;

        public Router(String filename)
        {
            RouterConfigReader.LoadConfig(this, filename);
           

            Console.WriteLine($"ROUTER: {EndPoint.Address}:{EndPoint.Port}");

            udp = new UdpClient(EndPoint);

            asyncStart();
        }

        public void asyncStart()
        {
            listener();
            
            while (true)
            {
                String x = Console.ReadLine();
                if (x == "1")
                {
                    printFIB();
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

                        if (protocol.isTransportProtocol())
                        {
                            TransportProtocolReader transportProtocol = TransportProtocolReader.EncodeDataFromString(protocol.Data);


                            String logmsg1 = $"[PACKET RECEIVED] toPort: {transportProtocol.Port} required_lambdas:{transportProtocol.required_lambdas} starts_at: {transportProtocol.firstLambda} ";
                            Logs.TransportLOG(Name, logmsg1, Colors.ROUTER);

                            bool found_row = false;
                            while (!found_row)
                            {
                                foreach (FIBRow row in FIB)
                                {
                                    if (transportProtocol.Port == row.PortIn && transportProtocol.required_lambdas == row.ReqLmbd && transportProtocol.firstLambda == row.firstLambda)
                                    {

                                        int outPort = row.PortOut;
                                        transportProtocol.Port = outPort;
                                        byte[] message_bytes_ = ByteCoder.toBytes(transportProtocol.ToStringWithProtocolType());

                                        udp.Send(message_bytes_, message_bytes_.Length, CableCloudEndPoint);

                                        String logmsg2 = $"[PACKET SEND] fromPort: {transportProtocol.Port} required_lambdas:{transportProtocol.required_lambdas} starts_at: {transportProtocol.firstLambda}";
                                        Logs.TransportLOG(Name, logmsg2, Colors.ROUTER);
                                    }
                                }
                                found_row = true;
                            }
                        }

                        else if (protocol.isControlProtocl())
                        {
                            ControlProtocolReader controlProtocol = ControlProtocolReader.EncodeDataFromString(protocol.Data);


                            if (ControlActionsReader.isRouterUpdate(controlProtocol))
                            {
                                String data = controlProtocol.Data;
                                String[] dataString = data.Split('&');
                                int portIn = int.Parse(dataString[0]);
                                int portOut = int.Parse(dataString[1]);
                                int required_lambdas = int.Parse(dataString[2]);
                                int first_lambda = int.Parse(dataString[3]);
                                if (controlProtocol.Action == StaticActions.ALLOCATE)
                                {
                                    Console.WriteLine($"Row added to FIB: {portIn} [{required_lambdas},{first_lambda}] -> {portOut}");
                                    
                                    FIB.Add(new FIBRow(portIn, portOut, required_lambdas, first_lambda));

                                }
                                else if (controlProtocol.Action == StaticActions.DEALLOCATE)
                                {
                                    Console.WriteLine("fib " + FIB.Count);
                                    if (FIB.Count != 0)
                                    {
                                        foreach (FIBRow row in FIB)
                                        {
                                            if (row.PortIn == portIn && row.PortOut == portOut && row.ReqLmbd == required_lambdas && row.firstLambda == first_lambda)
                                            {
                                                Console.WriteLine($"Row removed from FIB: {portIn} [{required_lambdas},{first_lambda}] -> {portOut}");

                                                FIB.Remove(row);
                                                break;
                                            }
                                        }
                                    }
                                    
                                }
                            }
                        }
                    }

                }
            });
        }

        public void printFIB()
        {
            
            Console.WriteLine("*************FIB*************");
            foreach (FIBRow row in FIB)
            {
                Console.WriteLine($"{row.PortIn} [{row.ReqLmbd}, {row.firstLambda}] -> {row.PortOut}");
            }
            Console.WriteLine("******************************");
        }
    }
}
