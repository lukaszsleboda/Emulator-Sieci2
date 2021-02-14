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

namespace NCC.Actions
{
    public class RealActions
    {
        /// <summary>
        /// NCC pyta CPCC czy zaakceptuje połączenie
        /// </summary>
        /// <param name="ncc"></param>
        /// <param name="ControlProtocol"></param>
        public static void CallAcceptRequest(NCC ncc, ControlProtocolReader ControlProtocol)
        {
            String data = ControlProtocol.Data;
            String[] dataString = data.Split('&');

            String senderString = dataString[0];
            String dstString = dataString[1];
            String bandwidth = dataString[2];
            //IPEndPoint h1sender = DirectoryRequest(devName, dir, senderString);
            IPEndPoint h2dest = Actions.FakeActions.DirectoryRequest(ncc.devName, ncc.dir, dstString);

            String data2 = senderString + "&" + dstString + "&" + bandwidth.ToString();
            ControlProtocolReader sndProtocol = new ControlProtocolReader(ControlProtocol);
            sndProtocol.Data = data2;
            ControlActionsReader.setCallAcceptRequest(sndProtocol);

            String message = sndProtocol.ToStringWithProtocolType();
            byte[] message_bytes = ByteCoder.toBytes(message);


            ncc.udp.Send(message_bytes, message_bytes.Length, h2dest);


            if (sndProtocol.Action == StaticActions.ALLOCATE)
            {
                String callAcceptmessage = $"<CallAcceptRequest> {senderString} -> {dstString} /{bandwidth}/Gb/s";
                Logs.ControlLOG(ncc.devName, callAcceptmessage, Colors.NCC);

            }
            else if (sndProtocol.Action == StaticActions.DEALLOCATE)
            {
                String callAcceptmessage = $"<CallTeardownRequest> {senderString} -> {dstString} /{bandwidth}/Gb/s";
                Logs.ControlLOG(ncc.devName, callAcceptmessage, Colors.NCC);
            }

        }


        /// <summary>
        /// Dzwonię do NCC w innej domenie. Mówię mu - słuchaj typie, przedłuż to połączenie w swojej domenie,
        /// żeby Łukasz z Maćkiem mogli gadać
        /// </summary>
        /// <param name="id1">Host dzwoniący</param>
        /// <param name="id2">Host odbierający</param>
        public static void CallCoordinationRequest(NCC ncc, ControlProtocolReader ControlProtocol)
        {
            String data = ControlProtocol.Data;
            String[] dataString = data.Split('&');

            String senderString = dataString[0];
            String dstString = dataString[1];
            String bandwidth = dataString[2];
            //IPEndPoint h1sender = DirectoryRequest(devName, dir, senderString);
            IPEndPoint h2dest = Actions.FakeActions.DirectoryRequest(ncc.devName, ncc.dir, dstString);


            ControlProtocolReader sndProtocol = new ControlProtocolReader(ControlProtocol);
            ControlActionsReader.setCallCoordinationRequest(sndProtocol);

            String message = sndProtocol.ToStringWithProtocolType();
            byte[] message_bytes = ByteCoder.toBytes(message);

            ncc.udp.Send(message_bytes, message_bytes.Length, ncc.nccNeighbour.EndPoint);

            String callCoordinationmessage = $"<CallCoordinationRequest> {senderString} -> {dstString} /{bandwidth}/Gb/s";
            Logs.ControlLOG(ncc.devName, callCoordinationmessage, Colors.NCC);

        }


        /// <summary>
        /// NCC prosi CC o zajęcie zasobów i znalezienie ścieżki
        /// </summary>
        /// <param name="ncc"></param>
        /// <param name="ControlProtocol"></param>
        public static void ConnectionRequestRequest(NCC ncc, ControlProtocolReader ControlProtocol)
        {
            String data = ControlProtocol.Data;
            String[] dataString = data.Split('&');

            String senderString = dataString[0];
            String senderDomain = senderString[^2..];

            String dstString = dataString[1];
            String dstDomain = dstString[^2..];

            String bandwidth = dataString[2];


            IPEndPoint id1 = Actions.FakeActions.DirectoryRequest(ncc.devName, ncc.dir, senderString);
            IPEndPoint id2 = Actions.FakeActions.DirectoryRequest(ncc.devName, ncc.dir, dstString);

            ControlProtocolReader sndProtocol = new ControlProtocolReader(ControlProtocol);
            ControlActionsReader.setConnectionRequestRequest(sndProtocol);
            String data2 = id1.Address.ToString() + "&" + id1.Port.ToString() + "&" + senderDomain + "&" + id2.Address.ToString() + "&" + id2.Port.ToString() + "&" + dstDomain + "&" + bandwidth.ToString();

            sndProtocol.Data = data2;
            ControlComponentsReader.setFromNCC(sndProtocol);
            ControlComponentsReader.setToCC(sndProtocol);

            String message = sndProtocol.ToStringWithProtocolType();
            byte[] message_bytes = ByteCoder.toBytes(message);

            ncc.udp.Send(message_bytes, message_bytes.Length, ncc.ccEndPoint);

            String callRequestmessage= $"<ConnectionRequestRequest> {id1} -> {id2} /{bandwidth}/Gb/s";
            Logs.ControlLOG(ncc.devName, callRequestmessage, Colors.NCC);



        }


