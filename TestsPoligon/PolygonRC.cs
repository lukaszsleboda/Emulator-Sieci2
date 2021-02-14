using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using API;

namespace TestsPoligon
{
    class PolygonRC
    {
        public class RCRouter
        {
            //ip to String uzyskany z przekonwartowania adresu ip routera, np "192.168.0.12"
            public String ip { get; set; }
            public List<Tuple<RCRouter, Link>> neighbours { get; set; }
            public RCRouter()
            {
                neighbours = new List<Tuple<RCRouter, Link>>();
            }

            public RCRouter(String ipaddr)
            {
                neighbours = new List<Tuple<RCRouter, Link>>();
                ip = ipaddr;
            }

        }

        public class DijkstraList
        {
            public String routerid { get; set; }
            public int distance { get; set; }
            public RCRouter predecessor { get; set; }
            public bool visited { get; set; }
            /// <summary>
            /// </summary>
            /// <param name="dist">aktualny dystans od wezla poczatkowego</param>
            /// <param name="pred">wartosc poprzedniego wezla, ktory powinnismy wybrac,
            /// jesli chcemy dojsc najmniejszym kosztem do aktualnego,</param>
            /// <param name="visit">flaga okreslajaca, czy dany wezel zostal juz 
            /// sprawdzony</param>
            public DijkstraList(String rid, int dist, RCRouter pred, bool visit)
            {
                routerid = rid;
                distance = dist;
                predecessor = pred;
                visited = visit;
            }
        }
        public class RC
        {
            public List<Link> links { get; set; }

            public RC()
            {
                //RCConfigReader.LoadConfig(this, "nazwapliku");
            }

            public void RouteTableQueryIndicator()
            {

            }

            public List<API.SNP> RouteTableQueryResponse()
            {
                List<API.SNP> SNPPList = null;
                return SNPPList;
            }

            public void LocalTopologyConfirm(List<SNP> list)
            {
                Console.WriteLine("potwierdzam, to pyton");
            }

