using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uMiner
{
    class Program
    {
        static void Main(string[] args)
        {
            Server.s.Run();
        }
    }

    public enum ServerType
    {
        POC
    }
}