        /// <summary>
        /// NCC zwraca do NCC info, że znalazł hosta u siebie, host zaakceptował i elo, można zestawiać ścieżkę
        /// </summary>
        /// <param name="ncc"></param>
        /// <param name="ControlProtocol"></param>
        public static void CallCoordinationResponse(NCC ncc, ControlProtocolReader ControlProtocol)
        {
            String data = ControlProtocol.Data;
            String[] dataString = data.Split('&');

            String senderString = dataString[0];
            String dstString = dataString[1];
            String bandwidth = dataString[2];
            //IPEndPoint h1sender = DirectoryRequest(devName, dir, senderString);
            //IPEndPoint h2dest = Actions.FakeActions.DirectoryRequest(ncc.devName, ncc.dir, dstString);
            //String bandwidth = dataString[2];


            ControlProtocolReader sndProtocol = new ControlProtocolReader(ControlProtocol);
            ControlActionsReader.setCallCoordinationResponse(sndProtocol);

            String message = sndProtocol.ToStringWithProtocolType();
            byte[] message_bytes = ByteCoder.toBytes(message);

            ncc.udp.Send(message_bytes, message_bytes.Length, ncc.nccNeighbour.EndPoint);

            String callCoordinationmessage = $"<CallCoordinationResponse> {senderString} -> {dstString} /{bandwidth}/Gb/s";
            Logs.ControlLOG(ncc.devName, callCoordinationmessage, Colors.NCC);

        }


        /// <summary>
        /// NCC dowiedziało się od CC że droga zestawiona. Odsyła do CPCC info że skonczone
        /// </summary>
        /// <param name="ncc"></param>
        /// <param name="ControlProtocol"></param>
        public static void CallRequestReponse(NCC ncc, ControlProtocolReader ControlProtocol)
        {
            String data = ControlProtocol.Data;
            String[] dataString = data.Split('&');


            IPEndPoint h1EndPoint = new IPEndPoint(IPAddress.Parse(dataString[0]), int.Parse(dataString[1]));
            IPEndPoint h2EndPoint = new IPEndPoint(IPAddress.Parse(dataString[3]), int.Parse(dataString[4]));
            String bandwidth = dataString[6];
            String req_lmb = dataString[7];
            String frs_lmb = dataString[8];

            
            String indicatorMessage = $"<ConnectionRequestResponse> {h1EndPoint}-> {h2EndPoint} /{bandwidth}/Gb/s";
            Logs.ControlLOG(ncc.devName, indicatorMessage, Colors.NCC);

            ControlProtocolReader sndProtocol = new ControlProtocolReader(ControlProtocol);
            ControlActionsReader.setCallRequestReponse1(sndProtocol);

            String h1String = Actions.FakeActions.DirectoryRequest(ncc.devName, ncc.dir, h1EndPoint);
            String h2String = Actions.FakeActions.DirectoryRequest(ncc.devName, ncc.dir, h2EndPoint);

            sndProtocol.Data = $"{h1String}&{h2String}&{bandwidth}&{req_lmb}&{frs_lmb}";

            String message = sndProtocol.ToStringWithProtocolType();
            byte[] message_bytes = ByteCoder.toBytes(message);

            ncc.udp.Send(message_bytes, message_bytes.Length, h1EndPoint);


            

            if(sndProtocol.Action == StaticActions.ALLOCATE)
            {
                String sendermesage = $"<CallRequestResponse> {h1String}-> {h2String} /{bandwidth}/Gb/s";
                Logs.ControlLOG(ncc.devName, sendermesage, Colors.NCC);

            }
            else if(sndProtocol.Action == StaticActions.DEALLOCATE)
            {
                String sendermesage = $"<CallTeardownResponse> {h1String}-> {h2String} /{bandwidth}/Gb/s";
                Logs.ControlLOG(ncc.devName, sendermesage, Colors.NCC);
            }

        }

