using System;

namespace Router
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("[ROUTER OPENED]");
            try
            {
                Router router = new Router(args[0]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                while (true)
                {
                    
                 
                    Console.ReadLine();
                }
            }
        }
    }
}
