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
        public string serverName = "uMiner Server";
        public string motd = "Welcome to my uMiner Server!";
        public string worldPath = "default.umw";
        public bool isPublic = true;
        public int port = 25565;
        public bool verify_names = true;
        
        //Server stuff
        public string salt;

        public const int protocolVersion = Protocol.version;
        public bool running = true;
        
        //Players
        public Player[] playerlist;
        public List<string> ipbanned;

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
            //Init salt
            string saltChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-.~";
            Random rand = new Random();
            int saltLength = rand.Next(12, 16);
            for (int i = 0; i < saltLength; i++)
            {
                salt += saltChars[rand.Next(0, saltChars.Length - 1)];
            }

            //Load config
            LoadConfig();

            playerlist = new Player[maxPlayers + 2];  //Extra slot is for rejects, and another one (for some odd reason it's less by one)
            for (int i = 0; i < maxPlayers + 2; i++)
            {
                playerlist[i] = null;
            }

            //Load world and start save timer
            if (!File.Exists("maps/" + worldPath))
            {
                world = new World(64, 64, 64);
                world.Save();
            }
            else
            {
                world = new World(worldPath);
                world.Save();
            }
            worldSaveTimer = new System.Timers.Timer(60000.0);
            worldSaveTimer.Elapsed += new System.Timers.ElapsedEventHandler(delegate
            {
                world.Save();
            });

            //Intercept Ctrl+C
            Console.CancelKeyPress += new ConsoleCancelEventHandler(delegate
                {
                    world.Save();
                });


            //Load Commands
            Command.Init();
            

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

            ipbanned = new List<string>();
            try
            {
                if (File.Exists("ipbans.txt"))
                {
                    StreamReader sr = new StreamReader(File.OpenRead("ipbans.txt"));
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (line != null && !line.Equals(""))
                        {
                            ipbanned.Add(line.Trim());
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
            logger.log("Loaded ipbans from ipbans.txt");
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

        public void saveIpBans()
        {
            try
            {
                StreamWriter sw = new StreamWriter(File.Open("ipbanned.txt", FileMode.OpenOrCreate, FileAccess.Write));
                foreach (string ip in this.ipbanned)
                {
                    sw.WriteLine(ip);
                }
                sw.Close();
            }
            catch (Exception e)
            {
                logger.log("Exception occurred while saving ipbans", Logger.LogType.Error);
                logger.log(e);
                return;
            }
            logger.log("Saved ipbans to ipbans.txt");
        }

        public void LoadConfig()
        {
            try
            {
                if (File.Exists("server.cfg"))
                {
                    StreamReader cfgReader = new StreamReader(File.OpenRead("server.cfg"));
                    while (!cfgReader.EndOfStream)
                    {
                        string line = cfgReader.ReadLine();
                        if (line.Replace(" ", "")[0] == '#') { continue; }
                        string field = line.Substring(0, line.IndexOf('=')).Trim();
                        string arg = line.Substring(line.IndexOf('=') + 1).Trim();
                        if (arg.Contains("#")) { arg = arg.Substring(0, arg.IndexOf("#")); }
                        switch (field)
                        {
                            case "server-name":
                                this.serverName = arg;
                                break;
                            case "server-motd":
                                this.motd = arg;
                                break;
                            case "port":
                                this.port = Int32.Parse(arg);
                                break;
                            case "max-players":
                                this.maxPlayers = Int32.Parse(arg);
                                break;
                            case "default-world":
                                this.worldPath = arg;
                                break;
                            case "verify-names":
                                this.verify_names = Boolean.Parse(arg);
                                break;
                            case "public":
                                this.isPublic = Boolean.Parse(arg);
                                break;
                            default:
                                break;
                        }
                    }
                    cfgReader.Close();
                }
                else
                {
                    Program.server.logger.log("File \"server.cfg\" not found.  Generating default.", Logger.LogType.Warning);
                    StreamWriter cfgWriter = new StreamWriter(File.Create("server.cfg"));
                    cfgWriter.WriteLine("#uMiner Configuration File");
                    cfgWriter.WriteLine("server-name = uMiner Server");
                    cfgWriter.WriteLine("server-motd = Welcome to my uMiner server");
                    cfgWriter.WriteLine("port = 25565");
                    cfgWriter.WriteLine("max-players = 8");
                    cfgWriter.WriteLine("default-world = default.umw");
                    cfgWriter.WriteLine("verify-names = true");
                    cfgWriter.WriteLine("public = true");
                    cfgWriter.Close();
                }
            }
            catch (Exception e)
            {
                Program.server.logger.log("Unable to load or generate config!  Full error:", Logger.LogType.Error);
                Program.server.logger.log(e);
            }
        }
    }
}
