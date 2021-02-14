using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using API;
using API.LOGS;
using Control;
using API.Protocols.ControlProtocol.Reader;

namespace Control.Components.RouteControler
{

    public class RC
    {
        public Dictionary<Tuple<int,int>, int[]> RCRoutes { get; set; } 
        public String devName { get; set; }

        public RC()
        {
            /*
            RCRoutes = new Dictionary<Tuple<int,int>, int[]>();
            RCRoutes.Add(new Tuple<int, int>(10301,30302), new int[] { 10111, 10301, 10302, 30301, 30302, 50302 });
            RCRoutes.Add(new Tuple<int, int>(50302, 80302), new int[] { 30302, 50302, 60302, 80301, 80302, 10113 });
            RCRoutes.Add(new Tuple<int, int>(50302, 60302), new int[] { 30302, 50302, 50301, 40302, 40304, 60301,60302,80301 });
            RCRoutes.Add(new Tuple<int, int>(10301, 20301), new int[] { 10111, 10301, 10302, 30301, 30303, 20302,20301,10112 });

            //Od H3 do H1
            RCRoutes.Add(new Tuple<int, int>(80302, 50302), new int[] {10113, 80302, 80301, 60302, 50302, 30302});
            RCRoutes.Add(new Tuple<int, int>(60302, 50302), new int[] {80301,60302,60301,40304,40302,50301,50302, 30302});
            RCRoutes.Add(new Tuple<int, int>(30302, 10301), new int[] {50302,30302,30301,10302,10301,10111});
            */

        }



        public void RouteTableQueryResponse(Control control, ControlProtocolReader controlProtocol, int snp11= -1, int snp22 = -1 )
        {
            String data = controlProtocol.Data;


            
            //{
                int snp1 = 0;
                int snp2 = 0;
                String[] dataString = data.Split('&');
                IPEndPoint h1 = new IPEndPoint(IPAddress.Parse(dataString[0]), int.Parse(dataString[1]));
                IPEndPoint h2 = new IPEndPoint(IPAddress.Parse(dataString[3]), int.Parse(dataString[4]));
                String bandwidth = dataString[6];

                String indicatorLog = $"<RouteTableQueryIndicator> {h1} -> {h2} {bandwidth}";
                Logs.ControlLOG(devName, indicatorLog, Colors.RC);

                if(snp11 == -1 && snp22 == -1)
                {
                    snp1 = control.RCInTable[h1];
                    snp2 = control.RCInTable[h2];
                }
                else {
                    snp1 = snp11;
                    snp2 = snp22;

                }

            //int[] SNPsList = FindPath(snp1, snp2);
            int[] SNPsList = FindPath(control, snp1, snp2);

            Cache cache = control.CacheDic[controlProtocol.ID()];

                foreach (int i in SNPsList)
                {
                    cache.SNPList.Add(i);
                }

                foreach (int snp in cache.SNPList)
                {
                    cache.SNPListTMP.Add(snp);
                }
                cache.SNPListTMP.RemoveAt(cache.SNPListTMP.Count-1);
                cache.SNPListTMP.RemoveAt(0);


                if (control.CacheDic[controlProtocol.ID()].lambdas.Count == 0)
                {
                //control.CacheDic[controlProtocol.ID()].lambdas_requirement = lambdas_requirements(cache);
                //control.CacheDic[controlProtocol.ID()].lambdas_requirement = calculateModulation(control.distances[new Tuple<IPAddress, IPAddress>(cache.FromEndPoint.Address, cache.DstEndPoint.Address)]);
                control.CacheDic[controlProtocol.ID()].lambdas_requirement = calculateNumberOfSlots(cache.bandwidth, calculateModulation(control.distances[new Tuple<IPAddress, IPAddress>(cache.FromEndPoint.Address, cache.DstEndPoint.Address)]));
               // Console.WriteLine($"wyliczona lambda: {control.CacheDic[controlProtocol.ID()].lambdas_requirement}");
            }


                String sendLog = $"<RouteTableQueryResponse> {h1} -> {h2} {bandwidth}";
                Logs.ControlLOG(devName, sendLog, Colors.RC);
           // }
        }


