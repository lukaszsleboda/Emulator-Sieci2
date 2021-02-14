using System;
using System.Net;
using System.Collections.Generic;
using System.Collections;

namespace API
{
    public class Cache
    {
        public IPEndPoint FromEndPoint { get; set; }
        public String FromDomain { get; set; }
        public IPEndPoint DstEndPoint { get; set; }

        public int bandwidth { get; set; }
        public String ToDomain { get; set; }
        public List<int> lambdas { get; set; }
        public int[] SNPList { get; set; }
        public ArrayList SNPListTMP { get; set; }
        public String tempAct { get; set; }

        public int lambdas_requirement {get; set;} 
        

        public Cache()
        {
            lambdas = new List<int>();
            SNPListTMP = new ArrayList();
            SNPList = new int[] { };
        }

    }
}