        /*
        /// <summary>
        /// Dzwonię do CC z rządaniem o zestawienie połączenia pomiędzy H1 i H2
        /// </summary>
        /// <param name="id1">Host dzwoniący</param>
        /// <param name="id2">Host odbierający</param>
        public static void ConnectionRequestRequest(ControlProtocolReader protocol, String devName, IPEndPoint id1, IPEndPoint id2, float bandwidth, UdpClient udp, IPEndPoint ccEndPoint)
        {
            ControlProtocolReader MPR = new ControlProtocolReader();
            ControlComponentsReader.setFromNCC(MPR);
            ControlComponentsReader.setToCC(MPR);
            ControlActionsReader.setConnectionRequestRequest(MPR);
            String data = id1.Address.ToString() + "&" + id1.Port.ToString() + "&" + id2.Address.ToString() + "&" + id2.Port.ToString() + "&" + bandwidth.ToString();
            MPR.SetData(data);
            MPR.setID(protocol.ID());
            String message = MPR.ToStringWithProtocolType();
            byte[] message_bytes = ByteCoder.toBytes(message);

            udp.Send(message_bytes, message_bytes.Length, ccEndPoint);

            String logmessage = $"<ConnectionRequestRequest> {id1} -> {id2} /{bandwidth}/Gb/s";
            Logs.ControlLOG(devName, logmessage, Colors.NCC);
        }

        public static void CallRequestResponse(String id1, String id2, float bandwidth, UdpClient udp, IPEndPoint cpccEndPoint)
        {
            byte[] test = null; //Tu wysyłamy Id1 i Id2

            ControlProtocolReader MPR = new ControlProtocolReader();
            ControlComponentsReader.setFromNCC(MPR);
            ControlComponentsReader.setToCC(MPR);
            ControlActionsReader.setConnectionRequestRequest(MPR);

            String data = id1 + "&" + id2 + "&" + bandwidth.ToString();
            MPR.SetData(data);
            String message = MPR.ToStringWithProtocolType();
            byte[] message_bytes = ByteCoder.toBytes(message);

            udp.Send(test, test.Length, cpccEndPoint);
        }


        public static void CallCoordinationResponse(IPEndPoint id1, IPEndPoint id2, float bandwidth, UdpClient udp, IPEndPoint nccEndPoint)
        {
            ControlProtocolReader MPR = new ControlProtocolReader();
            ControlComponentsReader.setFromNCC(MPR);
            ControlComponentsReader.setToNCC(MPR);
            ControlActionsReader.setCallCoordinationResponse(MPR);
            String data = id1.Address.ToString() + "&" + id1.Port.ToString() + "&" + id2.Address.ToString() + "&" + id2.Port.ToString() + "&" + bandwidth.ToString();
            MPR.SetData(data);
            String message = MPR.ToStringWithProtocolType();
            byte[] message_bytes = ByteCoder.toBytes(message);

            udp.Send(message_bytes, message_bytes.Length, nccEndPoint);
        }



        /// <summary>
        /// Dzwonię do Hosta. Sluchaj, kumpel do Ciebie dzwoni. CHcesz odebrać?
        /// </summary>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        public void CallAcceptRequest(String id1, String id2, float bandwidth, UdpClient udp, IPEndPoint IPid2)
        {
            ControlProtocolReader MPR = new ControlProtocolReader();
            ControlComponentsReader.setFromNCC(MPR);
            ControlComponentsReader.setToCPCC(MPR);
            ControlActionsReader.setCallAcceptRequest(MPR);
            String data = id1 + "&" + id2 + "&" + bandwidth.ToString();
            MPR.SetData(data);
            String message = MPR.ToStringWithProtocolType();
            byte[] message_bytes = ByteCoder.toBytes(message);

            udp.Send(message_bytes, message_bytes.Length, IPid2);

        }


        /// <summary>
        /// Decyzja hosta, czy chce z innym hostem gadać, czy ma iść na bambus
        /// </summary>
        public void CallResponse()
        {

        }
        */
    }
}
