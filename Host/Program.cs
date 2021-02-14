using System;

namespace Host
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("[HOST  opened]");
            try
            {
                Host conn = new Host(args[0]);
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
