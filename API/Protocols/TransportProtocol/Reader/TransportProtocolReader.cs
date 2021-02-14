using System;
using API.Protocols.Reader;
namespace API.Protocols.TransportProtocol

{
    public class TransportProtocolReader
    {
        public String SenderName { get; set; }
        public String DestName { get; set; }
 
        /// <summary>
        /// Jest to port wirtualny w sieci.
        /// Np. Host nadaje wiadomość ze swoim portem
        /// Chmura odczytuje skąd przyszlo, sprawdza drugi koniec kabla
        ///     a następnie ustawia port na wyjściu kabla
        /// Router sprawdza na jaki port przyszedł pakiet, zmienia na port wyjściowy
        /// </summary>
        public int Port { get; set; }

        public int firstLambda { get; set; }
        public int required_lambdas { get; set; }

        public float bandwidth { get; set; }

        public String Data { get; set; }


        public TransportProtocolReader()
        {

        }

        

        public static String CodeDataToString(String sender, String dest, int Port, int requiredLbd, int firstLmb,  float bandwidth, String Data)
        {
            
            String prep = sender + "*" + dest + "*" + Port.ToString() + "*" + requiredLbd.ToString() + "*" + firstLmb.ToString() + "*" + bandwidth.ToString() + "*" + Data;
            return prep;
        }

        public static TransportProtocolReader EncodeDataFromString(String data)
        {
            TransportProtocolReader coder = new TransportProtocolReader();

            String[] dataString = data.Split('*');
            coder.SenderName = dataString[0];
            coder.DestName = dataString[1];
            coder.Port = int.Parse(dataString[2]);
            coder.required_lambdas = int.Parse(dataString[3]);
            coder.firstLambda = int.Parse(dataString[4]);
            coder.bandwidth = float.Parse(dataString[5]);
            coder.Data = dataString[6];

            return coder;
        }

        public String ToStringWithProtocolType()
        {
            ProtocolReader protocol = new ProtocolReader();
            protocol.activateTransportProtocol();
            
            protocol.setData(CodeDataToString(this.SenderName, this.DestName, this.Port,this.required_lambdas, this.firstLambda,this.bandwidth, this.Data));
          
            return protocol.toString();
        }



    }
}
