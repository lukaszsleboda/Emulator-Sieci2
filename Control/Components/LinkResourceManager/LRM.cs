using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using API;
using API.Protocols.ControlProtocol.Reader;
using API.Protocols.ControlProtocol.Actions;
using API.Protocols.ControlProtocol.Components;
using API.LOGS;
using API.BytesCoder;


namespace Control.Components.LinkResourceManager
{
    public class LRM
    {
        public String Name { get; set; }
        public LRM()
        {

        }


        public void lrmComands(Control control, ControlProtocolReader controlProtocol)
        {
            System.Threading.Thread.Sleep(500);
            if (ControlActionsReader.isSNPNegotiation(controlProtocol))
            {
                String lrmNEgotiation = $"<SMPNegotiationIndicator>";
                Logs.ControlLOG(Name, lrmNEgotiation, Colors.LRM);

                Cache cache = control.CacheDic[controlProtocol.ID()];
                String data = controlProtocol.Data;
                String[] dataString = data.Split('&');
   
                int requirement = int.Parse(dataString[1]);
                int proposal = int.Parse(dataString[2]);

                List<int> lambdas_list = new List<int>();
                for(int i=0; i<requirement; i++)
                {
                    lambdas_list.Add(proposal + i);
                }
                cache.lambdas_requirement = requirement;
                //sprawdzamy czy w keszu są już lambdy
                if (cache.lambdas.Count != 0)
                {

                    //sprawdzamy czy lambdy w keszu są równe lambdom w wiadomości która do nas przyszła
                    if (cache.lambdas[0] == proposal)

                    { 
                        SNPAlocate(control, controlProtocol, cache, cache.bandwidth.ToString());
                        control.cc.LinkConnectionResponse(control, controlProtocol);

                    }
                    //lambdy w keszu są innymi niż lambdy w wiadomości
                    else
                    {
                        if (cache.lambdas.Count != 0)
                        {
                            foreach (int cs in cache.lambdas)
                            {
                               

                                cache.lambdas.Remove(cs);
                            }
                        }
                        SNPNegotiation(control, controlProtocol);
                    }
                }
                //W keszu nie ma lambd. Czyli pierwsza wiadomosc SNPNegotiation do nas.
                else
                {

                    //Sprawdzamy czy propozycje lambd są fajne
                    if (isLambdasAvaliable(control,proposal,requirement))
                    {

                        //zapisujemy w keszu
                        foreach (int lambda in lambdas_list)
                        {
                            cache.lambdas.Add(lambda);
                        }
                        //wysylamy taką samą wiadomość bez zmian
                        String neighborDomainName = checkNeighborName(control, cache);
                        String message = controlProtocol.ToStringWithProtocolType();
                        byte[] message_bytes = ByteCoder.toBytes(message);

                        System.Threading.Thread.Sleep(500);

                        control.udp.Send(message_bytes, message_bytes.Length, control.Controls[neighborDomainName]);


                        String snpNeg = "<SNP NegotiationReponse>";
                        Logs.ControlLOG(Name, snpNeg, Colors.LRM);


                    }
                    else
                    {

                        SNPNegotiation(control,controlProtocol);
                    }
                }

                


            }
        }


        public void LinkConnectionRequestResponse(Control control, ControlProtocolReader controlProtocol, String bandwidth, String senderDomain)
        {
            if (controlProtocol.Action == StaticActions.ALLOCATE)
            {
                String lrmIndicator = $"<LinkConnectionIndicator> {bandwidth}Gb/s";
                Logs.ControlLOG(Name, lrmIndicator, Colors.LRM);
            }
            else if (controlProtocol.Action == StaticActions.DEALLOCATE)
            {
                String lrmIndicator = $"<LinkDeallocationIndicator> {bandwidth}Gb/s";
                Logs.ControlLOG(Name, lrmIndicator, Colors.LRM);
            }

                Cache cache = control.CacheDic[controlProtocol.ID()];
            if(cache.lambdas.Count != 0)
            {
               // control.printLinks();
                if (controlProtocol.Action == StaticActions.ALLOCATE)
                {

                    SNPAlocate(control, controlProtocol, cache, bandwidth);
                    foreach(int lambda in cache.lambdas)
                    {
                        control.available_lambdas[lambda] = false;
                    }
                }
                else if (controlProtocol.Action == StaticActions.DEALLOCATE)
                {
                    SNPDealocate(control, controlProtocol, cache, bandwidth);
                }
                 //   control.printLinks();
                control.cc.LinkConnectionResponse(control, controlProtocol);

            }
            else
            {

                if (cache.ToDomain == control.DomainName && cache.FromDomain == control.DomainName)
                {

                    int[] lambdasProposal = randomLambdas(control, cache.lambdas_requirement);
                    foreach (int lambda in lambdasProposal)
                    {
                      
                        cache.lambdas.Add(lambda);
                        control.available_lambdas[lambda] = false;
                    }
                   // control.printLinks();
                    SNPAlocate(control, controlProtocol, cache, bandwidth);
                   // control.printLinks();
                    control.cc.LinkConnectionResponse(control, controlProtocol);


                }
                else if (cache.FromDomain != control.DomainName)
                {

                    SNPNegotiation(control, controlProtocol);
                }
            }
                

                //122, 222, 333, 444, 555
 
        }
        public void SNPDealocate(Control control, ControlProtocolReader controlProtocol, Cache cache, String bandwidth)
        {
            int ln = cache.SNPList.Count;
            if (ln != 0)
            {//1,2,3,4,5
                for (int i = 0; i < ln - 1; i++)
                {
                    foreach (Link link in control.LinksList)
                    {


                            int snp1 = Convert.ToInt32(cache.SNPList[i]);
                            int snp2 = Convert.ToInt32(cache.SNPList[i+1]);


                        if ((link.SNPs.Item1 == snp1 && link.SNPs.Item2 == snp2) || (link.SNPs.Item2 == snp1 && link.SNPs.Item1 == snp2))
                        {
                            link.actual_bandwidth += int.Parse(bandwidth);
                            foreach (int lambda in cache.lambdas)
                            {
                                link.usingLambdas.Remove(lambda);
                            }

                            String lrmAlocateLog = $"[LRM{link.id}]<ResourcesDeallocated> {bandwidth}Gb/s";
                            Logs.ControlLOG(Name, lrmAlocateLog, Colors.LRM);
                        }
                    }
                }

                String lrmAlocateLog2 = $"[LRMs <LocalTopology> ";
                Logs.ControlLOG(Name, lrmAlocateLog2, Colors.LRM);
                String lrmAlocateLog3 = $"<LocalTopology> ";
                Logs.ControlLOG(control.rc.devName, lrmAlocateLog3, Colors.RC);

            }


        }

