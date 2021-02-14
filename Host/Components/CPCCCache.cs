using System;
using API;
namespace Host.Components
{
    public class CPCCCache
    {
        public String guid { get; set; }
        public String FromHost { get; set; }
        public String ToHost { get; set; }
        public int bandwitdh { get; set; }
        public int req_lambdas { get; set; }
        public int first_lambda { get; set; }
       
        public CPCCCache()
        {

        }
    }
}
