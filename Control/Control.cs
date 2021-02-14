using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Control.Components.CallControler;
using Control.Components.RouteControler;
using Control.Components.LinkResourceManager;
using API.Protocols.Reader;
using API.Protocols.ControlProtocol.Actions;
using API.Protocols.ControlProtocol.Components;
using API.Protocols.ControlProtocol.Reader;
using API.BytesCoder;
using API.LOGS;
using API;

namespace Control
{
    public class Control
    {
        public UdpClient udp;

        public IPEndPoint ControlEndPoint { get; set; }
        public String devName { get; set; }
        public String DomainName { get; set; }

        public CC cc { get; set; }
        public RC rc { get; set; }
        public LRM lrm { get; set; }

        public IPEndPoint NCCpoint { get; set; }

        public List<Link> LinksList { get; set; }
        public List<NetworkDevice> NetworkDevicesList { get; set; }
        public Dictionary<IPEndPoint, int> RCInTable { get; set; }
        //odleglosci pomiedzy poszczegolnymi hostami
        public Dictionary<Tuple<IPAddress, IPAddress>, int> distances { get; set; }

        public Dictionary<String, Cache> CacheDic { get; set; }

        public Dictionary<String, IPEndPoint> Controls { get; set; }

        public Dictionary<int, bool> available_lambdas { get; set; }
                
        public Control(String filename)
        {

            ConConfigReader.LoadConfig(this, filename);
            Console.WriteLine($"IP:{ControlEndPoint}");
            CacheDic = new Dictionary<string, Cache>();
            available_lambdas = new Dictionary<int, bool>();
            for (int i=1; i<=90; i++)
            {
                available_lambdas.Add(i, true);
            }
            


            String lrmNEgotiation = $"<NetworkTopology>";
            if (devName == "ControlD1")
            {
                Logs.ControlLOG(rc.devName, lrmNEgotiation, Colors.RC);
                System.Threading.Thread.Sleep(500);
                lrmNEgotiation += "GOT";
                Logs.ControlLOG(rc.devName, lrmNEgotiation, Colors.RC);

            }
            else if (devName == "ControlD2")
            {
                Logs.ControlLOG(rc.devName, lrmNEgotiation, Colors.RC);
                System.Threading.Thread.Sleep(500);
                lrmNEgotiation += "GOT";
                Logs.ControlLOG(rc.devName, lrmNEgotiation, Colors.RC);
            }


            asyncStart();
        }

        public void asyncStart()
        {
            udp = new UdpClient(ControlEndPoint);

            listener();
            while (true)
            {
                var znak = Console.ReadLine(); 
                if(znak == "1")
                {
                    printCache();
                }
                if(znak == "10")
                {
                    printLinks();
                    var link = Console.ReadLine();
                    if(link == "6" || link == "7" || link == "8" || link == "9" || link == "10")
                    {
                        reconfigureConnection(link);
                    }
                }
                
            }
        }

