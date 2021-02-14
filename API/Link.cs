using System;
using System.Collections.Generic;
using System.Text;

namespace API
{
    public class Link
    {
        public int id { get; set; }
        public Tuple<int,int> SNPs { get; set; }
        public float actual_bandwidth { get; set; }
        public float max_bandwidth { get; set; }
        public bool isalive { get; set; }
        public int length { get; set; }
        public List<int> usingLambdas { get; set; }

        public Link()
        {
            usingLambdas = new List<int>();
            SNPs = new Tuple<int, int>(0,0);
        }

        public Link(int id, int length)
        {
            this.id = id;
            this.length = length;
        }

        public Link(int id, int SNP1, int SNP2, float actual_bandwidth, float max_bandwidth, bool isalive, int length)
        {
            this.id = id;
            usingLambdas = new List<int>();
            this.SNPs = new Tuple<int, int>(SNP1, SNP2);
            this.actual_bandwidth = actual_bandwidth;
            this.max_bandwidth = max_bandwidth;
            this.isalive = isalive;
            this.length = length;

        }
    }
}
