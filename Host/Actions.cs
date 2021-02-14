using System;
using System.Collections.Generic;
using System.Text;

namespace Host
{
    class Actions
    {
        /// <summary>
        /// Cześć, jestem Łukasz. Chcę zadzwonić do Maćka. Zestaw mi połączenie. Wysyła do NCC.
        /// </summary>
        /// <param name="id1">Host dzwoniący</param>
        /// <param name="id2">Host odbierający</param>
        public void CallRequest(String id1, String id2)
        {

        }

        /// <summary>
        /// Decyzja, czy chce z nim gadać, czy ma iść na bambus
        /// </summary>
        public void CallResponse()
        {

        }

        /// <summary>
        /// Koniec połączenia, nie chce już z nim gadać.
        /// </summary>
        /// <param name="id1">Host dzwoniący</param>
        /// <param name="id2">Host odbierający</param>
        public void CallTeardown(String id1, String id2)
        {

        }

    }
}
