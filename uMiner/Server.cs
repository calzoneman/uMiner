/**
 * uMiner - A lightweight custom Minecraft Classic server written in C#
 * Copyright 2010 Calvin "calzoneman" Montgomery
 * 
 * Licensed under the Creative Commons Attribution-ShareAlike 3.0 Unported License
 * (see http://creativecommons.org/licenses/by-sa/3.0/, or LICENSE.txt for a full license
 */


using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace uMiner
{
    public class Server
    {
        //Default config values
        public int plyCount = 0;
        public int maxPlayers = 3;
        public string serverName = "uMinerServer";
        public string motd = "In Development";
        public bool isPublic = true;
        public int port = 25565;
        public bool verify_names = true;
        
        //Server stuff
        public int salt = new Random().Next();
        public const int protocolVersion = Protocol.version;
        public bool running = true;
        
        //Players
        public Player[] playerlist;

        //Map
        public World world;
        public System.Timers.Timer worldSaveTimer;

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
            playerlist = new Player[maxPlayers + 1];  //Extra slot is for rejects
            for (int i = 0; i < maxPlayers + 1; i++)
            {
                playerlist[i] = null;
            }

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
                logger.log("Error while loading ranks:", Logger.LogType.Error);
                logger.log(e);
                running = false;
                return;
            }
            logger.log("Loaded ranks from ranks.txt");
            //etc
            
            //Initialize heartbeat timer
            heartbeatTimer.Elapsed += new System.Timers.ElapsedEventHandler(delegate
                {
                    beater.Beat(false);
                });
            heartbeatTimer.Start();
            beater.Beat(true);  //Initial heartbeat
            
            //Start TCP listener thread
            this.ListenThread = new System.Threading.Thread( new System.Threading.ThreadStart( delegate
                {
                    this.listener = new TcpListener(IPAddress.Any, this.port);
                    try
                    {
                        listener.Start();
                        listener.BeginAcceptTcpClient(new AsyncCallback(ClientAccept), null);
                    }
                    catch(Exception e)
                    {
                        listener.Stop();
                        logger.log(e);
                    }
                    logger.log("Started listener thread");
                }));
            this.ListenThread.IsBackground = true;
            this.ListenThread.Start();

        }


        public void Run()
        {
            if (!File.Exists("maps/default.umw"))
            {
                world = new World(64, 64, 64);
                world.Save();
            }
            else
            {
                world = new World("default.umw");
                world.Save();
            }
            worldSaveTimer = new System.Timers.Timer(60000.0);
            worldSaveTimer.Elapsed += new System.Timers.ElapsedEventHandler(delegate
                {
                    world.Save();
                });
            worldSaveTimer.Start();
            while(true)  //Main Loop
            {
                if (!running) { return; }
                System.Threading.Thread.Sleep(10);
            }
        }
                
        public void ClientAccept(IAsyncResult result) //Accepts TCP connections and assigns them to players
        {
            try
            {
                TcpClient client = listener.EndAcceptTcpClient(result);
                string ip = ((IPEndPoint)(client.Client.RemoteEndPoint)).Address.ToString();
                logger.log("Accepted socket from " + ip, Logger.LogType.Info);
                
                for (int i = 0; i < playerlist.Length; i++)
                {
                    if (playerlist[i] == null)
                    {
                        playerlist[i] = new Player(client, ip, (byte)i);
                        break;
                    }
                }
                listener.BeginAcceptTcpClient(new AsyncCallback(ClientAccept), null);
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
