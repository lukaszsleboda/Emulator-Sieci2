using System;
using System.Text;

namespace API.BytesCoder
{
    public class ByteCoder
    {
        public static byte[] toBytes(String data)
        {
            return Encoding.ASCII.GetBytes(data);
        }

        public static String fromBytes(byte[] data)
        {
             return System.Text.Encoding.UTF8.GetString(data);
        }
    }
}
