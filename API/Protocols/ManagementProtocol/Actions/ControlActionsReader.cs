using System;
using System.Collections.Generic;
using System.Text;
using API.Protocols.ControlProtocol.Reader;

namespace API.Protocols.ControlProtocol.Actions
{
    public class ControlActionsReader
    {
        public static bool isCallRequestRequest(ControlProtocolReader coder)
        {
            if (coder.CommandType == "CallRequestRequest") { return true; }
            else { return false; }
        }


        public static void setCallRequestRequest(ControlProtocolReader coder)
        {
            coder.CommandType = "CallRequestRequest";
        }

        public static bool isPeerCoordinationRequest(ControlProtocolReader coder)
        {
            if (coder.CommandType == "PeerCoordinationRequest") { return true; }
            else { return false; }
        }


        public static void setPeerCoordinationRequest(ControlProtocolReader coder)
        {
            coder.CommandType = "PeerCoordinationRequest";
        }
        public static bool isPeerCoordinationResponse(ControlProtocolReader coder)
        {
            if (coder.CommandType == "PeerCoordinationResponse") { return true; }
            else { return false; }
        }


        public static void setPeerCoordinationResponse(ControlProtocolReader coder)
        {
            coder.CommandType = "PeerCoordinationResponse";
        }


        public static bool isConnectionRequestRequest(ControlProtocolReader coder)
        {
            if (coder.CommandType == "ConnectionRequestRequest") { return true; }
            else { return false; }
        }


        public static void setConnectionRequestRequest(ControlProtocolReader coder)
        {
            coder.CommandType = "ConnectionRequestRequest";
        }

        public static bool isConnectionRequestResponse(ControlProtocolReader coder)
        {
            if (coder.CommandType == "ConnectionRequestResponse") { return true; }
            else { return false; }
        }

        public static void setCallRequestReponse(ControlProtocolReader coder)
        {
            coder.CommandType = "CallRequestReponse";
        }

        public static bool isCallRequestReponse(ControlProtocolReader coder)
        {
            if (coder.CommandType == "CallRequestReponse") { return true; }
            else { return false; }
        }


        public static void setConnectionRequestResponse(ControlProtocolReader coder)
        {
            coder.CommandType = "ConnectionRequestResponse";
        }

        public static bool isCallAcceptResponse(ControlProtocolReader coder)
        {
            if (coder.CommandType == "CallAcceptResponse") { return true; }
            else { return false; }
        }


        public static void setCallAcceptResponse(ControlProtocolReader coder)
        {
            coder.CommandType = "CallAcceptResponse";
        }

        

        public static bool isCallTeardown(ControlProtocolReader coder)
        {
            if (coder.CommandType == "CallTeardown") { return true; }
            else { return false; }
        }

        public static void setCallAcceptRequest(ControlProtocolReader coder)
        {
            coder.CommandType = "CallAcceptRequest";
        }



        public static bool isCallAcceptRequest(ControlProtocolReader coder)
        {
            if (coder.CommandType == "CallAcceptRequest") { return true; }
            else { return false; }
        }

        


        public static void setCallTeardown(ControlProtocolReader coder)
        {
            coder.CommandType = "CallTeardown";
        }

        public static bool isCallCoordinationResponse(ControlProtocolReader coder)
        {
            if (coder.CommandType == "CallCoordinationResponse") { return true; }
            else { return false; }
        }


        public static void setCallCoordinationResponse(ControlProtocolReader coder)
        {
            coder.CommandType = "CallCoordinationResponse";
        }

        public static bool isCallCoordinationRequest(ControlProtocolReader coder)
        {
            if (coder.CommandType == "CallCoordinationRequest") { return true; }
            else { return false; }
        }


        public static void setCallCoordinationRequest(ControlProtocolReader coder)
        {
            coder.CommandType = "CallCoordinationRequest";
        }

        public static bool isCallRequestResponse(ControlProtocolReader coder)
        {
            if (coder.CommandType == "CallRequestResponse") { return true; }
            else { return false; }
        }

        public static void setCallRequestReponse1(ControlProtocolReader coder)
        {
            coder.CommandType = "CallRequestResponse";

        }


        public static void setCallRequestResponse(ControlProtocolReader coder)
        {
            coder.CommandType = "CallRequestResponse";
        }

        public static bool isSNPNegotiation(ControlProtocolReader coder)
        {
            if (coder.CommandType == "SNPNegotiation") { return true; }
            else { return false; }
        }


        public static void setSNPNegotiation(ControlProtocolReader coder)
        {
            coder.CommandType = "SNPNegotiation";
        }


        public static bool isRouterUpdate(ControlProtocolReader coder)
        {
            if (coder.CommandType == "RouterUpdate") { return true; }
            else { return false; }
        }


        public static void setRouterUpdate1(ControlProtocolReader coder)
        {
            coder.CommandType = "RouterUpdate";
        }


    }
}
