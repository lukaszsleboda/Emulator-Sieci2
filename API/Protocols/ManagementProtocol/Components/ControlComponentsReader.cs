using System;
using API.Protocols.ControlProtocol.Reader;
namespace API.Protocols.ControlProtocol.Components
{
    public class ControlComponentsReader
    {
        //******************************
        //CPCC
        //******************************
        public static bool isToCPCC(ControlProtocolReader coder)
        {
            if(coder.ToComponent == "CPCC") { return true; }
            else { return false; }
        }

        public static bool isFromCPCC(ControlProtocolReader coder)
        {
            if (coder.FromComponent == "CPCC") { return true; }
            else { return false; }
        }


        public static void setToCPCC(ControlProtocolReader coder)
        {
            coder.ToComponent = "CPCC";
        }

        public static void setFromCPCC(ControlProtocolReader coder)
        {
            coder.FromComponent = "CPCC";
        }

        //******************************
        //NCC
        //******************************
        public static bool isFromNCC(ControlProtocolReader coder)
        {
            if (coder.ToComponent == "NCC") { return true; }
            else { return false; }
        }

        public static bool isToNCC(ControlProtocolReader coder)
        {
            if (coder.FromComponent == "NCC") { return true; }
            else { return false; }
        }

        public static void setToNCC(ControlProtocolReader coder)
        {
            coder.ToComponent = "NCC";
        }

        public static void setFromNCC(ControlProtocolReader coder)
        {
            coder.FromComponent = "NCC";
        }


        //******************************
        //CC
        //******************************
        public static bool isToCC(ControlProtocolReader coder)
        {
            if (coder.ToComponent == "CC") { return true; }
            else { return false; }
        }

        public static bool isFromCC(ControlProtocolReader coder)
        {
            if (coder.FromComponent == "CC") { return true; }
            else { return false; }
        }

        public static void setToCC(ControlProtocolReader coder)
        {
            coder.ToComponent = "CC";
        }

        public static void setFromCC(ControlProtocolReader coder)
        {
            coder.FromComponent = "CC";
        }


        //******************************
        //RC
        //******************************
        public static bool isFromRC(ControlProtocolReader coder)
        {
            if (coder.ToComponent == "RC") { return true; }
            else { return false; }
        }

        public static bool isToRC(ControlProtocolReader coder)
        {
            if (coder.FromComponent == "RC") { return true; }
            else { return false; }
        }

        public static void setToRC(ControlProtocolReader coder)
        {
            coder.ToComponent = "RC";
        }

        public static void setFromRC(ControlProtocolReader coder)
        {
            coder.FromComponent = "RC";
        }

        //******************************
        //LRM
        //******************************
        public static bool isFromLRM(ControlProtocolReader coder)
        {
            if (coder.ToComponent == "LRM") { return true; }
            else { return false; }
        }

        public static bool isToLRM(ControlProtocolReader coder)
        {
            if (coder.FromComponent == "LRM") { return true; }
            else { return false; }
        }

        public static void setToLRM(ControlProtocolReader coder)
        {
            coder.ToComponent = "LRM";
        }

        public static void setFromLRM(ControlProtocolReader coder)
        {
            coder.FromComponent = "LRM";
        }
    }
}
