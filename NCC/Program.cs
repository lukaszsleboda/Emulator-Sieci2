using System;

namespace NCC
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("[NCC  opened]");
            try
            {
                NCC conn = new NCC(args[0]);
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
