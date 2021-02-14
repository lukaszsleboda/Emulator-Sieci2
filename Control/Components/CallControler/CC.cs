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

namespace Control.Components.CallControler
{
    public class CC
    {
        public String Name { get; set; }
        public List<IPEndPoint> routerlist { get; set; }
        public CC()
        {

        }

        public void ccCommands(Control control, ControlProtocolReader controlProtocol)
        {

            if (ControlActionsReader.isCallCoordinationResponse(controlProtocol))
            {
                RouteTableQueryRequest(control, controlProtocol);
            }
            else if (ControlActionsReader.isPeerCoordinationResponse(controlProtocol))
            {

                RouteTableQueryRequest(control, controlProtocol);
            }
            else if (ControlActionsReader.isConnectionRequestResponse(controlProtocol))
            {
                String msg2 = $"<ConnectionlRequestResponse> Subnetwork finished";
                Logs.ControlLOG(Name, msg2, Colors.CC);

                LinkConnectionResponse(control, controlProtocol);

            }
            else if (ControlActionsReader.isConnectionRequestRequest(controlProtocol) || ControlActionsReader.isPeerCoordinationRequest(controlProtocol))
            {
                String data = controlProtocol.Data;
                String[] dataString = data.Split('&');
                IPEndPoint h1 = new IPEndPoint(IPAddress.Parse(dataString[0]), int.Parse(dataString[1]));
                IPEndPoint h2 = new IPEndPoint(IPAddress.Parse(dataString[3]), int.Parse(dataString[4]));
                String bandwidth = dataString[6];
                String h1Domain = dataString[2];
                String h2Domain = dataString[5];


                if (controlProtocol.Action == StaticActions.ALLOCATE)
                {

                    ControlProtocolReader sndProtocol = new ControlProtocolReader(controlProtocol);
                    control.CacheDic.Add(sndProtocol.ID(), new Cache());
                    control.CacheDic[sndProtocol.ID()].FromEndPoint = h1;
                    control.CacheDic[sndProtocol.ID()].FromDomain = h1Domain;
                    control.CacheDic[sndProtocol.ID()].DstEndPoint = h2;
                    control.CacheDic[sndProtocol.ID()].ToDomain = h2Domain;
                    control.CacheDic[sndProtocol.ID()].bandwidth = int.Parse(bandwidth);
                 //   control.printCache();
                }


                if (ControlActionsReader.isConnectionRequestRequest(controlProtocol))
                {

                    String indicatorMessage = $"<ConnectionRequestIndicator> {h1} -> {h2} /{bandwidth}/Gb/s ";
                    Logs.ControlLOG(Name, indicatorMessage, Colors.CC);

                }
                else if (ControlActionsReader.isPeerCoordinationRequest(controlProtocol))
                {
                    String peerLog = $"<PeerCoordinationIndicator> {h1} -> {h2} {bandwidth}Gb/s";
                    Logs.ControlLOG(Name, peerLog, Colors.CC);

                }



                //Jeśli domena Control zgadza się z domeną adresu docelowego
                if (control.DomainName == h2Domain)
                {
                    RouteTableQueryRequest(control, controlProtocol);
                    //Tutaj idziemy do RC i mówimy odmień być (TableQuert)
                }
                else
                {
                    PeerCoordinationRequest(control, controlProtocol, h1, h2, bandwidth, h2Domain);
                    //Tutaj dzwonimy do kumpla naszego CC z innej domeny CallCoordination
                }
            }


        }

        public void PeerCoordinationRequest(Control control, ControlProtocolReader controlProtocol, IPEndPoint h1, IPEndPoint h2, String bandwidth, String h2Domain)
        {
            ControlComponentsReader.setToCC(controlProtocol);
            ControlComponentsReader.setFromCC(controlProtocol);
            ControlActionsReader.setPeerCoordinationRequest(controlProtocol);
            controlProtocol.setTransitDomain(true);


            String message = controlProtocol.ToStringWithProtocolType();
            byte[] message_bytes = ByteCoder.toBytes(message);

            System.Threading.Thread.Sleep(1000);

            control.udp.Send(message_bytes, message_bytes.Length, control.Controls[h2Domain]);

            String peerLog = $"<PeerCoordinationRequest> {h1} -> {h2} {bandwidth}Gb/s";
            Logs.ControlLOG(Name, peerLog, Colors.CC);

        }