        //public int[] FindPath(int snp1, int snp2)
        public int[] FindPath(Control control, int snp1, int snp2)
        {
            //Console.WriteLine($"SNPPoints: {snp1} {snp2}");
            //Console.WriteLine("FIND PATH");
            //foreach (KeyValuePair<Tuple<int, int>, int[]> entry in RCRoutes)
            //{
            //    Console.WriteLine($"{entry.Key.Item1.ToString()}  {entry.Key.Item2.ToString()}");
            //}
            //Console.WriteLine("FIND PATH");
            //int[] x = RCRoutes[new Tuple<int, int>(snp1, snp2)];
            //Console.WriteLine("tmp1" + snp1 + "  " + snp2);
            int[] x = DijkstraAlgorithm(control, snp1, snp2);
           // foreach(int e in x)
            //{
           //    Console.WriteLine(e);
            //}
            return x;
        }

        //TODO OSZUSTWO!!!!
        public int lambdas_requirements(Cache cache)
        {
            if (cache.ToDomain == cache.FromDomain)
            {
                return 2;
            }
            else
            {
                return 3;
            }
            
        }
        //TODO OSZUSTWO!!!!
        public class NeighbourList
        {
            public List<Tuple<NetworkDevice, Link>> neighbours { get; set; }
            public NeighbourList(List<Tuple<NetworkDevice, Link>> neighbours)
            {
                this.neighbours = neighbours;
            }
        }
        public class DijkstraList
        {
            public NetworkDevice vertex { get; set; }
            public int distance { get; set; }
            public NetworkDevice predecessor { get; set; }
            public bool visited { get; set; }
            public DijkstraList(NetworkDevice vertex, int distance, NetworkDevice predecessor, bool visited)
            {
                this.vertex = vertex;
                this.distance = distance;
                this.predecessor = predecessor;
                this.visited = visited;
            }

        }

