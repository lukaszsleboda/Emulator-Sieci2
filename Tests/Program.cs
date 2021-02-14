using System;
using System.Collections.Generic;
using System.Collections;

namespace Tests
{
    class Program
    {

        static void Main(string[] args)
        {
            ArrayListTest();
            //Console.WriteLine("Hello World!");
           // Tests test = new Tests();
            //test.randomLambdas(3);
        }








        public static void ArrayListTest()
        {
            ArrayList arlist = new ArrayList();
            arlist.Add(2);
            arlist.Add(19);
            arlist.Add(40);

            foreach(int element in arlist)
            {
                Console.WriteLine(element);
            }
            Console.WriteLine("***");

            arlist.RemoveAt(0);
            foreach (int element in arlist)
            {
                Console.ReadLine();
            }

        }





        public class Tests
        {
            public Dictionary<int, bool> available_lambdas { get; set; }

            public Tests()
            {
                available_lambdas = new Dictionary<int, bool>();
                for (int i = 1; i <= 10; i++)
                {
                    available_lambdas.Add(i, true);
                }

                available_lambdas[2] = false;
                available_lambdas[4] = false;
                available_lambdas[6] = false;




            }
            public void randomLambdas(int requirements)
            {
                Random rnd = new Random();
                bool finish = false;
                int first = 0;
                while (!finish)
                {
                    first = rnd.Next(1, 10 - requirements + 1);
                    Console.WriteLine($"WYLOSOWAŁEM {first}");
                    int counter = 0;
                    for (int i = first; i < first + requirements; i++)
                     {
                         if (available_lambdas[i])
                         {
                                counter++;
                         }
                         else
                        {
                            break;
                        }
                     }
                     if(counter == requirements)
                    {
                        finish = true;
                    }
                     else
                    {
                        counter = 0;
                    }
                }
                Console.WriteLine(first);
            }
        }
       
    }
}