        public void RouteTableQueryRequest(Control control, ControlProtocolReader controlProtocol)
        {
            String data = controlProtocol.Data;

            String[] dataString = data.Split('&');
            IPEndPoint h1 = new IPEndPoint(IPAddress.Parse(dataString[0]), int.Parse(dataString[1]));
            String senderDomain = dataString[2];
            IPEndPoint h2 = new IPEndPoint(IPAddress.Parse(dataString[3]), int.Parse(dataString[4]));
            String bandwidth = dataString[6];




            if (controlProtocol.Action == StaticActions.ALLOCATE)
            {
                String routerMessageLog = $"<RouteTableQueryRequest> {h1} -> {h2} {bandwidth}Gb/s";
                Logs.ControlLOG(Name, routerMessageLog, Colors.CC);
                if (controlProtocol.isTransitSubnetwork())
                {
                    control.rc.RouteTableQueryResponse(control, controlProtocol, int.Parse(dataString[7]), int.Parse(dataString[8]));

                }
                else
                {
                    control.rc.RouteTableQueryResponse(control, controlProtocol);
                }
                RouteTableQueryConfirm(h1, h2, bandwidth);
              //  control.printCache();
            }


            if (controlProtocol.Action == StaticActions.ALLOCATE)
            {
                //TODO czy na pewno wyświetlamy punkty docelowe całej sieci?
                String lrmmessage = $"<LinkConnectionRequest> {h1} -> {h2} {bandwidth}Gb/s";
                Logs.ControlLOG(Name, lrmmessage, Colors.CC);
            }
            else if (controlProtocol.Action == StaticActions.DEALLOCATE)
            {
                String lrmmessage = $"<LinkConnectionDeallocation> {h1} -> {h2} {bandwidth}Gb/s";
                Logs.ControlLOG(Name, lrmmessage, Colors.CC);
            }
            control.lrm.LinkConnectionRequestResponse(control, controlProtocol, bandwidth, senderDomain);

        }


        public void RouteTableQueryConfirm(IPEndPoint h1, IPEndPoint h2, String bandwidth)
        {
            String confirm = $"<RouteTableQueryConfirm> {h1} -> {h2} {bandwidth}Gb/s ";
            Logs.ControlLOG(Name, confirm, Colors.CC);

        }


