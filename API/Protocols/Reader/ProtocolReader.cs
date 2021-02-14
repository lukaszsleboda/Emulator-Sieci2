using System;
using System.Collections.Generic;
using API.BytesCoder;

namespace API.Protocols.Reader
{
    public class ProtocolReader
    {
        public String ProtocolType { get; set; }
        public String Data { get; set; }

        public ProtocolReader() { }

        public ProtocolReader(byte[] data)
        {
            String stringData = ByteCoder.fromBytes(data);
            String[] dataString = stringData.Split('!');
            this.ProtocolType = dataString[0];
            this.Data = dataString[1];
        }

        public String toString()
        {
            String data = this.ProtocolType + "!" + this.Data;

            return data;
        }

        public static ProtocolReader fromString(String dataString)
        {
            String[] data = dataString.Split('!');
            ProtocolReader protocol = new ProtocolReader();
            protocol.ProtocolType = data[0];
            protocol.Data = data[1];

            return protocol;
        }


        public void activateTransportProtocol()
        {
            this.ProtocolType = "TRANSPORT";
        }

        public void activateControlProtocol()
        {
             this.ProtocolType =  "Control";
        }

        public void setData(String data)
        {
            this.Data = data;
        }

       

        public bool isControlProtocl()
        {
            if (this.ProtocolType == "Control") { return true; }
            else { return false; }
        }

        public bool isTransportProtocol()
        {
            if (this.ProtocolType == "TRANSPORT") { return true; }
            else { return false; }
        }

    }
}
