using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NCC.Components;
using API.Protocols.Reader;
using API.Protocols.ControlProtocol.Actions;
using API.Protocols.ControlProtocol.Components;
using API.Protocols.ControlProtocol.Reader;
using API.BytesCoder;
using API.LOGS;
using NCC.Actions;

namespace NCC
{
    public class NCCNeighbour
    {
        public String Name { get; set; }
        public IPEndPoint EndPoint { get; set; }

        public NCCNeighbour() { }
        public NCCNeighbour(String name, IPEndPoint endPoint)
        {
            this.Name = name;
            this.EndPoint = endPoint;
        }
    }

    public class NCC
    {
        public String devName { get; set; }
        public String domainName { get; set; }
        public IPEndPoint NCCEndPoint { get; set; }
        public String dirFilename { get; set; }
        public IPEndPoint ccEndPoint { get; set; }

        public NCCNeighbour nccNeighbour { get; set; }

        public Directory dir { get; set; }

        

        public UdpClient udp;


        public NCC(String filename)
        {
            NCCConfigReader.LoadConfig(this, filename);
            Console.WriteLine($"NCC: {NCCEndPoint}");
            dir = new Directory(dirFilename, domainName);
            asyncStart();
        }

        public void asyncStart()
        {
            udp = new UdpClient(NCCEndPoint);

            listener();
            while (true)
            {
                Console.ReadLine();
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
                        ProtocolReader protocol = ProtocolReader.fromString(ByteCoder.fromBytes(resultBytes));
                        if (protocol.isControlProtocl())
                        {
                          //  Console.WriteLine("isControlProtocl");

                            ControlPlain(protocol);
                        }
                        //jeśli przyszło CallRequest od CPCC
                        
                        
                        


                    }
                }
            });
        }



        public void ControlPlain(ProtocolReader protocol)
        {
            ControlProtocolReader ControlProtocol = ControlProtocolReader.EncodeDataFromString(protocol.Data);

           // Console.WriteLine("IN NCC");
            //odbiór wiadomości CPCC->NCC
            if (ControlActionsReader.isCallRequestRequest(ControlProtocol))
            {
               // Console.WriteLine("isCallRequestRequest");

                Actions.NccLogs.IndicatorLog(this, ControlProtocol, "<CallRequestIndicator>");
                Actions.FakeActions.Policy(ControlProtocol, devName);

                //Czy dst jest w naszej domenie?
                if (Actions.FakeActions.isInMyDomain(ControlProtocol,true,domainName))
                {
               //     Console.WriteLine("IsInMyDomain");
                    //JEŚLI JEST -> CallAcceptRequest
                    Actions.RealActions.CallAcceptRequest(this, ControlProtocol);            //GOTOWE
                }
                else
                {
                    //JEŚLI NIE -> CallCoordinationRequest
                    Actions.RealActions.CallCoordinationRequest(this, ControlProtocol);      //GOTOWE
                }
            }
            //NCC->NCC
            else if(ControlActionsReader.isCallCoordinationRequest(ControlProtocol))
            {
                Actions.NccLogs.IndicatorLog(this, ControlProtocol, "<CallCoordinationIndicator>");

                //CallAcceptRequest
                Actions.RealActions.CallAcceptRequest(this, ControlProtocol);                //GOTOWE  

            }
            //Zwrotka od CPCC
            else if(ControlActionsReader.isCallAcceptResponse(ControlProtocol))
            {
                Actions.NccLogs.IndicatorLog(this, ControlProtocol, "<CallAcceptConfirm>");     

                if (Actions.FakeActions.isInMyDomain(ControlProtocol, false, domainName))
                {
                    //JEŚLI JEST -> ConnectionRequestRequest
                    Actions.RealActions.ConnectionRequestRequest(this, ControlProtocol);        //GOTOWE
                }
                else
                {
                    Actions.RealActions.CallCoordinationResponse(this, ControlProtocol);        //GOTOWE
                    //JEŚLI NIE -> CallCoordinationReponse
                }

            }
            //Zwrotka od CC
            else if(ControlActionsReader.isConnectionRequestResponse(ControlProtocol))
            {


                Actions.RealActions.CallRequestReponse(this, ControlProtocol);   //GOTOWE
            }

            else if (ControlActionsReader.isCallCoordinationResponse(ControlProtocol))
            {
                Actions.RealActions.ConnectionRequestRequest(this, ControlProtocol);
            }





            /*

            //Jeśli przyjdzie CallRequestRequest od CPCC
            if (ControlActionsReader.isCallRequestRequest(ControlProtocol))
            {
                String dstHostDomain = ControlProtocol.Data.Split('&')[1][^2..];
                String sndHost = ControlProtocol.Data.Split('&')[0];
                String dstHost = ControlProtocol.Data.Split('&')[1];
                String bandwidth = ControlProtocol.Data.Split('&')[2];

                String callRequestRequestMessage = $"<CallRequestIndicator> {sndHost} -> {dstHost} /{bandwidth}/Gb/s";
                Logs.ControlLOG(devName, callRequestRequestMessage, Colors.NCC);


                Action.Policy(sndHost, dstHost);

                //Jeśli host docelowy jest w tej samej domenie
                if (dstHostDomain == domainName)
                {
                    //to dzwonimy do hosta docelowego z pytaniem elo odbierz
                    Actions.CallAcceptRequest(ControlProtocol);
                }
                //Jeśli host docelowy jest w innej domenie
                else ()
                {
                    //To dzwonimy do NCC kumpla z tekstem elo znajdź mi hosta i spytaj czy odbierze
                    Actions.CallCoordinationRequest(ControlProtocol);
                }


                //Actions.ConnectionRequestRequest(ControlProtocol, devName, h1sender, h2dest, bandwidth, udp, ccEndPoint);
            }

            else if (ControlActionsReader.isCallAcceptResponse(ControlProtocol))
            {
                String data = ControlProtocol.Data;
                String[] dataString = data.Split('&');
                IPEndPoint h1address = Actions.DirectoryRequest(devName, dir, dataString[0]);
                IPEndPoint h2address = Actions.DirectoryRequest(devName, dir, dataString[1]);
                float bandwidth = float.Parse(dataString[2]);
                Actions.CallCoordinationResponse(h1address, h2address, bandwidth, udp, neighborNCCEndPoint);
            }
            //Odbieramy od NCC z innej domeny CallCoordinationResponse
            //Wysyłamy do CPCC CallRequestResponse 
            else if (ControlActionsReader.isCallCoordinationResponse(ControlProtocol))
            {
                String data = ControlProtocol.Data;
                String[] dataString = data.Split('&');
                IPEndPoint h1address = new IPEndPoint(IPAddress.Parse(dataString[0]), int.Parse(dataString[1]));
                IPEndPoint h2address = new IPEndPoint(IPAddress.Parse(dataString[2]), int.Parse(dataString[3]));
                float bandwidth = float.Parse(dataString[4]);
                //TODO: słownik uzyskujacy Nazwe hosta z IPENDpoina
                Actions.CallRequestResponse("", "", bandwidth, udp, h1address);
            }
            */
        }
    }
}