        public void listener()
        {
            Task.Run(async () =>
            {
                using (var updClient = udp)
                {
                    while (true)
                    {

                        var result = await updClient.ReceiveAsync();

                        byte[] resultBytes = result.Buffer;
                        ProtocolReader protocol = ProtocolReader.fromString(ByteCoder.fromBytes(resultBytes));
                       
                        if (protocol.isControlProtocl())
                        {
                            ControlProtocolReader controlProtocol = ControlProtocolReader.EncodeDataFromString(protocol.Data);
                            if (ControlComponentsReader.isToCC(controlProtocol))
                            {
                                if(controlProtocol.isTransitSubnetwork())
                                {
                                    cc.ccComandsSubnetwork(this, controlProtocol);
                                }
                                else
                                {
                                    cc.ccCommands(this, controlProtocol);
                                }

                            }
                            else if(ControlComponentsReader.isToLRM(controlProtocol))
                            {
                                lrm.lrmComands(this, controlProtocol);
                            }
                            /*if (ControlActionsReader.isConnectionRequestRequest(ControlProtocol))
                            {
                                String data = ControlProtocol.Data;
                                String[] dataString = data.Split('&');
                                IPEndPoint h1address = new IPEndPoint(IPAddress.Parse(dataString[0]), int.Parse(dataString[1]));
                                IPEndPoint h2address = new IPEndPoint(IPAddress.Parse(dataString[2]), int.Parse(dataString[3]));
                                float bandwidth = float.Parse(dataString[4]);


                                cc.RouteTableQuery(h1address, h2address, bandwidth);
                                rc.RouteTableQueryIndicator();
                                rc.RouteTableQueryResponse();
                                cc.RouteTableQueryConfirm();
                            }*/

                        }


                        //Czy wiadomość do CC
                     /*   //Czy wiadomość ConnectionRequest od NCC domeny
                        if (true)
                            {
                                //dekodujemy result
                                IPEndPoint h1 = null;
                                IPEndPoint h2 = null;
                                float bandwidth = 0;
                                cc.RouteTableQuery();
                                rc.RouteTableQueryIndicator();




                                List < SNP > SNPPList = rc.RouteTableQueryResponse(h1, h2, bandwidth);
                                cc.RouteTableQueryConfirm(); 
                                cc.LinkConnectionRequest(bandwidth);
                                bool isallocated = lrm.LinkConnectionRequestResponse();
                                cc.LinkConnectionRequestConfirm(isallocated);
                                lrm.LocalTopology();
                                rc.LocalTopologyConfirm(SNPPList);
                                

                            }*/



                        //Czy wiadomość do RC




                        //Czy wiadomość do LRM

                    }
                }
            });
        }


        public void printCache()
        {
            Console.WriteLine("*********************CACHE*************************");
            Console.WriteLine("GUID | FromEndPoint | FromDomain | DstEndPoint | ToDomain | bandwidth | SNPs list | Lambdas | tempAct ");

            foreach (KeyValuePair<String, Cache> entry in CacheDic)
            {
                String SNPs = "";
                foreach(int element in entry.Value.SNPList)
                {
                    SNPs += element.ToString() + ", ";
                }
                String lambdas = "";
                foreach (int element in entry.Value.lambdas)
                {
                    lambdas += element.ToString() + ", ";
                }
                Console.WriteLine($"{entry.Key}: | {entry.Value.FromEndPoint} | {entry.Value.FromDomain} | {entry.Value.DstEndPoint} | {entry.Value.ToDomain} | {entry.Value.bandwidth} | {SNPs} | {lambdas} | {entry.Value.tempAct}  ");
            }
            Console.WriteLine("**********************************************");

        }


        public void printLinks()
        {
            Console.WriteLine("*********************LINKS*************************");
            Console.WriteLine($"id porzadkowe | id | SNP1-SNP2 | actual_bandwidth | max_bandwidth | isAlive | length | lambdas");

            int licznik = 0;
            foreach (Link link in LinksList)
            {

                if (link.usingLambdas.Count == 0)
                {
                    
                    Console.WriteLine($"{licznik} | {link.id} | {link.SNPs.Item1}-{link.SNPs.Item2} | {link.actual_bandwidth} | {link.max_bandwidth} | {link.isalive} | - ");
                    licznik += 1;
                }
                else
                {
                    String lambdas = "";

                    foreach (int lambda in link.usingLambdas)
                    {
                        lambdas += $"{lambda}, ";
                    }
                    Console.WriteLine($"{licznik} | {link.id} | {link.SNPs.Item1}-{link.SNPs.Item2} | {link.actual_bandwidth} | {link.max_bandwidth} | {link.isalive} | {lambdas} ");
                    licznik += 1;
                }
            }
             Console.WriteLine("**********************************************");
        }

