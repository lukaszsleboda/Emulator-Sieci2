using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Control
{
    class CCActions
    {
        /// <summary>
        /// Dzwonię do RC. Prośba o drogę pomiędzy H1 a H2
        /// </summary>
        public void RouteTableQuery()
        {

        }

        /// <summary>
        /// Dzwonię do LRM. Rządanie o zarezerwowanie zasobów na łączach nowej drogi.
        /// </summary>
        public void LinkConnectionRequest()
        {

        }

        /// <summary>
        /// Dzwonię do CC niżej. Podaję gdzie wchodzi i gdzie wychodzi. Bo nasze CC widzi tylko wejścia i wyjścia podsieci. 
        /// </summary>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        public void ConnectionRequestToCC(IPEndPoint id1, IPEndPoint id2)
        {

        }

        /// <summary>
        /// Dzwonię do CC innej podsieci. Mówię mu skąd dokąd chce zadzwonić.
        /// </summary>
        /// <param name="id1">Host dzwoniący</param>
        /// <param name="id2">Host do którego dzwonimy</param>
        public void PeerCoordination(IPEndPoint id1, IPEndPoint id2)
        {

        }


        public void LinkConnectionDealocation()
        {

        }



    }
}