            public void ExamineUnvisited(ref Dictionary<String, DijkstraList> dlist, RCRouter vertex)
            {
                int currentdistance = dlist[vertex.ip].distance;

                foreach (Tuple<RCRouter, Link> element in vertex.neighbours)
                {
                    String ip = element.Item1.ip;
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

                dlist[vertex.ip].visited = true;
            }

            public void getRoute(Dictionary<String, DijkstraList> vertices, RCRouter desiredrouter)
            {
                List<RCRouter> route = new List<RCRouter>();
                route.Add(desiredrouter);
                DijkstraList endvertex = vertices[desiredrouter.ip];
                Console.WriteLine("przystanek3");
                while (endvertex.predecessor != null)
                {
                    route.Add(endvertex.predecessor);
                    endvertex = vertices[endvertex.predecessor.ip];
                }

                List<Link> links = new List<Link>();
                for (int i = 0; i < route.Count - 1; i++)
                {
                    RCRouter currentrouter = route[i];
                    String ipkolejnego = route[i + 1].ip;
                    foreach (Tuple<RCRouter, Link> element in currentrouter.neighbours)
                    {
                        if (element.Item1.ip == ipkolejnego)
                        {
                            links.Add(element.Item2);
                            continue;
                        }
                    }
                }

                foreach (RCRouter element in route)
                {
                    Console.WriteLine($"sciezka przez router{element.ip}");
                }
                foreach (Link element in links)
                {
                    Console.WriteLine($"sciezka przez link{element.id}");
                }
            }

            public String zwrocIP(List<PolygonReader.NetworkDeviceModel> devices, int snp)
            {
                String wynik = "";
                foreach(PolygonReader.NetworkDeviceModel element in devices)
                {
                    foreach(int numer in element.SNPs)
                    {
                        if(numer == snp)
                        {
                            wynik = element.IP;
                        }
                    }
                }
                return wynik;
            }

            //public List<RCRouter> ExtractRouters(int snp1, int snp2, ConConfigReader.CCModel ccmodel, ConConfigReader.LRMModel lrmmodel)
            public List<RCRouter> ExtractRouters(int snp1, int snp2, Polygon control)
            {
                List<NetworkDevice> ccmodel = control.NetworkDevicesList;
                List<Link> lrmmodel = control.LinksList;
                List<PolygonReader.NetworkDeviceModel> devices = control.devices;

                List<RCRouter> routers = new List<RCRouter>();
                List<RCRouter> edgerouters = new List<RCRouter>();
                List<PolygonReader.NetworkDeviceModel> edges = new List<PolygonReader.NetworkDeviceModel>();
                //zrob sztuczke
                //znalazles juz dwa routery graniczne, musisz tylko
                //dopisac kod ktory znajdzie sasiadow tych routerow
                int snp1_opposite = -1;
                int snp2_opposite = -1;
                foreach (Link element in lrmmodel)
                {
                    if (snp1 == element.SNPs.Item1)
                    {
                        snp1_opposite = element.SNPs.Item2;
                    }
                    else if (snp1 == element.SNPs.Item2)
                    {
                        snp1_opposite = element.SNPs.Item1;
                    }
                    else if (snp2 == element.SNPs.Item1)
                    {
                        snp2_opposite = element.SNPs.Item2;
                    }
                    else if (snp2 == element.SNPs.Item2)
                    {
                        snp2_opposite = element.SNPs.Item1;
                    }
                }

                /*
                foreach (PolygonReader.NetworkDeviceModel element in devices)
                {
                    for (int i = 0; i < element.SNPs.Count; i++)
                    {
                        if (element.SNPs[i] == snp1_opposite)
                        {
                            edges.Add(element);
                            //RCRouter edgerouter1 = new RCRouter(element.IP);
                            //routers.Add(edgerouter1);
                        }
                        else if (element.SNPs[i] == snp2_opposite)
                        {
                            edges.Add(element);
                            //RCRouter edgerouter2 = new RCRouter(element.IP);
                            //routers.Add(edgerouter2);
                        }
                    }
                }*/
                
                bool czy_znalezione = false;
                while(!czy_znalezione)
                {
                    foreach(Link element in lrmmodel)
                    {
                        if (element.SNPs.Item1 == snp1 || element.SNPs.Item2 == snp1)
                        {
                                RCRouter edgerouter1 = new RCRouter("fejk1");

                                //Tuple<RCRouter, Link>
                                RCRouter routerneighbor = new RCRouter(zwrocIP(devices, snp1));

                                Tuple<RCRouter, Link> neighbor = new Tuple<RCRouter, Link>(routerneighbor, element);

                                edgerouter1.neighbours.Add(neighbor);
                                edgerouters.Add(edgerouter1);
                                czy_znalezione = true;
                                break;
                        }
                    }
                }

                czy_znalezione = false;
                while (!czy_znalezione)
                {
                    foreach (Link element in lrmmodel)
                    {
                        if (element.SNPs.Item1 == snp2 || element.SNPs.Item2 == snp2)
                        {
                                RCRouter edgerouter2 = new RCRouter("fejk2");

                                //Tuple<RCRouter, Link>
                                RCRouter routerneighbor = new RCRouter(zwrocIP(devices, snp2));

                                Tuple<RCRouter, Link> neighbor = new Tuple<RCRouter, Link>(routerneighbor, element);

                                edgerouter2.neighbours.Add(neighbor);
                                edgerouters.Add(edgerouter2);
                                czy_znalezione = true;
                                break;
                        }
                    }
                }

                //edgerouters = findNeighbors(edges, lrmmodel);

                List<PolygonReader.NetworkDeviceModel> networkdevices = devices;
                routers = findNeighbors(networkdevices, lrmmodel);
                routers.Add(edgerouters[0]);
                routers.Add(edgerouters[1]);
                return routers;
            }

            public List<RCRouter> findNeighbors(List<PolygonReader.NetworkDeviceModel> networkdevices, List<Link> lrmmodel)
            {
                List<Link> links = lrmmodel;

                List<RCRouter> routers = new List<RCRouter>();

                //dla kazdego routera w podsieci/domenie znajdz sasiadow
                for (int i = 0; i < networkdevices.Count; i++)
                {
                    RCRouter router = new RCRouter(networkdevices[i].IP);
                    List<Tuple<RCRouter, Link>> neighbors = new List<Tuple<RCRouter, Link>>();
                    //lista wszystkich linkow polaczonych z danym routerem
                    List<int> routerlinks = networkdevices[i].Links;

                    int length = -1;
                    //znajdz router sasiadujacy dla kazdego linku
                    for (int j = 0; j < routerlinks.Count; j++)
                    {
                        //sprawdz kazdy router w poszukiwaniu linku
                        foreach (PolygonReader.NetworkDeviceModel element in networkdevices)
                        {
                            if (element.IP != networkdevices[i].IP)
                            {
                                //porownaj kazdy link w danym routerze
                                for (int z = 0; z < element.Links.Count; z++)
                                {
                                    length = -1;
                                    if (element.Links[z] == routerlinks[j])
                                    {
                                        RCRouter neighbor_router = new RCRouter(element.IP);
                                        foreach (Link linkelement in links)
                                        {
                                            if (linkelement.id == element.Links[z])
                                            {
                                                length = linkelement.length;
                                            }

                                        }
                                        Link link = new Link(z, length);
                                        Tuple<RCRouter, Link> neighbor = new Tuple<RCRouter, Link>(neighbor_router, link);
                                        neighbors.Add(neighbor);
                                    }


                                }
                            }

                        }
                    }

                    router.neighbours = neighbors;
                    routers.Add(router);
                    //routers[i] = router;

                }
                return routers;
            }

            public void DijkstraAlgorithm(List<RCRouter> routers, int startvertex)
            {
                //List<DijkstraList> vertices = new List<DijkstraList>();
                Dictionary<String, DijkstraList> vertices = new Dictionary<string, DijkstraList>();
                int len = routers.Count;
                for (int i = 0; i < len; i++)
                {
                    if (i == startvertex)
                    {
                        DijkstraList row = new DijkstraList(routers[i].ip, 0, null, false);
                        vertices.Add(routers[i].ip, row);
                    }
                    else
                    {
                        DijkstraList row = new DijkstraList(routers[i].ip, 100000, null, false);
                        vertices.Add(routers[i].ip, row);
                    }


                }

                Console.WriteLine("przystanek1");

                for (int i = 0; i < len; i++)
                {
                    int distancevalue = 100001;
                    int index = -1;
                    int bestindex = -1;
                    foreach (KeyValuePair<String, DijkstraList> element in vertices)
                    {
                        index += 1;
                        if ((element.Value.distance < distancevalue) & (element.Value.visited == false))
                        {
                            distancevalue = element.Value.distance;
                            bestindex = index;

                        }
                    }
                    ExamineUnvisited(ref vertices, routers[bestindex]);
                    Console.WriteLine("przystanek2");

                }
                getRoute(vertices, routers[4]);
            }


        }
    }
}
