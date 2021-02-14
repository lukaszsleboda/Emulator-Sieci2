using System;
using API.Protocols.ControlProtocol.Reader;
using API.LOGS;
using System.Net;
using NCC.Components;
namespace NCC.Actions
{
    public class FakeActions
    {
        public static String DirectoryRequest(String devName, Directory dir, IPEndPoint endPoint)
        {

            String message1 = $"<DirectoryRequestRequest>: {endPoint}";
            Logs.ControlLOG(devName, message1, Colors.NCC);

            String dest = dir.getNameByIP(endPoint);

            String message2 = $"<DirectoryRequestConfirm>: {endPoint} -> {dest}";
            Logs.ControlLOG(devName, message2, Colors.NCC);

            return dest;
        }

        /// <summary>
        /// Przychodzi CallRequest od Hosta. Tłumaczymy nazwę hosta na adres IP
        /// </summary>
        /// <param name="name">Nazwa Hosta docelowego</param>
        /// <returns>IPEndPoint hosta docelowego</returns>
        public static IPEndPoint DirectoryRequest(String devName, Directory dir, String name)
        {
            String message1 = $"<DirectoryRequestRequest>: {name}";
            Logs.ControlLOG(devName, message1, Colors.NCC);

            IPEndPoint dest = dir.getIPByName(name);

            String message2 = $"<DirectoryRequestConfirm>: {name} -> {dest}";
            Logs.ControlLOG(devName, message2, Colors.NCC);

            return dest;
        }


        /// <summary>
        /// Udaje, że coś robię. Sprawdzam, czy host dzwoniący zapłacił za neta
        /// </summary>
        public static void Policy(ControlProtocolReader ControlProtocol, String devName)
        {
            String data = ControlProtocol.Data;
            String[] dataString = data.Split('&');

            String senderString = dataString[0];

            String messageNCC = $"<PolicyRequest>: {senderString}";
            Logs.ControlLOG(devName, messageNCC, Colors.NCC);

            String policyMessage = $"<PolicyIndicator>: {senderString}";
            Logs.ControlLOG("POLICY", policyMessage, Colors.POLICY);

            String policyMessageR = $"<PolicyResponse>: {senderString} VALID OK";
            Logs.ControlLOG("POLICY", policyMessageR, Colors.POLICY);

            String messageNCCR = $"<PolicyConfirm>: {senderString} VALID OK";
            Logs.ControlLOG(devName, messageNCCR, Colors.NCC);
        }






        public static bool isInMyDomain(ControlProtocolReader ControlProtocol, bool DstHost, String domainName)
        {
            String data = ControlProtocol.Data;
            String[] dataString = data.Split('&');

            String senderString = dataString[0];
            String dstString = dataString[1];

            if(DstHost)
            {
                //Console.WriteLine($"MY DOMAIN: {domainName} **************   hostDmomain: {dstString[^2..]}");
                if(domainName == dstString[^2..]) 
                {
                 //   Console.WriteLine("MY DOMAIN!!!!!!!");
                    return true; 
                }
                else 
                {
                //    Console.WriteLine("NOT MY DOMAIN!!!!!!");
                    return false; 
                }
            }
            else
            {
              //  Console.WriteLine($"MY DOMAIN: {domainName} **************   hostDmomain: {senderString[^2..]}");

                if (domainName == senderString[^2..]) 
                {
                  //  Console.WriteLine("MY DOMAIN!!!!!!!");
                    return true; 
                }
                else 
                {
                   // Console.WriteLine("NOT MY DOMAIN!!!!!!");
                    return false; 
                }
            }
        }

    }
}
