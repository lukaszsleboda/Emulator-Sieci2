using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using API.Protocols.ControlProtocol.Components;
using API.Protocols.ControlProtocol.Reader;
using API.Protocols.Reader;
using API.Protocols.ControlProtocol.Actions;
using API.BytesCoder;
using API.LOGS;
using API;

namespace Host.Components
{
    public class CPCC
    {

        public String cpccName { get; set; }
        public IPEndPoint NCCendPoint { get; set; }
        public Dictionary<int,CPCCCache> connectionsList { get; set; } 
        //public List<CPCCCache> connectionsList { get; set; }
        public int counter { get; set; } 


        public CPCC()
        {
             connectionsList = new Dictionary<int,CPCCCache>();
            //connectionsList = new List<CPCCCache>();
             counter = 1;
        }

        /*
        /// <summary>
        /// CPCC wysyła do NCC. Prosi o stworzenie połączenia.
        /// </summary>
        /// <param name="H1"></param>
        /// <param name="H2"></param>
        /// <param name="bandwidth"></param>
        /// <param name="udp"></param>
        public void CallRequestRequest(String H1, String H2, float bandwidth, UdpClient udp)
        {
            byte[] test = null; //Tu wysyłamy nazwy hosta dzwoniącego i odbierającego

            udp.Send(test, test.Length, NCCendPoint);
        }
        */
        /*
        public void CallRequestResponse()
        {

        }
        */

        public void CallRequestRequest(String H1, String H2, int bandwidth, UdpClient udp, bool isDealocation = false, int cacheRecord = 0, String guid="0")
        {
            ControlProtocolReader MPR = new ControlProtocolReader();
            ControlComponentsReader.setFromCPCC(MPR);
            ControlComponentsReader.setToNCC(MPR);
            String data = H1 + "&" + H2 + "&" + bandwidth.ToString();
            MPR.SetData(data);
            ControlActionsReader.setCallRequestRequest(MPR);


            if (isDealocation == true && guid != "0")
            {
                MPR.setID(guid);
                connectionsList.Remove(cacheRecord);
                MPR.Action = StaticActions.DEALLOCATE;
            }
            else
            {
                MPR.setID();
                ControlActionsReader.setCallRequestRequest(MPR);
                CPCCCache cache = new CPCCCache();
                cache.guid = MPR.ID();
                cache.FromHost = H1;
                cache.ToHost = H2;
                cache.bandwitdh = bandwidth;
                connectionsList.Add(counter, cache);
                //counter++;
                
            }

            String message = MPR.ToStringWithProtocolType();
            byte[] message_bytes = ByteCoder.toBytes(message);

            udp.Send(message_bytes, message_bytes.Length, NCCendPoint);

            if (isDealocation && guid != null)
            {
                String messgage = $"<CallTeardownRequest> {H1} -> {H2} /{bandwidth}/Gb/s";
                Logs.ControlLOG(cpccName, messgage, Colors.CPCC);

            }
            else
            {
                String messgage = $"<CallRequestRequest> {H1} -> {H2} /{bandwidth}/Gb/s";
                Logs.ControlLOG(cpccName, messgage, Colors.CPCC);

            }



        }

        public void CallAcceptResponse(Host host,  ControlProtocolReader ControlProtocol)
        {
            String data = ControlProtocol.Data;
            String[] dataString = data.Split('&');

            String senderString = dataString[0];
            String dstString = dataString[1];
            String bandwidth = dataString[2];
            ControlProtocolReader sndProtocol = new ControlProtocolReader(ControlProtocol);
            ControlActionsReader.setCallAcceptResponse(sndProtocol);



            String message = sndProtocol.ToStringWithProtocolType();
            byte[] message_bytes = ByteCoder.toBytes(message);

            host.udp.Send(message_bytes, message_bytes.Length, NCCendPoint);


            if (ControlProtocol.Action == StaticActions.ALLOCATE)
            {
                String callCoordinationmessage = $"<CallAcceptResponse> {senderString} -> {dstString} /{bandwidth}/Gb/s";
                Logs.ControlLOG(cpccName, callCoordinationmessage, Colors.CPCC);
            }
            else if(ControlProtocol.Action == StaticActions.DEALLOCATE)
            {
                String callCoordinationmessage = $"<CallTeardownResponse> {senderString} -> {dstString} /{bandwidth}/Gb/s";
                Logs.ControlLOG(cpccName, callCoordinationmessage, Colors.CPCC);

            }



        }





    }
}