        public void LinkConnectionResponse(Control control, ControlProtocolReader controlProtocol)
        {
            Cache cache = new Cache();
            if (control.CacheDic.ContainsKey(controlProtocol.ID()))
            {

                if (controlProtocol.Action == StaticActions.ALLOCATE)
                {
                    String response = $"<LinkConnectionConfirm> LinksAlocated";
                    Logs.ControlLOG(Name, response, Colors.CC);
                }
                else if (controlProtocol.Action == StaticActions.DEALLOCATE)
                {
                    String response = $"<LinkConnectionConfirm> LinksDeallocated";
                    Logs.ControlLOG(Name, response, Colors.CC);
                }

                cache = control.CacheDic[controlProtocol.ID()];

             //   control.printNetworkDevices();

                if ((cache.SNPListTMP.Count == 0 && controlProtocol.isTransitDomain() && controlProtocol.Action == StaticActions.ALLOCATE) || (cache.SNPList.Count == 0 && controlProtocol.isTransitDomain() && controlProtocol.Action == StaticActions.DEALLOCATE))
                {
                    controlProtocol.Data = $"{cache.FromEndPoint.Address}&{cache.FromEndPoint.Port}&{cache.FromDomain}&{cache.DstEndPoint.Address}&{cache.DstEndPoint.Port}&{cache.ToDomain}&{cache.bandwidth}&{cache.lambdas.Count}&{cache.lambdas[0]}";
                    ControlActionsReader.setPeerCoordinationResponse(controlProtocol);
                    ControlComponentsReader.setFromCC(controlProtocol);
                    ControlComponentsReader.setToCC(controlProtocol);
                    controlProtocol.setTransitDomain(false);
                    String message4 = controlProtocol.ToStringWithProtocolType();



                    byte[] message_bytes3 = ByteCoder.toBytes(message4);

                    System.Threading.Thread.Sleep(1000);

                    control.udp.Send(message_bytes3, message_bytes3.Length, control.Controls[cache.FromDomain]);
                    String toSub = $"<PeerCoordinationResponse>";
                    Logs.ControlLOG(Name, toSub, Colors.CC);
                }




                else if (cache.SNPListTMP.Count == 0 && !controlProtocol.isTransitDomain() && controlProtocol.Action == StaticActions.ALLOCATE)
                {
                    ControlActionsReader.setConnectionRequestResponse(controlProtocol);
                    ControlComponentsReader.setToNCC(controlProtocol);
                    ControlComponentsReader.setFromCC(controlProtocol);
                    controlProtocol.Data = $"{cache.FromEndPoint.Address}&{cache.FromEndPoint.Port}&{cache.FromDomain}&{cache.DstEndPoint.Address}&{cache.DstEndPoint.Port}&{cache.ToDomain}&{cache.bandwidth}&{cache.lambdas.Count}&{cache.lambdas[0]}";


                    String message = controlProtocol.ToStringWithProtocolType();
                    byte[] message_bytes = ByteCoder.toBytes(message);
                    System.Threading.Thread.Sleep(1000);

                    control.udp.Send(message_bytes, message_bytes.Length, control.NCCpoint);
                }

                if (cache.SNPList.Count != 0 && controlProtocol.Action == StaticActions.DEALLOCATE)
                {
                    int snp1 = Convert.ToInt32(cache.SNPList[0]);
                    int snp2 = Convert.ToInt32(cache.SNPList[cache.SNPList.Count - 1]);
                    if (controlProtocol.isTransitSubnetwork())
                    {
                        if (snp2 == 30302 || snp2 == 80301 || snp2 == 90301)
                        {

                            cache.SNPList.RemoveAt(cache.SNPList.Count - 1);

                        }
                        if (snp1 == 30302 || snp1 == 80301 || snp1 == 90301)
                        {
                            cache.SNPList.RemoveAt(0);

                        }
                    }
                    else if (control.DomainName == "D1")
                    {
                        if (snp2 == 50302 || snp2 == 10111 || snp2 == 10112)
                        {

                            cache.SNPList.RemoveAt(cache.SNPList.Count - 1);

                        }
                        if (snp1 == 50302 || snp1 == 10111 || snp1 == 10112)
                        {
                            cache.SNPList.RemoveAt(0);

                        }
                    }
                    else if (control.DomainName == "D2")
                    {
                        if (snp2 == 10113 || snp2 == 10114 || snp2 == 30302)
                        {

                            cache.SNPList.RemoveAt(cache.SNPList.Count - 1);

                        }
                        if (snp1 == 10113 || snp1 == 10114 || snp1 == 30302)
                        {
                            cache.SNPList.RemoveAt(0);

                        }
                    }


                }

                while (cache.SNPList.Count != 0 && controlProtocol.Action == StaticActions.DEALLOCATE)
                {


                    foreach (NetworkDevice device in control.NetworkDevicesList)
                    {
                        int snp1 = 0;
                        int snp2 = 1;
                        if (cache.SNPList.Count != 0)
                        {
                            snp1 = Convert.ToInt32(cache.SNPList[0]);
                            snp2 = Convert.ToInt32(cache.SNPList[1]);

                        }
                        else { break; }



                        if ((device.SNPs.Item1 == snp1 && device.SNPs.Item2 == snp2) || (device.SNPs.Item2 == snp1 && device.SNPs.Item1 == snp2))
                        {
                            if (device.DeviceType == NetworkDevTypes.ROUTER_TYPE)
                            {

                                //wysylamy do rutera

                                String dataToRouter = $"{cache.SNPList[0]}&{cache.SNPList[1]}&{cache.lambdas_requirement.ToString()}&{cache.lambdas[0]}";
                                ControlProtocolReader routerUpdate = new ControlProtocolReader(controlProtocol);
                                routerUpdate.Action = StaticActions.DEALLOCATE;
                                routerUpdate.SetData(dataToRouter);
                                ControlActionsReader.setRouterUpdate1(routerUpdate);

                                String message = routerUpdate.ToStringWithProtocolType();

                                byte[] message_bytes = ByteCoder.toBytes(message);
                                System.Threading.Thread.Sleep(1000);

                                control.udp.Send(message_bytes, message_bytes.Length, device.EndPoint);

                                String RouterUpdateTable = $"<Router {device.Name} Table Updated>";
                                Logs.ControlLOG(Name, RouterUpdateTable, Colors.CC);

                                cache.SNPList.Remove(snp1);
                                cache.SNPList.Remove(snp2);
                            }
                            else if (device.DeviceType == NetworkDevTypes.SUBNETWORK_TYPE)
                            {
                                //dzwonimy do CC nizej
                                ControlComponentsReader.setToCC(controlProtocol);
                                ControlComponentsReader.setFromCC(controlProtocol);
                                ControlActionsReader.setConnectionRequestRequest(controlProtocol);
                                controlProtocol.setTransitSubnetwork(true);
                                controlProtocol.Data = $"{cache.FromEndPoint.Address}&{cache.FromEndPoint.Port}&{cache.FromDomain}&{cache.DstEndPoint.Address}&{cache.DstEndPoint.Port}&{cache.ToDomain}&{cache.bandwidth}&{snp1}&{snp2}&{cache.lambdas_requirement}&{cache.lambdas[0]}";


                                String message = controlProtocol.ToStringWithProtocolType();
                                byte[] message_bytes = ByteCoder.toBytes(message);
                                System.Threading.Thread.Sleep(1000);
                                control.udp.Send(message_bytes, message_bytes.Length, control.Controls[device.Name]);

                                String toSub = $"<ConnectionRequest> Call to Sub";
                                Logs.ControlLOG(Name, toSub, Colors.CC);


                                cache.SNPList.Remove(snp1);
                                cache.SNPList.Remove(snp2);
                            }
                        }
                        if (cache.SNPList.Count == 0)
                        {

                            if (controlProtocol.isTransitSubnetwork())
                            {

                                ControlActionsReader.setConnectionRequestResponse(controlProtocol);

                                controlProtocol.setTransitSubnetwork(false);


                                String message3 = controlProtocol.ToStringWithProtocolType();

                                byte[] message_bytes3 = ByteCoder.toBytes(message3);


                                if (40112 != control.ControlEndPoint.Port)
                                {
                                    IPEndPoint endPointDoKolegiWYzej = new IPEndPoint(IPAddress.Parse("127.0.0.42".ToString()), int.Parse("40112"));
                                    controlProtocol.setTransitDomain(true);
                                    System.Threading.Thread.Sleep(1000);
                                    control.udp.Send(message_bytes3, message_bytes3.Length, endPointDoKolegiWYzej);
                                    //dzwonimy do kunpla NadSIeci

                                    String doKumplaCC = $"<ConnectionRequestReponse>";
                                    Logs.ControlLOG(Name, doKumplaCC, Colors.CC);



                                }


                            }


                            else if (controlProtocol.isTransitDomain() && !controlProtocol.isTransitSubnetwork())
                            {
                                controlProtocol.Data = $"{cache.FromEndPoint.Address}&{cache.FromEndPoint.Port}&{cache.FromDomain}&{cache.DstEndPoint.Address}&{cache.DstEndPoint.Port}&{cache.ToDomain}&{cache.bandwidth}&{cache.lambdas.Count}&{cache.lambdas[0]}";

                                ControlActionsReader.setPeerCoordinationResponse(controlProtocol);
                                ControlComponentsReader.setFromCC(controlProtocol);
                                ControlComponentsReader.setToCC(controlProtocol);
                                controlProtocol.setTransitDomain(false);
                                String message4 = controlProtocol.ToStringWithProtocolType();


                                byte[] message_bytes3 = ByteCoder.toBytes(message4);

                                System.Threading.Thread.Sleep(1000);

                                control.udp.Send(message_bytes3, message_bytes3.Length, control.Controls[cache.FromDomain]);
                                String toSub = $"<PeerCoordinationResponse>";
                                Logs.ControlLOG(Name, toSub, Colors.CC);

                               /* if (cache.SNPList.Count == 0 && controlProtocol.Action == StaticActions.DEALLOCATE)
                                {
                                    control.CacheDic.Remove(controlProtocol.ID());
                                }*/

                                //wysyłamy do kumpla CC
                            }
                            else if (!controlProtocol.isTransitDomain() && !controlProtocol.isTransitSubnetwork())
                            {
                                //Dzwonimy do NCC 
                                ControlActionsReader.setConnectionRequestResponse(controlProtocol);
                                ControlComponentsReader.setToNCC(controlProtocol);
                                ControlComponentsReader.setFromCC(controlProtocol);
                                controlProtocol.Data = $"{cache.FromEndPoint.Address}&{cache.FromEndPoint.Port}&{cache.FromDomain}&{cache.DstEndPoint.Address}&{cache.DstEndPoint.Port}&{cache.ToDomain}&{cache.bandwidth}&{cache.lambdas.Count}&{cache.lambdas[0]}";



                                String message = controlProtocol.ToStringWithProtocolType();
                                byte[] message_bytes = ByteCoder.toBytes(message);
                                System.Threading.Thread.Sleep(2000);

                                /*if (cache.SNPList.Count == 0 && controlProtocol.Action == StaticActions.DEALLOCATE)
                                {
                                    control.CacheDic.Remove(controlProtocol.ID());
                                }*/

                                control.udp.Send(message_bytes, message_bytes.Length, control.NCCpoint);

                            }
                        }
                    }



                }



                while ((cache.SNPListTMP.Count != 0) && controlProtocol.Action == StaticActions.ALLOCATE)
                {
                    foreach (int element in cache.SNPListTMP)
                    {
                    }
                    foreach (NetworkDevice device in control.NetworkDevicesList)
                    {
                        int snp1 = 0;
                        int snp2 = 1;
                        if (cache.SNPListTMP.Count != 0)
                        {
                            snp1 = Convert.ToInt32(cache.SNPListTMP[0]);
                            snp2 = Convert.ToInt32(cache.SNPListTMP[1]);

                        }
                        else { break; }

                        if ((device.SNPs.Item1 == snp1 && device.SNPs.Item2 == snp2) || (device.SNPs.Item2 == snp1 && device.SNPs.Item1 == snp2))
                        {
                            if (device.DeviceType == NetworkDevTypes.ROUTER_TYPE)
                            {

                                //wysylamy do rutera

                                String dataToRouter = $"{cache.SNPListTMP[0]}&{cache.SNPListTMP[1]}&{cache.lambdas_requirement.ToString()}&{cache.lambdas[0]}";
                                ControlProtocolReader routerUpdate = new ControlProtocolReader(controlProtocol);
                                routerUpdate.SetData(dataToRouter);
                                ControlActionsReader.setRouterUpdate1(routerUpdate);

                                String message = routerUpdate.ToStringWithProtocolType();

                                byte[] message_bytes = ByteCoder.toBytes(message);
                                System.Threading.Thread.Sleep(1000);

                                control.udp.Send(message_bytes, message_bytes.Length, device.EndPoint);

                                String RouterUpdateTable = $"<Router {device.Name} Table Updated>";
                                Logs.ControlLOG(Name, RouterUpdateTable, Colors.CC);


                                cache.SNPListTMP.Remove(snp1);
                                cache.SNPListTMP.Remove(snp2);
                            }
                            else if (device.DeviceType == NetworkDevTypes.SUBNETWORK_TYPE)
                            {
                                //dzwonimy do CC nizej

                                ControlComponentsReader.setToCC(controlProtocol);
                                ControlComponentsReader.setFromCC(controlProtocol);
                                ControlActionsReader.setConnectionRequestRequest(controlProtocol);
                                controlProtocol.setTransitSubnetwork(true);
                                controlProtocol.Data = $"{cache.FromEndPoint.Address}&{cache.FromEndPoint.Port}&{cache.FromDomain}&{cache.DstEndPoint.Address}&{cache.DstEndPoint.Port}&{cache.ToDomain}&{cache.bandwidth}&{snp1}&{snp2}&{cache.lambdas_requirement}&{cache.lambdas[0]}";


                                String message = controlProtocol.ToStringWithProtocolType();
                                byte[] message_bytes = ByteCoder.toBytes(message);
                                System.Threading.Thread.Sleep(1000);
                                control.udp.Send(message_bytes, message_bytes.Length, control.Controls[device.Name]);

                                String toSub = $"<ConnectionRequest> Call to Sub";
                                Logs.ControlLOG(Name, toSub, Colors.CC);


                                cache.SNPListTMP.Remove(snp1);
                                cache.SNPListTMP.Remove(snp2);
                            }
                        }
                        if (cache.SNPListTMP.Count == 0)
                        {
                            if (controlProtocol.isTransitSubnetwork())
                            {
                                ControlActionsReader.setConnectionRequestResponse(controlProtocol);
                                controlProtocol.setTransitSubnetwork(false);
                                String message3 = controlProtocol.ToStringWithProtocolType();
                                byte[] message_bytes3 = ByteCoder.toBytes(message3);

                                if (40112 != control.ControlEndPoint.Port)
                                {
                                    IPEndPoint endPointDoKolegiWYzej = new IPEndPoint(IPAddress.Parse("127.0.0.42".ToString()), int.Parse("40112"));
                                    controlProtocol.setTransitDomain(true);
                                    System.Threading.Thread.Sleep(1000);
                                    control.udp.Send(message_bytes3, message_bytes3.Length, endPointDoKolegiWYzej);
                                    //dzwonimy do kunpla NadSIeci

                                    String doKumplaCC = $"<ConnectionRequestReponse> Reponse";
                                    Logs.ControlLOG(Name, doKumplaCC, Colors.CC);


                                }
                            }



                            else if (controlProtocol.isTransitDomain() && !controlProtocol.isTransitSubnetwork())
                            {
                                controlProtocol.Data = $"{cache.FromEndPoint.Address}&{cache.FromEndPoint.Port}&{cache.FromDomain}&{cache.DstEndPoint.Address}&{cache.DstEndPoint.Port}&{cache.ToDomain}&{cache.bandwidth}&{cache.lambdas.Count}&{cache.lambdas[0]}";

                                ControlActionsReader.setPeerCoordinationResponse(controlProtocol);
                                ControlComponentsReader.setFromCC(controlProtocol);
                                ControlComponentsReader.setToCC(controlProtocol);
                                controlProtocol.setTransitDomain(false);
                                String message4 = controlProtocol.ToStringWithProtocolType();


                                byte[] message_bytes3 = ByteCoder.toBytes(message4);

                                System.Threading.Thread.Sleep(1000);

                                control.udp.Send(message_bytes3, message_bytes3.Length, control.Controls[cache.FromDomain]);
                                String toSub = $"<PeerCoordinationResponse>";
                                Logs.ControlLOG(Name, toSub, Colors.CC);



                                //wysyłamy do kumpla CC
                            }
                            else if (!controlProtocol.isTransitDomain() && !controlProtocol.isTransitSubnetwork())
                            {
                                //Dzwonimy do NCC 
                                ControlActionsReader.setConnectionRequestResponse(controlProtocol);
                                ControlComponentsReader.setToNCC(controlProtocol);
                                ControlComponentsReader.setFromCC(controlProtocol);
                                controlProtocol.Data = $"{cache.FromEndPoint.Address}&{cache.FromEndPoint.Port}&{cache.FromDomain}&{cache.DstEndPoint.Address}&{cache.DstEndPoint.Port}&{cache.ToDomain}&{cache.bandwidth}&{cache.lambdas.Count}&{cache.lambdas[0]}";

                                String message = controlProtocol.ToStringWithProtocolType();
                                byte[] message_bytes = ByteCoder.toBytes(message);
                                System.Threading.Thread.Sleep(2000);

                                control.udp.Send(message_bytes, message_bytes.Length, control.NCCpoint);

                            }
                        }

                    }
                }
            }

        }





