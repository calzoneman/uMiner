using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uMiner
{
    public class Server
    {
        //Default values
        public int plyCount = 0;
        public int maxPlayers = 0;
        public int salt = new Random().Next();
        public string serverName = "uMinerServer";
        public string motd = "In Development";
        public const int protocolVersion = Protocol.version;
        public bool isPublic = true;
        public int port = 25565;

        //Heartbeat stuff
        public static Heartbeat beater = new Heartbeat(BeatType.Minecraft);
        System.Timers.Timer heartbeatTimer = new System.Timers.Timer(60 * 1000.0);

        //Logger
        public Logger logger;

        public Server(ServerType type)
        {
            logger = new Logger();
            if (type == ServerType.POC)
            {
            }
        }

        public void Init()
        {
            logger.log("Server initialized");
            //Load config
            //Load ranks
            //etc
            
            //Init heartbeat
            heartbeatTimer.Elapsed += new System.Timers.ElapsedEventHandler(delegate
            {
                beater.Beat(false);
            });
            heartbeatTimer.Start();
            beater.Beat(true);  //Initial heartbeat
        }


        public void Run()
        {
            while(true)
            {
            }
        }
    }
}
