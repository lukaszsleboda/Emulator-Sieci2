using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace API
{
    public class SNP
    {
        public IPEndPoint endPoint { get; set;  }
        public int Interface { get; set; }

        public SNP()
        {

        }

        public SNP(SNP snp)
        {
            endPoint = snp.endPoint;
            Interface = snp.Interface;
        }

        public SNP(IPEndPoint endPoint, int Interface)
        {
            this.endPoint = endPoint;
            this.Interface = Interface;
        }
    }

}
