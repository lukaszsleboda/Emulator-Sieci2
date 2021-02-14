using System;
namespace API.LOGS
{
    public class Logs
    {
        

        public static void ControlLOG(String devName, String message, String logColor = "WHITE")
        {
            setColor(logColor);

            Console.WriteLine($"[C] [{devName}] [{TimeTool.getActualTime()}] {message}");

            SaveToFile.WriteToFile(message, "control", devName);
            Console.ResetColor();
        }

        public static void TransportLOG(String devName, String message, String logColor = "WHITE")
        {
            setColor(logColor);

            Console.WriteLine($"[T] [{devName}] [{TimeTool.getActualTime()}] {message}");

            SaveToFile.WriteToFile(message, "transport", devName);
            Console.ResetColor();
        }


        private static void setColor(String logColor)
        {
            switch (logColor)
            {
                case "BLACK":
                    Console.ForegroundColor = ConsoleColor.Black;
                    break;

                case "WHITE":
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case "RED":
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case "YELLOW":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case "GREEN":
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;

                case "BLUE":
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;

                case "CYAN":
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;

                case "MAGENTA":
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
        }
    }
}
