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
    public class NccLogs
    {


        public static void IndicatorLog(NCC ncc, ControlProtocolReader controlProtocol, String messageType)
        {
            String data = controlProtocol.Data;
            String[] dataString = data.Split('&');

            String senderString = dataString[0];
            String dstString = dataString[1];

            String bandwidth = dataString[2];


            String callCoordinationtMessage = $"{messageType} {senderString} -> {dstString} /{bandwidth}/Gb/s";
            Logs.ControlLOG(ncc.devName, callCoordinationtMessage, Colors.NCC);

        }


    }
}
