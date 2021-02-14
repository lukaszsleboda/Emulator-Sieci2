using System;
using API.Protocols.Reader;
namespace API.Protocols.ControlProtocol.Reader
{
    public class ControlProtocolReader
    {
        //obiekt typu CPCC, NCC itd.
        public String FromComponent { get; set; }
        //obiekt typu CPCC, NCC itd.
        public String ToComponent { get; set; }
        //nazwa zadania typu CallRequest itd.
        public String CommandType { get; set; }

        public bool TransitDomain { get; set; }
        public bool TransitSubnetwork { get; set; }

        public String Action { get; set; }

        private String id { get; set; }

        public String Data { get; set; }


        /// <summary>
        /// ProtocolType*ComponentType*CommandType*data
        /// </summary>
        /// <param name="data"></param>
        public ControlProtocolReader(String data)
        {

            String[] dataString = data.Split('*');
            this.id = dataString[0];
            this.FromComponent = dataString[1];
            this.ToComponent = dataString[2];
            this.CommandType = dataString[3];
            this.TransitDomain = TransitConverterToBool(dataString[4]);
            this.TransitSubnetwork = TransitConverterToBool(dataString[5]);
            this.Action = dataString[6];
            this.Data = dataString[7];
        }

        public ControlProtocolReader(ControlProtocolReader coder)
        {
            this.id = coder.id;
            this.FromComponent = coder.FromComponent;
            this.ToComponent = coder.ToComponent;
            this.CommandType = coder.CommandType;
            this.TransitDomain = coder.TransitDomain;
            this.TransitSubnetwork = coder.TransitSubnetwork;
            this.Action = coder.Action;
            this.Data = coder.Data;
        }


        public ControlProtocolReader()
        {
            this.TransitDomain = false;
            this.TransitSubnetwork = false;
            this.Action = StaticActions.ALLOCATE;
        }


        public static String CodeDataToString(String id, String FromComponent, String ToComponent, String CommandType, bool TransitDomain, bool TransitSubnetwork, String Action, String Data)
        {
            String prep = "";
            try
            {
                if (id is null || id == "")
                {
                    throw new ArgumentNullException(id, "You're trying to create TransportProtocol without GUID!!!");
                }
                prep = id + "*" + FromComponent + "*" + ToComponent + "*" + CommandType + "*" + TransitConverterToString(TransitDomain) + "*" + TransitConverterToString(TransitSubnetwork) + "*"+ Action + "*"+ Data;
            } catch (ArgumentNullException e)
            {
                Console.WriteLine(e);
            }
            return prep;
        }

        public static ControlProtocolReader EncodeDataFromString(String data)
        {
           

            ControlProtocolReader coder = new ControlProtocolReader();

            String[] dataString = data.Split('*');
            coder.id = dataString[0];
            coder.FromComponent = dataString[1];
            coder.ToComponent = dataString[2];
            coder.CommandType = dataString[3];
            coder.TransitDomain = TransitConverterToBool(dataString[4]);
            coder.TransitSubnetwork = TransitConverterToBool(dataString[5]);
            coder.Action = dataString[6];
            coder.Data = dataString[7];
            return coder;
        }

        public String ToStringWithProtocolType()
        {
            ProtocolReader protocol = new ProtocolReader();
            protocol.activateControlProtocol();


            protocol.setData(CodeDataToString(this.id, this.FromComponent, this.ToComponent,this.CommandType, this.TransitDomain, this.TransitSubnetwork, this.Action ,this.Data));
            
            //TYLKO INFORMACYJNIE
            //x
            return protocol.toString();
        }

        public void SetData(String data)
        {
            this.Data = data;
        }


        public bool isTransitSubnetwork()
        {
            if (TransitSubnetwork) { return true; }
            else { return false; }
        }

        public bool isTransitDomain()
        {
            if (TransitDomain) { return true; }
            else { return false; }
        }

        public void setTransitDomain(bool isTransit)
        {
            if (isTransit) { TransitDomain = true; }
            else { TransitDomain = false; }
        }

        public void setTransitSubnetwork(bool isTransit)
        {
            if (isTransit) { TransitSubnetwork = true; }
            else { TransitSubnetwork = false; }
        }



        //****PRIVATE METHODS****
        private static bool TransitConverterToBool(String element)
        {
            if (element == "T") { return true; }
            else { return false; }
        }

        private static String TransitConverterToString(bool element)
        {
            if(element) { return "T"; }
            else { return "F"; }
        }


        public void setID()
        {
            if(id == null)
            {
                id = (Guid.NewGuid().ToString());
            }
        }

        public void setID(String ID)
        {
            if(id == null)
            {
                this.id = ID;
            }    
        }

        public String ID()
        {
            return id;
        }
    }
}