        public void printNetworkDevices()
        {
            Console.WriteLine("*********************DEVICES*************************");
            Console.WriteLine($" SNP1-SNP2 | Name | DevType | endPoint");

            foreach (NetworkDevice device in NetworkDevicesList)
            {
                Console.WriteLine($"{device.SNPs.Item1}-{device.SNPs.Item2} | {device.Name} | {device.DeviceType} | {device.EndPoint}");
            }
            Console.WriteLine("**********************************************");

        }

        public void allocateRouter(Cache cache, NetworkDevice device, int snp1, int snp2)
        {
           
            //String dataToRouter = $"{cache.SNPListTMP[0]}&{cache.SNPListTMP[1]}&{cache.lambdas_requirement.ToString()}&{cache.lambdas[0]}";
            String dataToRouter = $"{snp1}&{snp2}&{cache.lambdas_requirement.ToString()}&{cache.lambdas[0]}";
            ControlProtocolReader routerUpdate = new ControlProtocolReader();
            routerUpdate.setID();
            routerUpdate.Action = StaticActions.ALLOCATE;
            routerUpdate.SetData(dataToRouter);
            ControlActionsReader.setRouterUpdate1(routerUpdate);

            String message = routerUpdate.ToStringWithProtocolType();

            byte[] message_bytes = ByteCoder.toBytes(message);
            
            //Console.WriteLine("ENDPOINT:" + device.EndPoint);
            IPAddress ip = device.EndPoint.Address;
            int port = device.EndPoint.Port;
            udp.Send(message_bytes, message_bytes.Length, new IPEndPoint(ip,port));
            //System.Threading.Thread.Sleep(2000);
            String RouterUpdateTable = $"<Router {device.Name} Table Updated>";
            Logs.ControlLOG(cc.Name, RouterUpdateTable, Colors.CC);

            if(cache.SNPListTMP.Count != 0)
            {
                cache.SNPListTMP.Remove(snp1);
                cache.SNPListTMP.Remove(snp2);
            }
            
            
        }
        public void deallocateRouter(Cache cache, NetworkDevice device, int snp1, int snp2)
        {
            String dataToRouter = $"{cache.SNPList[0]}&{cache.SNPList[1]}&{cache.lambdas_requirement.ToString()}&{cache.lambdas[0]}";
            ControlProtocolReader routerUpdate = new ControlProtocolReader();
            routerUpdate.setID();
            routerUpdate.Action = StaticActions.DEALLOCATE;
            routerUpdate.SetData(dataToRouter);
            ControlActionsReader.setRouterUpdate1(routerUpdate);

            String message = routerUpdate.ToStringWithProtocolType();

            byte[] message_bytes = ByteCoder.toBytes(message);
            System.Threading.Thread.Sleep(1000);

            udp.Send(message_bytes, message_bytes.Length, device.EndPoint);

            String RouterUpdateTable = $"<Router {device.Name} Table Updated>";
            Logs.ControlLOG(cc.Name, RouterUpdateTable, Colors.CC);
            if(cache.SNPList.Count != 0)
            {
                cache.SNPList.Remove(snp1);
                cache.SNPList.Remove(snp2);
            }
            
        }

