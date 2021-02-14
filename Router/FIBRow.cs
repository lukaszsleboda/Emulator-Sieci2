using System;
using System.Collections.Generic;
using System.Text;

namespace Router
{
    public class FIBRow
    {
        public int PortIn { get; set; }
        public int PortOut { get; set; }
        public int ReqLmbd { get; set; }
        public int firstLambda { get; set; }
        public FIBRow(int PortIn, int PortOut, int ReqLmbd, int firstLambda)
        {
            this.PortIn = PortIn;
            this.ReqLmbd = ReqLmbd;
            this.PortOut = PortOut;
            this.firstLambda = firstLambda;

        }

    }
}
