using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uMiner
{
    public class Program
    {
        public static Server server = new Server(ServerType.POC);
        static void Main(string[] args)
        {
            server.Init();
            server.Run();
        }
    }

    public enum ServerType
    {
        POC // Proof Of Concept
    }
}