        public void reconfigureConnection(String link)
        {
            //lrm wyswietla log, ze sie zepsulo
            String lrmlog = $"<Link malfunction> {link}";
            Logs.ControlLOG(lrm.Name, lrmlog, Colors.LRM);

            int tmp = -1;
            for (int i = 0; i < LinksList.Count; i++)
            {
                if(LinksList[i].id == int.Parse(link))
                {
                    tmp = i;
                    break;
                }
            }
            LinksList[tmp].isalive = false;
            String lrmlog1 = "<LocalTopology>";
            Logs.ControlLOG(lrm.Name, lrmlog1, Colors.LRM);
            Logs.ControlLOG(rc.devName, lrmlog1, Colors.RC);
            Logs.ControlLOG(lrm.Name, lrmlog, Colors.LRM);
            Logs.ControlLOG(cc.Name, lrmlog, Colors.CC);
            Tuple<int, int> snps = new Tuple<int, int>(LinksList[tmp].SNPs.Item1, LinksList[tmp].SNPs.Item2);
            
            foreach (KeyValuePair<string, Cache> entry in CacheDic)
            {
                Tuple<int, int> bordersnps = new Tuple<int, int>(Convert.ToInt32(entry.Value.SNPList[0]), Convert.ToInt32(entry.Value.SNPList[entry.Value.SNPList.Count-1]));

                foreach (int snp in entry.Value.SNPList)
                {
                    if(snp == snps.Item1 || snp == snps.Item2)
                    {
                        lrm.SNPDealocate(this, new ControlProtocolReader(), entry.Value, entry.Value.bandwidth.ToString());
                        //entry.Value.SNPList.Clear();
                        entry.Value.SNPList.RemoveAt(0);
                        entry.Value.SNPList.RemoveAt(entry.Value.SNPList.Count - 1);
                        while (entry.Value.SNPList.Count != 0)
                        {
                            foreach (NetworkDevice device in NetworkDevicesList)
                            {
                                int snp1 = 0;
                                int snp2 = 1;
                                if (entry.Value.SNPList.Count != 0)
                                {
                                    snp1 = Convert.ToInt32(entry.Value.SNPList[0]);
                                    snp2 = Convert.ToInt32(entry.Value.SNPList[1]);


                                }
                                else { break; }
                                if ((device.SNPs.Item1 == snp1 && device.SNPs.Item2 == snp2) || (device.SNPs.Item2 == snp1 && device.SNPs.Item1 == snp2))
                                {
                                    deallocateRouter(entry.Value, device, snp1, snp2);
                                }

                            }
                        }

                        int tmp1 = -1;
                        int tmp2 = -1;
                        
                        if(bordersnps.Item1 == 30302)
                        {
                            tmp1 = 50302;
                        }
                        else if (bordersnps.Item2 == 30302)
                        {
                            tmp2 = 50302;
                        }
                        if (bordersnps.Item1 == 80301)
                        {
                            tmp1 = 60302;
                        }
                        else if (bordersnps.Item2 == 80301)
                        {
                            tmp2 = 60302;
                        }
                        if (bordersnps.Item1 == 90301)
                        {
                            tmp1 = 70304;
                        }
                        else if (bordersnps.Item2 == 90301)
                        {
                            tmp2 = 70304;
                        }

                        
                        int[] tmpSNPList = rc.DijkstraAlgorithm(this, tmp1, tmp2);
                        foreach(int element in tmpSNPList)
                        {
                            entry.Value.SNPList.Add(element);
                            entry.Value.SNPListTMP.Add(element);
                           
                        }
                        entry.Value.SNPListTMP.RemoveAt(0);
                        entry.Value.SNPListTMP.RemoveAt(entry.Value.SNPListTMP.Count-1);
                        

                       /* foreach(int element in entry.Value.SNPListTMP)
                        {
                            Console.WriteLine($"wartosc z SNPListTMP: {element}");
                        }*/
                        while (entry.Value.SNPListTMP.Count != 0)
                        {
                            foreach (NetworkDevice device in NetworkDevicesList)
                            {
                                
                                int snp1 = 0;
                                int snp2 = 1;
                                if (entry.Value.SNPListTMP.Count != 0)
                                {
                                    snp1 = Convert.ToInt32(entry.Value.SNPListTMP[0]);
                                    snp2 = Convert.ToInt32(entry.Value.SNPListTMP[1]);

                                }
                                else { break; }
                                
                                if ((device.SNPs.Item1 == snp1 && device.SNPs.Item2 == snp2) || (device.SNPs.Item2 == snp1 && device.SNPs.Item1 == snp2))
                                {
                                    System.Threading.Thread.Sleep(2000);
                                   
                                    allocateRouter(entry.Value, device, snp1, snp2);
                                }
                            }
                        }
                            
                        lrm.SNPAlocate(this, new ControlProtocolReader(), entry.Value, entry.Value.bandwidth.ToString());

                        break;
                    }
                }
            }

            //
        }
    }
}
