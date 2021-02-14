using System;
namespace API.LOGS
{
    public class TimeTool
    {
        public TimeTool()
        {
        }

        public static String getActualTime()
        {
            // return DateTime.Now.ToString("yyyy-MM-dd - HH:mm:ss:ffff");
            return DateTime.Now.ToString("HH:mm:ss:ffff");
        }
    }
}
