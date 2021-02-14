using System;
using System.Collections.Generic;
using API;
using API.Protocols.ControlProtocol.Reader;
using System.Linq;

namespace TestsPoligon
{
    class Program
    {
        public static int calculateNumberOfSlots(int bandwidth, int modulation)
        {
            int numberofslots = new int();
            int spectral_efficiency = 2 * bandwidth;
            //ponizej kod zaokragla do najblizszej calkowitej liczby
            int tmp = ((spectral_efficiency - 1) / modulation) + 1;
            //float tmp2 = (float)tmp;
            //numberofslots = (int)((float)tmp / 12.5);
            numberofslots = (int)((float)tmp / 12.5) + 1;
            return numberofslots;
        }
        static void Main(string[] args)
        {
            //int wynik = calculateNumberOfSlots(120, 4);
            //Console.WriteLine($"wynik to {wynik}");
            Test11();

        }

        public static void Test6()
        {
            string var = "H1D1";

            var = var[^2..];
            Console.WriteLine(var);
        }

        public static void Test5()
        {
            List<int> devices = new List<int>();
            devices.Add(2);
            devices.Add(5);
            devices.Add(7);
            devices.Add(9);
            List<Tuple<int, int>> list = new List<Tuple<int, int>>();

            int len = devices.Count;
            for (int i = 0; i < len - 1; i++)
            {
                for (int j = i + 1; j < len; j++)
                {
                    list.Add(new Tuple<int, int>(devices[i], devices[j]));
                }
            }
            foreach (Tuple<int, int> tuple in list)
            {
                Console.WriteLine($"({tuple.Item1},{tuple.Item2})");
            }

        }

        public static void Test4()
        {
            ControlProtocolReader coder = new ControlProtocolReader();
            coder.ToStringWithProtocolType();
        }

        public static void Test2()
        {
            Tuple<int, int> tuple;

            tuple = new Tuple<int, int>(1, 2);

            Console.WriteLine(tuple);

        }

        public static void Test3()
        {
            Tuple<int, int> tuple;
            Tuple<int, int> tuple2 = new Tuple<int, int>(2, 3);
            tuple = tuple2;

            Console.WriteLine(tuple);
        }

        public static void Test10()
        {
            Console.WriteLine("przed");
            Polygon polygon = new Polygon("Control3.json");
            PolygonRC.RC rc = new PolygonRC.RC();
            List<PolygonRC.RCRouter> routers = rc.ExtractRouters(10301, 30302, polygon);
            rc.DijkstraAlgorithm(routers, 0);
            Console.WriteLine("po");
        }

        public static void Test11()
        {
            Console.WriteLine("przed");
            Polygon polygon = new Polygon("Control3.json");
            Polygon1RC polygon1rc = new Polygon1RC();
            //polygon.LinksList[3].isalive = false;
            Console.WriteLine($"pierwszy router na liscie to: {polygon.NetworkDevicesList[0].Name}");
            //polygon1rc.DijkstraAlgorithm(polygon, 0, ip1: polygon.RCInTable.Keys.ToList()[0], ip2: polygon.RCInTable.Keys.ToList()[2]);
            polygon1rc.DijkstraAlgorithm(polygon, snp1: 50302, snp2: 60302);
            Console.WriteLine("po");
        }
        /*
        public void Test1()
        {
            Console.WriteLine("Hello World!");
            RCRouter r1 = new RCRouter("10.0.0.1");
            RCRouter r2 = new RCRouter("10.0.0.2");
            RCRouter r3 = new RCRouter("10.0.0.3");
            RCRouter r4 = new RCRouter("10.0.0.4");
            RCRouter r5 = new RCRouter("10.0.0.5");

            Link l1 = new Link(1, 10, 15, true, 1);
            Link l2 = new Link(2, 10, 15, true, 6);
            Link l3 = new Link(3, 10, 15, true, 2);
            Link l4 = new Link(4, 10, 15, true, 1);
            Link l5 = new Link(5, 10, 15, true, 2);
            Link l6 = new Link(6, 10, 15, true, 5);
            Link l7 = new Link(7, 10, 15, true, 5);


            r1.neighbours.Add(new Tuple<RCRouter, Link>(r2, l1));
            r1.neighbours.Add(new Tuple<RCRouter, Link>(r3, l2));
            r2.neighbours.Add(new Tuple<RCRouter, Link>(r1, l1));
            r2.neighbours.Add(new Tuple<RCRouter, Link>(r3, l3));
            r2.neighbours.Add(new Tuple<RCRouter, Link>(r4, l4));
            r3.neighbours.Add(new Tuple<RCRouter, Link>(r1, l2));
            r3.neighbours.Add(new Tuple<RCRouter, Link>(r2, l3));
            r3.neighbours.Add(new Tuple<RCRouter, Link>(r4, l5));
            r4.neighbours.Add(new Tuple<RCRouter, Link>(r2, l4));
            r4.neighbours.Add(new Tuple<RCRouter, Link>(r5, l7));
            r5.neighbours.Add(new Tuple<RCRouter, Link>(r3, l6));
            r5.neighbours.Add(new Tuple<RCRouter, Link>(r4, l7));

            List<RCRouter> routers = new List<RCRouter>();
            routers.Add(r1);
            routers.Add(r2);
            routers.Add(r3);
            routers.Add(r4);
            routers.Add(r5);
            RC rc = new RC();
            rc.DijkstraAlgorithm(routers, 0);
        }
        */
    }
}