        public bool checkLinkState(Link link)
        {
            if (link.isalive)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Link findLink(Control polygon, int snp)
        {
            Link tmplink = new Link();
            foreach (Link link in polygon.LinksList)
            {
                if ((snp == link.SNPs.Item1) || (snp == link.SNPs.Item2))
                {
                    tmplink = link;
                    break;
                }
            }
            return tmplink;
        }

        //funkcja sluzy do znalezienia wszystkich snp nalezacych do jednego routera
        public List<int> findSNPs(Control polygon, NetworkDevice vertex)
        {
            String Name = vertex.Name;
            List<int> snps = new List<int>();
            foreach (NetworkDevice element in polygon.NetworkDevicesList)
            {
                if (element.Name == Name)
                {
                    snps.Add(element.SNPs.Item1);
                    snps.Add(element.SNPs.Item2);
                }
            }
            List<int> newsnps = snps.Distinct().ToList();
            return newsnps;
        }

        //funkcja sluzy do znalezienia wszystkich obiektow Link, ktore sa powiazana z snp znajdujacymi sie w liscie
        public List<Tuple<int, Link>> findLinks(Control polygon, List<int> snps)
        {
            List<Tuple<int, Link>> links = new List<Tuple<int, Link>>();
            foreach (int snp in snps)
            {
                foreach (Link link in polygon.LinksList)
                {
                    //jesli snp sa te same, to dodaj listy snp po przeciwnej stronie linku, by moc potem znalezc odpowiednie routery
                    if (link.SNPs.Item1 == snp)
                    {
                        Tuple<int, Link> krotka = new Tuple<int, Link>(link.SNPs.Item2, link);
                        links.Add(krotka);
                    }
                    else if (link.SNPs.Item2 == snp)
                    {
                        Tuple<int, Link> krotka = new Tuple<int, Link>(link.SNPs.Item1, link);
                        links.Add(krotka);
                    }
                }
            }
            return links;
        }

        public List<Tuple<NetworkDevice, Link>> getNeighbours(Control polygon, List<Tuple<int, Link>> links)
        {
            List<Tuple<NetworkDevice, Link>> neighbours = new List<Tuple<NetworkDevice, Link>>();
            foreach (Tuple<int, Link> element in links)
            {
                if (checkLinkState(element.Item2))
                {
                    foreach (NetworkDevice device in polygon.NetworkDevicesList)
                    {
                        if ((element.Item1 == device.SNPs.Item1) || (element.Item1 == device.SNPs.Item2))
                        {
                            Tuple<NetworkDevice, Link> neighbour = new Tuple<NetworkDevice, Link>(device, element.Item2);
                            neighbours.Add(neighbour);
                        }
                    }
                }

            }
            List<Tuple<NetworkDevice, Link>> newneighbours = neighbours.GroupBy(x => x.Item1.Name).Select(y => y.First()).ToList();
            return newneighbours;
        }

        public List<Tuple<NetworkDevice, NeighbourList>> getNeighboursForAllDevices(Control polygon)
        {
            List<NetworkDevice> routers = polygon.NetworkDevicesList.GroupBy(x => x.Name).Select(y => y.First()).ToList();
            List<Tuple<NetworkDevice, NeighbourList>> newrouters = new List<Tuple<NetworkDevice, NeighbourList>>();
            foreach (NetworkDevice router in routers)
            {
                List<int> snps = new List<int>();
                snps = findSNPs(polygon, router);
                List<Tuple<int, Link>> links = findLinks(polygon, snps);
                List<Tuple<NetworkDevice, Link>> neighbours = getNeighbours(polygon, links);
                NeighbourList neighbourlist = new NeighbourList(neighbours);
                Tuple<NetworkDevice, NeighbourList> newrouter = new Tuple<NetworkDevice, NeighbourList>(router, neighbourlist);
                newrouters.Add(newrouter);
            }
            return newrouters;
        }

        //slownik jest podawany, aby mozna bylo sprawdzic, ktore z sasiadow aktualnie przeszukiwanego
        //routera nie byly jeszcze odwiedzane;
        //vertex jest podawany, aby bylo wiadomo ktory router obecnie jest przeszukiwany
        public void ExamineUnvisited(ref Dictionary<String, DijkstraList> dlist, Tuple<NetworkDevice, NeighbourList> router)
        {
            NetworkDevice vertex = router.Item1;
            int currentdistance = dlist[vertex.Name].distance;

            foreach (Tuple<NetworkDevice, Link> element in router.Item2.neighbours)
            {
                String ip = element.Item1.Name;
                int linklength = element.Item2.length;
                DijkstraList dijkstralist = dlist[ip];
                if (!dijkstralist.visited)
                {
                    int calculateddistance = currentdistance + linklength;
                    if (calculateddistance < dijkstralist.distance)
                    {
                        dijkstralist.distance = calculateddistance;
                        dijkstralist.predecessor = vertex;
                    }
                }

            }

            dlist[vertex.Name].visited = true;
        }

        public List<int> getRoute(int snp1, int snp2, Control polygon, Dictionary<String, DijkstraList> vertices, NetworkDevice desiredrouter)
        {
            List<NetworkDevice> route = new List<NetworkDevice>();
            route.Add(desiredrouter);
            DijkstraList endvertex = vertices[desiredrouter.Name];
            //Console.WriteLine($"router o nazwie: {desiredrouter.Name}");
            while (endvertex.predecessor != null)
            {
                route.Add(endvertex.predecessor);
                //Console.WriteLine($"router o nazwie: {endvertex.predecessor.Name}");
                endvertex = vertices[endvertex.predecessor.Name];

            }

            List<int> snproute = new List<int>();
            List<Link> linksroute = new List<Link>();

            Link link1 = findLink(polygon, snp1);
            Link link2 = findLink(polygon, snp2);

            if (link2.SNPs.Item1 == snp2)
            {
                snproute.Add(link2.SNPs.Item2);
                snproute.Add(link2.SNPs.Item1);
            }
            else
            {
                snproute.Add(link2.SNPs.Item1);
                snproute.Add(link2.SNPs.Item2);
            }

            for (int i = 0; i < route.Count - 1; i++)
            {
                List<Link> linkids1 = new List<Link>();
                List<Link> linkids2 = new List<Link>();
                NetworkDevice router1 = route[i];
                NetworkDevice router2 = route[i + 1];
                List<int> snps1 = findSNPs(polygon, router1);
                List<int> snps2 = findSNPs(polygon, router2);
                List<Tuple<int, Link>> links1 = findLinks(polygon, snps1);
                List<Tuple<int, Link>> links2 = findLinks(polygon, snps2);
                foreach (Tuple<int, Link> element in links1)
                {
                    linkids1.Add(element.Item2);
                }
                foreach (Tuple<int, Link> element in links2)
                {
                    linkids2.Add(element.Item2);
                }
                Link mutuallink = linkids1.Intersect(linkids2).First();
                linksroute.Add(mutuallink);
                bool kolejnosc = false;
                foreach (int snp in snps1)
                {
                    if (snp == mutuallink.SNPs.Item1)
                    {
                        kolejnosc = true;
                        break;
                    }
                }

                if (kolejnosc)
                {
                    snproute.Add(mutuallink.SNPs.Item1);
                    snproute.Add(mutuallink.SNPs.Item2);
                }
                else
                {
                    snproute.Add(mutuallink.SNPs.Item2);
                    snproute.Add(mutuallink.SNPs.Item1);
                }


            }

            if (link1.SNPs.Item1 == snp1)
            {
                snproute.Add(link1.SNPs.Item1);
                snproute.Add(link1.SNPs.Item2);
            }
            else
            {
                snproute.Add(link1.SNPs.Item2);
                snproute.Add(link1.SNPs.Item1);
            }

            snproute.Reverse();

            /*foreach (int snp in snproute)
            {
                Console.WriteLine($"snp: {snp}");
            }*/
            return snproute;

            //tutaj potrzebna informacja, ktore lambdy na ktorym laczu sa zajete
        }


        //snp1 to snp wejsciowe, snp2 to snp wyjsciowe

        public int resolveIP(Control polygon, IPEndPoint ip)
        {
            int snp = polygon.RCInTable[ip];
            return snp;
        }

        public int[] DijkstraAlgorithm(Control polygon, int snp1 = -1, int snp2 = -1, IPEndPoint ip1 = null, IPEndPoint ip2 = null)
        {
            if ((snp1 == -1) && (snp2 == -1))
            {
                snp1 = resolveIP(polygon, ip1);
                snp2 = resolveIP(polygon, ip2);
            }
            //List<NetworkDevice> routers = polygon.NetworkDevicesList;
            //slownik zawiera pary [nazwa urzadzenia, wiersz do tabelki dijkstry]
            Dictionary<String, DijkstraList> vertices = new Dictionary<string, DijkstraList>();

            List<Tuple<NetworkDevice, NeighbourList>> newrouters = getNeighboursForAllDevices(polygon);
            int len1 = newrouters.Count();

            for (int i = 0; i < len1; i++)
            {
                if ((newrouters[i].Item1.SNPs.Item1 == snp1) || (newrouters[i].Item1.SNPs.Item2 == snp1))
                {
                    DijkstraList row = new DijkstraList(newrouters[i].Item1, 0, null, false);
                    if (!vertices.ContainsKey(newrouters[i].Item1.Name))
                    {
                        vertices.Add(newrouters[i].Item1.Name, row);
                    }

                }
                else
                {
                    DijkstraList row = new DijkstraList(newrouters[i].Item1, 100000, null, false);
                    if (!vertices.ContainsKey(newrouters[i].Item1.Name))
                    {
                        vertices.Add(newrouters[i].Item1.Name, row);
                    }

                }
            }

            //wykonuj dla kazdego routera
            for (int i = 0; i < len1; i++)
            {
                int distancevalue = 100001;
                int index = -1;
                int bestindex = -1;

                //znajdz jeszcze nie odwiedzony router o najmniejszym dystansie od wierzchołka startowego
                foreach (KeyValuePair<String, DijkstraList> element in vertices)
                {
                    index += 1;
                    if ((element.Value.distance < distancevalue) & (element.Value.visited == false))
                    {
                        distancevalue = element.Value.distance;
                        bestindex = index;

                    }
                }
                ExamineUnvisited(ref vertices, newrouters[bestindex]);

            }
            NetworkDevice endrouter = newrouters[0].Item1;
            foreach (Tuple<NetworkDevice, NeighbourList> element in newrouters)
            {
                if ((element.Item1.SNPs.Item1 == snp2) || (element.Item1.SNPs.Item2 == snp2))
                {
                    endrouter = element.Item1;
                }
            }
            //getRoute(snp1, snp2, polygon, vertices, newrouters[2].Item1);
            List<int> snproute = getRoute(snp1, snp2, polygon, vertices, endrouter);
            return snproute.ToArray();
        }

        public int calculateModulation(int length)
        {
            //POTRZEBNE JEST WYLICZENIE DROGI NA POCZATKU PROGRAMU
            int modulation = new int();
            if(length < 100)
            {
                modulation = 6;
            }
            else if(length < 200)
            {
                modulation = 5;
            }
            else if (length < 300)
            {
                modulation = 4;
            }
            else if (length < 400)
            {
                modulation = 3;
            }
            else if (length < 500)
            {
                modulation = 2;
            }
            else if (length < 600)
            {
                modulation = 1;
            }
            return modulation;
        }

        public static int calculateNumberOfSlots(int bandwidth, int modulation)
        {
            int numberofslots = new int();
            int spectral_efficiency = 2 * bandwidth;
            //ponizej kod zaokragla do najblizszej calkowitej liczby
            int tmp = ((spectral_efficiency - 1) / modulation) + 1;
            numberofslots = (int)((float)tmp / 12.5) + 1;
            return numberofslots;
        }

        public int findAvailableSlotRange(List<Link> links, int numberofslots)
        {
            List<int> availableslots = Enumerable.Range(1, 90).ToList();
            foreach(Link link in links)
            {
                availableslots = availableslots.Except(link.usingLambdas).ToList();
            }

            if(availableslots.Count >= numberofslots)
            {
                
                for (int i = 0; i < availableslots.Count - numberofslots; i++)
                {
                    List<int> sequence = new List<int>();
                    for (int j = i; j < i + numberofslots; j++)
                    {
                        sequence.Add(availableslots[j]);
                    }
                    bool isSequential = Enumerable.Range(sequence.Min(), sequence.Count())
                              .SequenceEqual(sequence);
                    if(isSequential)
                    {
                        return i;
                    }
                }
                
            }
            return -1;
        }

        //zwraca dlugosc drogi miedzy para snp
        public int getShortestPathLength(int snp1, int snp2, Control polygon, Dictionary<String, DijkstraList> vertices, NetworkDevice desiredrouter)
        {
            int length = 0;
            List<NetworkDevice> route = new List<NetworkDevice>();
            route.Add(desiredrouter);
            DijkstraList endvertex = vertices[desiredrouter.Name];
            //Console.WriteLine($"router o nazwie: {desiredrouter.Name}");
            //while (endvertex.predecessor != null)
            {
                route.Add(endvertex.predecessor);
               // Console.WriteLine($"router o nazwie: {endvertex.predecessor.Name}");
                endvertex = vertices[endvertex.predecessor.Name];

            }

            Link link1 = findLink(polygon, snp1);
            Link link2 = findLink(polygon, snp2);

            length += link1.length;
            length += link2.length;

            for (int i = 0; i < route.Count - 1; i++)
            {
                List<Link> linkids1 = new List<Link>();
                List<Link> linkids2 = new List<Link>();
                NetworkDevice router1 = route[i];
                NetworkDevice router2 = route[i + 1];
                List<int> snps1 = findSNPs(polygon, router1);
                List<int> snps2 = findSNPs(polygon, router2);
                List<Tuple<int, Link>> links1 = findLinks(polygon, snps1);
                List<Tuple<int, Link>> links2 = findLinks(polygon, snps2);
                foreach (Tuple<int, Link> element in links1)
                {
                    linkids1.Add(element.Item2);
                }
                foreach (Tuple<int, Link> element in links2)
                {
                    linkids2.Add(element.Item2);
                }
                Link mutuallink = linkids1.Intersect(linkids2).First();
                length += mutuallink.length;
            }

            return length;
        }

    }
}
