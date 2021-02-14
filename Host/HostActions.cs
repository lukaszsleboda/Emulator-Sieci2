using System;
using System.Net;
using System.Net.Sockets;
using API.LOGS;
using API.Protocols.Reader;
using API.Protocols.TransportProtocol;
using API.BytesCoder;
namespace Host
{
    public class HostActions
    {
        public HostActions()
        {
        }

        public static void SendPacket(String sender, String dest, String bandwidth, String required_lmbd, String first_lbd, String Data, UdpClient udp, IPEndPoint senderHostEndPoint, IPEndPoint cloudEndPoint)
        {

            TransportProtocolReader transportProtocol = new TransportProtocolReader();
            transportProtocol.SenderName = sender;
            transportProtocol.DestName = dest;
            transportProtocol.bandwidth = int.Parse(bandwidth);
            transportProtocol.required_lambdas = int.Parse(required_lmbd);
            transportProtocol.firstLambda = int.Parse(first_lbd);
            transportProtocol.Data = Data;
            transportProtocol.Port = senderHostEndPoint.Port;

            String messageToSend = transportProtocol.ToStringWithProtocolType();
            byte[] message_bytes = ByteCoder.toBytes(messageToSend);

            udp.Send(message_bytes, message_bytes.Length, cloudEndPoint);

            String logsMessage = $"[PACKET SEND] {sender} -> {dest} of {bandwidth}Gb/s [required_lambdas:{transportProtocol.required_lambdas} starts_at: {transportProtocol.firstLambda}]: {Data}";
            Logs.TransportLOG(sender, logsMessage, Colors.HOST);


        }
    }
}
