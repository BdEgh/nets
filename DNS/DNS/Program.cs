using System;

namespace DNS
{
    internal static class Program
    {
    
        private static void Main()
        {
            var s = new DnsServer(10000);
            while (true)
            {
                if (Console.ReadLine()?.ToLower() != "exit")
                    continue;
                s.Stop();
                break;
            }
        }
    }
}