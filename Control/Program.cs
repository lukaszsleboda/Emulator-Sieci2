using System;

namespace Control
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("[CC & RC & LRM  opened]");
            try
            {
                Control conn = new Control(args[0]);
            } catch(Exception e)
            {
                Console.WriteLine(e);
            } finally
            {
                while(true)
                {
                   
                    Console.ReadLine();
                }
            }
            
        }
    }
}