        public void ccComandsSubnetwork(Control control, ControlProtocolReader controlProtocol)
        {
            if (ControlActionsReader.isConnectionRequestRequest(controlProtocol))
            {
                ControlProtocolReader sndProtocol = new ControlProtocolReader(controlProtocol);
                String data = controlProtocol.Data;
                String[] dataString = data.Split('&');
                IPEndPoint h1 = new IPEndPoint(IPAddress.Parse(dataString[0]), int.Parse(dataString[1]));
                IPEndPoint h2 = new IPEndPoint(IPAddress.Parse(dataString[3]), int.Parse(dataString[4]));
                String bandwidth = dataString[6];
                String h1Domain = dataString[2];
                String h2Domain = dataString[5];
                int snp1 = int.Parse(dataString[7]);
                int snp2 = int.Parse(dataString[8]);
                int rqs_lmb = int.Parse(dataString[9]);
                int first_lmb = int.Parse(dataString[10]);

                if (controlProtocol.Action == StaticActions.ALLOCATE)
                {
                    control.CacheDic.Add(sndProtocol.ID(), new Cache());
                    control.CacheDic[sndProtocol.ID()].FromEndPoint = h1;
                    control.CacheDic[sndProtocol.ID()].FromDomain = h1Domain;
                    control.CacheDic[sndProtocol.ID()].DstEndPoint = h2;
                    control.CacheDic[sndProtocol.ID()].ToDomain = h2Domain;
                    control.CacheDic[sndProtocol.ID()].bandwidth = int.Parse(bandwidth);
                   // control.printCache();

                    for (int i = 0; i < rqs_lmb; i++)
                    {
                        control.CacheDic[sndProtocol.ID()].lambdas.Add(first_lmb + i);
                    }
                    control.CacheDic[sndProtocol.ID()].lambdas_requirement = rqs_lmb;
                }

              //  control.printCache();

                //control.rc.RouteTableQueryResponse(control, controlProtocol,snp1,snp2);

                String indicatorMessage = $"<ConnectionRequestIndicator> {h1} -> {h2} /{bandwidth}/Gb/s ";
                Logs.ControlLOG(Name, indicatorMessage, Colors.CC);
                RouteTableQueryRequest(control, controlProtocol);

            }
        }


    }
}
