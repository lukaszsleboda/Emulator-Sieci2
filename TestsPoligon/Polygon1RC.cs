using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using API;

namespace TestsPoligon
{
    class Polygon1RC
    {
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

        public Link findLink(Polygon polygon, int snp)
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
        public List<int> findSNPs(Polygon polygon, NetworkDevice vertex)
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
        public List<Tuple<int, Link>> findLinks(Polygon polygon, List<int> snps)
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

        public List<Tuple<NetworkDevice, Link>> getNeighbours(Polygon polygon, List<Tuple<int, Link>> links)
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

        public List<Tuple<NetworkDevice, NeighbourList>> getNeighboursForAllDevices(Polygon polygon)
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

        public List<int> getRoute(int snp1, int snp2, Polygon polygon, Dictionary<String, DijkstraList> vertices, NetworkDevice desiredrouter)
        {
            List<NetworkDevice> route = new List<NetworkDevice>();
            route.Add(desiredrouter);
            DijkstraList endvertex = vertices[desiredrouter.Name];
            Console.WriteLine($"router o nazwie: {desiredrouter.Name}");
            while (endvertex.predecessor != null)
            {
                route.Add(endvertex.predecessor);
                Console.WriteLine($"router o nazwie: {endvertex.predecessor.Name}");
                endvertex = vertices[endvertex.predecessor.Name];

            }

            List<int> snproute = new List<int>();

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

            foreach (int snp in snproute)
            {
                Console.WriteLine($"snp: {snp}");
            }
            return snproute;
        }

        //snp1 to snp wejsciowe, snp2 to snp wyjsciowe

        public int resolveIP(Polygon polygon, IPEndPoint ip)
        {
            int snp = polygon.RCInTable[ip];
            return snp;
        }

        public int[] DijkstraAlgorithm(Polygon polygon, int snp1 = -1, int snp2 = -1, IPEndPoint ip1 = null, IPEndPoint ip2 = null)
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

    }
}
