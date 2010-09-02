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
        
        //Players
        public List<Player> playerlist = new List<Player>();

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
                //Process player buffers
                for(int i = 0; i < playerlist.Count; i++)
                {
                    Player p = playerlist[i];
                    if (p.readSize == 0)
                    {
                        p.readSize++;
                        p.socket.Receive(p.buffer, 1, SocketFlags.None);
                        int length = Protocol.incomingPacketLengths[p.buffer[0]];
                        p.readSize += p.socket.Receive(p.buffer, 1, length - 1, SocketFlags.None);
                        p.Process();
                    }
                }
                //Give it a rest
                System.Threading.Thread.Sleep(100);
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
    }
}
