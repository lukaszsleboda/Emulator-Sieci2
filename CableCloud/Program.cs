using System;

namespace CableCloud
{
    class Program
    {
        static void Main(string[] args)
        {

            
            Console.WriteLine(args[0]);
            Console.WriteLine("CableCloud app opened");

            CableCloud cableCloud = new CableCloud(args[0]);
            
            

            
        }
    }
}
