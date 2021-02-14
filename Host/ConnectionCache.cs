using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Host
{
    public class ConnectionCache
    {
        String SenderName { get; set; }
        String DestName { get; set; }
        int bandwidth { get; set; }
        int required_lambdas { get; set; }
        int firstLambda { get; set; }
        IPEndPoint senderHostEndPoint { get; set; }

        public ConnectionCache(String SenderName, String DestName, int bandwidth, int required_lambdas, int firstLambda)
        {
            this.SenderName = SenderName;
            this.DestName = DestName;
            this.bandwidth = bandwidth;
            this.required_lambdas = required_lambdas;
            this.firstLambda = firstLambda;
        }


    }
}
