/**
 * uMiner - A lightweight custom Minecraft Classic server written in C#
 * Copyright 2010 Calvin "calzoneman" Montgomery
 * 
 * Licensed under the Creative Commons Attribution-ShareAlike 3.0 Unported License
 * (see http://creativecommons.org/licenses/by-sa/3.0/, or LICENSE.txt for a full license
 */


using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Linq;
using System.Text;

namespace uMiner
{
    public class Server
    {
        //Default values
        public int plyCount = 0;
        public int maxPlayers = 2;
        public int salt = new Random().Next();
        public string serverName = "uMinerServer";
        public string motd = "In Development";
        public const int protocolVersion = Protocol.version;
        public bool isPublic = true;
        public int port = 25565;
        public bool running = true;
        
        //Players
        public List<Player> playerlist = new List<Player>();

        //Ranks
        public Dictionary<string, byte> playerRanksDict;

        //TcpListener
        public TcpListener listener;
        public System.Threading.Thread ListenThread;



        //Heartbeat stuff
        public static Heartbeat beater = new Heartbeat(BeatType.Minecraft);
        System.Timers.Timer heartbeatTimer = new System.Timers.Timer(60 * 1000.0);

        //Logger
        public Logger logger;

        public Server(ServerType type)
        {
            logger = new Logger();
        }

        public void Init()
        {
            logger.log("Server initialized");
            //Load config
            playerlist.Capacity = maxPlayers;
            //Load ranks
            playerRanksDict = new Dictionary<string, byte>();
            try
            {
                if (File.Exists("ranks.txt"))
                {
                    StreamReader sr = new StreamReader(File.OpenRead("ranks.txt"));
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (line != null && !line.Equals("") && line.IndexOf(':') >= 0)
                        {
                            string user = line.Substring(0, line.IndexOf(':'));
                            byte rank = byte.Parse(line.Substring(line.IndexOf(':') + 1));
                            playerRanksDict.Add(user, rank);
                        }
                    }
                    sr.Close();
                }
            }
            catch (Exception e)
            {
                logger.log("FATAL ERROR WHILE LOADING RANKS", Logger.LogType.Error);
                logger.log(e);
                running = false;
                return;
            }
            logger.log("Loaded ranks from ranks.txt");
            //etc
            
            //Init heartbeat
            heartbeatTimer.Elapsed += new System.Timers.ElapsedEventHandler(delegate
            {
                beater.Beat(false);
            });
            heartbeatTimer.Start();
            beater.Beat(true);  //Initial heartbeat

            this.ListenThread = new System.Threading.Thread( new System.Threading.ThreadStart( delegate
                {
                    this.listener = new TcpListener(IPAddress.Any, this.port);
                    try
                    {
                        listener.Start();
                        listener.BeginAcceptSocket(new AsyncCallback(SocketAccept), null);
                    }
                    catch(Exception e)
                    {
                        listener.Stop();
                        logger.log(e);
                    }
                    logger.log("Started listener thread");
                }));
            this.ListenThread.Start();
                    

        }


        public void Run()
        {
            while(true)  //Main Loop
            {
                if (!running) { return; }
                System.Threading.Thread.Sleep(10);
            }
        }
                
        public void SocketAccept(IAsyncResult result)
        {
            try
            {
                Socket sock = listener.EndAcceptSocket(result);
                logger.log("Accepted socket from " + sock.RemoteEndPoint.ToString(), Logger.LogType.Info);
                playerlist.Add(new Player(sock));
                listener.BeginAcceptSocket(new AsyncCallback(SocketAccept), null);
            }
            catch (Exception e)
            {
                logger.log(e);
            }
        }

        public void saveRanks()
        {
            try
            {
                StreamWriter sw = new StreamWriter(File.Open("ranks.txt", FileMode.OpenOrCreate, FileAccess.Write));
                foreach (KeyValuePair<string, byte> k in this.playerRanksDict)
                {
                    sw.WriteLine(k.Key + ":" + k.Value.ToString());
                }
                sw.Close();
            }
            catch (Exception e)
            {
                logger.log("Exception occurred while saving ranks", Logger.LogType.Error);
                logger.log(e);
                return;
            }
            logger.log("Saved ranks to ranks.txt");
        }
    }
}