        public void SNPAlocate(Control control, ControlProtocolReader controlProtocol, Cache cache, String bandwidth)
        {
            int ln = cache.SNPList.Count;
            if (ln != 0)
            {//1,2,3,4,5
                for (int i=0; i<ln-1; i++)
                {
                    foreach (Link link in control.LinksList)
                    {

                        int snp1 = Convert.ToInt32(cache.SNPList[i]);
                        int snp2 = Convert.ToInt32(cache.SNPList[i + 1]);

                        if ((link.SNPs.Item1 == snp1 && link.SNPs.Item2 == snp2) || (link.SNPs.Item2 == snp1 && link.SNPs.Item1 == snp2))
                        {
                            link.actual_bandwidth -= int.Parse(bandwidth);
                            foreach(int lambda in cache.lambdas)
                            {
                                link.usingLambdas.Add(lambda);
                            }

                            String lrmAlocateLog = $"[LRM{link.id}]<ResourcesAllocated> {bandwidth}Gb/s";
                            Logs.ControlLOG(Name, lrmAlocateLog, Colors.LRM);
                        }
                    }
                }
                String lrmAlocateLog2 = $"[LRMs <LocalTopology> ";
                Logs.ControlLOG(Name, lrmAlocateLog2, Colors.LRM);
                String lrmAlocateLog3 = $"<LocalTopology> ";
                Logs.ControlLOG(control.rc.devName, lrmAlocateLog3, Colors.RC);

            }
        }


        public void SNPNegotiation(Control control, ControlProtocolReader controlProtocol)
        {
            Cache cache = control.CacheDic[controlProtocol.ID()];

            int[] lambdasProposal = randomLambdas(control, cache.lambdas_requirement);

            String lrmProposal = "";
            foreach (int lambda in lambdasProposal)
            {
                cache.lambdas.Add(lambda);
                lrmProposal += $"{lambda}, ";
                control.available_lambdas[lambda] = false;//
            }

            cache.lambdas_requirement = lambdasProposal.Length;
            controlProtocol.SetData($"PROPOSAL&{cache.lambdas_requirement}&{lambdasProposal[0]}");
            ControlActionsReader.setSNPNegotiation(controlProtocol);
            ControlComponentsReader.setFromLRM(controlProtocol);
            ControlComponentsReader.setToLRM(controlProtocol);
            String message2 = controlProtocol.ToStringWithProtocolType();
            byte[] message_bytes = ByteCoder.toBytes(message2);

            String neighborDomainName = checkNeighborName(control, cache);

            System.Threading.Thread.Sleep(500);

            control.udp.Send(message_bytes, message_bytes.Length, control.Controls[neighborDomainName]);

            String negotiation = $"<SNP Negotiation> {cache.lambdas_requirement}, MyProposal:{lrmProposal}";
            Logs.ControlLOG(Name, negotiation, Colors.LRM);
        }


        public int[]  randomLambdas(Control control, int requirements)
        {
            Random rnd = new Random();
            bool finish = false;
            int first = 0;
            List<int> returned_lambdas = new List<int>();
            while (!finish)
            {

                first = rnd.Next(1, 90 - requirements + 1);

                if (isLambdasAvaliable(control,first,requirements))
                {
                    finish = true;
                }
            }
            for (int i = first; i < requirements+first; i++)
            {
                returned_lambdas.Add(i);
            }
            foreach(int element in returned_lambdas)
            {
            }

            //return new int[] { first, first + 1, first + 2 };
            return returned_lambdas.ToArray();
        }

        public bool isLambdasAvaliable(Control control, int first, int requirements)
        {
            bool finish = false;
            int counter = 0;
            for (int i = first; i < first + requirements; i++)
            {
                if (control.available_lambdas[i])
                {
                    counter++;
                }
                else
                {
                    break;
                }
            }
            if (counter == requirements)
            {
                finish = true;
            }
            return finish;
        }

        public String checkNeighborName(Control control,Cache cache)
        {
            String myDomain = control.DomainName;
            if(myDomain != cache.FromDomain)
            {
                return cache.FromDomain;
            }
            else
            {
                return cache.ToDomain;
            }
        }



        public void LocalTopology()
        {
          
        }

        public void ComputeSzczelins()
        {

        }
    }
}
