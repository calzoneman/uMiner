using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uMiner
{
    class Server
    {
        public int plyCount = 0;
        public int maxPlayers = 0;
        public int salt = new Random().Next();
        public string serverName = "uMinerServer";
        public string motd = "In Development";
        public const int protocolVersion = Protocol.version;
        public bool isPublic = true;
        public int port = 25565;
        public static Server s = new Server(ServerType.POC);
        public Server(ServerType type)
        {
            if (type == ServerType.POC)
            {
            }
        }

        public void Run()
        {
            System.Timers.Timer heartbeatTimer = new System.Timers.Timer(10 * 1000.0);
            heartbeatTimer.Elapsed += new System.Timers.ElapsedEventHandler(delegate
                {
                    new Heartbeat(BeatType.Minecraft, ref s).Beat();
                });
            heartbeatTimer.Start();
            new Heartbeat(BeatType.Minecraft, ref s).Beat();
            while(true)
            {
            }
        }
    }
}
