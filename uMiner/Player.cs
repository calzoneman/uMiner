/**
 * uMiner - A lightweight custom Minecraft Classic server written in C#
 * Copyright 2010 Calvin "calzoneman" Montgomery
 * 
 * Licensed under the Creative Commons Attribution-ShareAlike 3.0 Unported License
 * (see http://creativecommons.org/licenses/by-sa/3.0/, or LICENSE.txt for a full license
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;



namespace uMiner
{
    public class Player
    {
        public short x, y, z;
        public byte rotx, roty;
        public string username;
        public string prefix = "";
        public string ip;
        public bool loggedIn = false;
        public int loginTmr = 0;
        public byte id;
        public byte rank = 0x01; //Guest
        public bool disconnected = false;

        public World world;

        object queueLock = new object();

        public TcpClient plyClient;
        public BinaryReader inputReader;
        public BinaryWriter outputWriter;
        public Queue<Packet> outQueue;

        public Thread IOThread;

        public Player(TcpClient client, string ip, byte id)
        {
            this.username = "player";
            this.plyClient = client;
            this.x = 0;
            this.y = 0;
            this.z = 0;
            this.rotx = 0;
            this.roty = 0;
            this.prefix = "";
            this.id = id;
            this.ip = ip;

            this.world = new World(16, 16, 16);

            this.outQueue = new Queue<Packet>();
            this.IOThread = new Thread(PlayerIO);
            this.outputWriter = new BinaryWriter(client.GetStream());
            this.inputReader = new BinaryReader(client.GetStream());

            this.IOThread.IsBackground = true;
            this.IOThread.Start();
        }

        public void PlayerIO()
        {
            try
            {
                Login();
            }
            catch (IOException) { Disconnect(true); }
            catch (SocketException) { Disconnect(true); }
            catch (Exception e) { Program.server.logger.log(e); Disconnect(true); }

            DateTime pingTime = DateTime.Now;
            while (!disconnected)
            {
                try
                {
                    //Send whatever remains in the queue
                    lock (queueLock)
                    {
                        
                        while (outQueue.Count > 0)
                        {
                            Packet p = outQueue.Dequeue();
                            this.outputWriter.Write(p.raw);
                        }
                    }
                    if (((TimeSpan)(DateTime.Now - pingTime)).TotalSeconds > 2)
                    {
                        this.outputWriter.Write((byte)ServerPacket.Ping); //Ping
                        pingTime = DateTime.Now;
                    }

                    //Accept input
                    while (plyClient.GetStream().DataAvailable)
                    {
                        byte opcode = this.inputReader.ReadByte();
                        switch ((ClientPacket)opcode)
                        {
                            case ClientPacket.Login:
                                if (loggedIn)
                                {
                                    Program.server.logger.log("Player " + username + " has already logged in!", Logger.LogType.Warning);
                                    Kick("Already logged in", false);
                                }
                                break;

                            case ClientPacket.Blockchange:
                                PlayerBlockchange();
                                break;
                            case ClientPacket.MoveRotate:
                                PositionChange();
                                break;
                            case ClientPacket.Message:
                                PlayerMessage();
                                break;
                            default:
                                Program.server.logger.log("Unhandled packet type \"" + opcode + "\"", Logger.LogType.Warning);
                                Kick("Unknown packet type", false);
                                break;
                        }
                    }
                    //Clean up
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    Thread.Sleep(10);

                }

                catch (IOException) { Disconnect(false); }
                catch (SocketException) { Disconnect(false); }
                catch (Exception e) { Program.server.logger.log(e); Disconnect(false); }
                                

            }
        }


        #region Received Data
        public void Login()
        {
            byte opLogin = this.inputReader.ReadByte();
            if (opLogin != (byte)ClientPacket.Login)
            {
                Program.server.logger.log("Wrong login opcode received from " + ip, Logger.LogType.Warning);
                Kick("Wrong Login Opcode", true);
                return;
            }
            byte plyProtocol = this.inputReader.ReadByte();
            if (plyProtocol != Protocol.version)  //Shouldn't happen
            {
                Program.server.logger.log("Wrong protocol version received from " + ip, Logger.LogType.Warning);
                Kick("Wrong Protocol Version", true);
                return;
            }
            //Read username
            this.username = Encoding.ASCII.GetString(this.inputReader.ReadBytes(64)).Trim();
            //There is also an mppass for name verification but we don't care (yet)
            this.inputReader.ReadBytes(64);

            //Unused byte
            this.inputReader.ReadByte();

            //Check Rank
            if (Program.server.playerRanksDict.ContainsKey(username))
            {
                this.rank = Program.server.playerRanksDict[username];
            }
            else
            {
                this.rank = Rank.RankLevel("guest");
                Program.server.saveRanks();
            }

            //Send a response
            this.outputWriter.Write((byte)ServerPacket.Login);
            this.outputWriter.Write((byte)Protocol.version); // Protocol version
            this.outputWriter.Write(Encoding.ASCII.GetBytes(Program.server.serverName.PadRight(64).Substring(0, 64))); // name
            this.outputWriter.Write(Encoding.ASCII.GetBytes(Program.server.motd.PadRight(64).Substring(0, 64))); //motd

            if (rank >= Rank.RankLevel("operator")) { this.outputWriter.Write((byte)0x64); } //Can break adminium
            else { this.outputWriter.Write((byte)0x00); } //Cannot break adminium

            Program.server.logger.log(ip + " logged in as " + username);

            //Find an empty slot for them
            bool emptySlot = false;
            for (int i = 0; i < Program.server.playerlist.Length - 1; i++)
            {
                if (Program.server.playerlist[i] == null)
                {
                    emptySlot = true;
                    break;
                }
            }
            if (!emptySlot) //Server is full :(
            {
                Kick("Server is full!", true);
                return;
            }

            //We are logged in now
            loggedIn = true;

            //If they are ranked operator or admin, give them a snazzy prefix
            if (rank >= 128) { prefix = "+"; }
            if (rank == 255) { prefix = "@"; }

            //Send the map
            byte[] emptymap = new byte[4096];
            for (int i = 0; i < 256; i++) { emptymap[i] = (byte)2; }
            for (int i = 256; i < 4096; i++) { emptymap[i] = (byte)0; }
            this.SendPacket(new Packet(new byte[1] { (byte)ServerPacket.MapBegin }));
            SendMap(Program.server.world);

            //Announce the player's arrival
            string loginMessage = "[ " + Rank.GetColor(rank);
            if(!prefix.Equals(""))
            {
                loginMessage += prefix;
            }
            loginMessage += username + "&e joined the game ]";
            GlobalMessage(loginMessage);
            
        }

        public void PrintPlayerlist()
        {
            for(int i = 0; i < Program.server.playerlist.Length; i++)
            {
                string name = "";
                if(Program.server.playerlist[i] == null)
                {
                    name = "null";
                }
                else
                {
                    name = Program.server.playerlist[i].username;
                }
                Console.WriteLine(i + "|" + name);
            }
        }



        public void PositionChange()
        {
            this.inputReader.ReadByte();
            this.x = IPAddress.NetworkToHostOrder(this.inputReader.ReadInt16());
            this.y = IPAddress.NetworkToHostOrder(this.inputReader.ReadInt16());
            this.z = IPAddress.NetworkToHostOrder(this.inputReader.ReadInt16());
            this.rotx = this.inputReader.ReadByte();
            this.roty = this.inputReader.ReadByte();
            foreach (Player pl in Program.server.playerlist)
            {
                if (pl != null && pl.loggedIn && pl != this)
                {
                    pl.SendPlayerPositionChange(this);
                }
            }
        }

        public void PlayerMessage()
        {
            this.inputReader.ReadByte();
            string rawmsg = Encoding.ASCII.GetString(this.inputReader.ReadBytes(64)).Trim();
            string message = "";
            if (prefix != "")
            {
                Program.server.logger.log("<" + prefix + "" + username + "> " + rawmsg, Logger.LogType.Chat);
                message = Rank.GetColor(rank) + "<" + prefix + "" + username + "> &f" + rawmsg;
            }
            else
            {
                Program.server.logger.log("<" + username + "> " + rawmsg, Logger.LogType.Chat);
                message = Rank.GetColor(rank) + "<" + username + "> &f" + rawmsg;
            }
            foreach (Player p in Program.server.playerlist)
            {
                if (p != null && p.loggedIn)
                {
                    p.SendMessage(id, message);
                }
            }

        }

        public void PlayerBlockchange()
        {
            short x = IPAddress.HostToNetworkOrder(this.inputReader.ReadInt16());
            short y = IPAddress.HostToNetworkOrder(this.inputReader.ReadInt16());
            short z = IPAddress.HostToNetworkOrder(this.inputReader.ReadInt16());
            byte action = this.inputReader.ReadByte();
            byte type = this.inputReader.ReadByte();

            byte mapBlock = world.GetTile(x, y, z);

            if (mapBlock == 7 && rank < Rank.RankLevel("operator"))
            {
                Kick("Attempted to break adminium", false);
                return;
            }
            if ((type >= 7 && type <= 11) && rank < Rank.RankLevel("operator"))
            {
                Kick("Illegal tile type", false);
                return;
            }

            if (action == 0)
            {
                type = 0;
            }
            this.world.SetTile(x, y, z, type);

            foreach (Player p in Program.server.playerlist)
            {
                if (p != null && p.loggedIn)
                {
                    p.SendBlock(x, y, z, type);
                }
            }
        }


        #endregion

        #region Sending
        public void SendMessage(byte pid, string message)
        {
            try
            {
                foreach (string line in SplitLines(message))
                {
                    if (!loggedIn) { return; }
                    Packet msgPacket = new Packet(66);
                    msgPacket.Append((byte)ServerPacket.Message);
                    msgPacket.Append(pid);
                    msgPacket.Append(line);
                    this.SendPacket(msgPacket);
                }
            }

            catch (IOException) { }
            catch (SocketException) { }
            catch (Exception e) { Program.server.logger.log(e); }
        }

        public void SendBlock(short x, short y, short z, byte type)
        {
            Packet block = new Packet(8);
            block.Append((byte)ServerPacket.Blockchange);
            block.Append(x);
            block.Append(y);
            block.Append(z);
            block.Append(type);
            this.SendPacket(block);
        }

        public void Kick(string reason, bool silent)  //Disconnect someone
        {
            try
            {
                if (!loggedIn) { silent = true; }
                if (!this.plyClient.Connected) //Oops
                {
                    Program.server.logger.log("Player " + username + " has already disconnected.", Logger.LogType.Warning);
                    return;
                }
                //Send kick (0x0e + kick message)
                this.outputWriter.Write((byte)ServerPacket.Kick);
                this.outputWriter.Write(reason);
                
                this.plyClient.Close();
                Program.server.logger.log("Player " + username + " kicked (" + reason + ")");
                if (!silent)
                {
                    GlobalMessage("[ Player " + Rank.GetColor(rank) + prefix + username + "&e kicked (" + reason + ") ]");
                }
                Disconnect(silent);
            }
            catch
            {
                Disconnect(true);
            }
        }

        /*public void SendMap(ref byte[] leveldata, short width, short height, short depth)
        {
            try
            {
                byte[] buffer = new byte[leveldata.Length + 4];
                BitConverter.GetBytes(IPAddress.HostToNetworkOrder(leveldata.Length)).CopyTo(buffer, 0);
                for (int i = 0; i < leveldata.Length; ++i)
                {
                    buffer[4 + i] = leveldata[i];
                }
                buffer = GZip(buffer);
                int number = (int)Math.Ceiling(((double)buffer.Length) / 1024);
                for (int i = 1; buffer.Length > 0; ++i)
                {
                    Packet chunk = new Packet(1028);
                    short length = (short)Math.Min(buffer.Length, 1024);
                    chunk.Append((byte)ServerPacket.MapChunk);
                    chunk.Append(length);
                    chunk.Append(byteArraySlice(ref buffer, 0, length));
                    for (short j = length; j < 1024; j++)
                    {
                        chunk.Append((byte)0);
                    }
                    byte[] tempbuffer = new byte[buffer.Length - length];
                    Buffer.BlockCopy(buffer, length, tempbuffer, 0, buffer.Length - length);
                    buffer = tempbuffer;
                    chunk.Append((byte)((i * 100.0) / number));
                    this.SendPacket(chunk);
                    System.Threading.Thread.Sleep(1);
                }
                Packet mapFinal = new Packet(7);
                mapFinal.Append((byte)ServerPacket.MapFinal);
                mapFinal.Append((short)width);
                mapFinal.Append((short)depth);
                mapFinal.Append((short)height);
                this.SendPacket(mapFinal);

                //Spawn player
                this.SpawnPlayer(this, true);
                this.SendSpawn(new short[3] { 8 * 32 + 16, 64, 8 * 32 + 16 }, new byte[2] { 0, 0 });

                //Spawn other players
                foreach (Player p in Program.server.playerlist)
                {
                    if (p != null && p.loggedIn && p != this)
                    {
                        this.SpawnPlayer(p, false);
                    }
                }

                //Spawn self
                GlobalSpawnPlayer(this);
            }
            catch (IOException) { }
            catch (SocketException) { }
            catch (Exception e) { Program.server.logger.log(e); }

        } */

        public void SendMap(World w)
        {
            try
            {
                byte[] buffer = new byte[w.blocks.Length + 4];
                BitConverter.GetBytes(IPAddress.HostToNetworkOrder(w.blocks.Length)).CopyTo(buffer, 0);
                for (int i = 0; i < w.blocks.Length; ++i)
                {
                    buffer[4 + i] = w.blocks[i];
                }
                buffer = GZip(buffer);
                int number = (int)Math.Ceiling(((double)buffer.Length) / 1024);
                for (int i = 1; buffer.Length > 0; ++i)
                {
                    Packet chunk = new Packet(1028);
                    short length = (short)Math.Min(buffer.Length, 1024);
                    chunk.Append((byte)ServerPacket.MapChunk);
                    chunk.Append(length);
                    chunk.Append(byteArraySlice(ref buffer, 0, length));
                    for (short j = length; j < 1024; j++)
                    {
                        chunk.Append((byte)0);
                    }
                    byte[] tempbuffer = new byte[buffer.Length - length];
                    Buffer.BlockCopy(buffer, length, tempbuffer, 0, buffer.Length - length);
                    buffer = tempbuffer;
                    chunk.Append((byte)((i * 100.0) / number));
                    this.SendPacket(chunk);
                    System.Threading.Thread.Sleep(1);
                }
                Packet mapFinal = new Packet(7);
                mapFinal.Append((byte)ServerPacket.MapFinal);
                mapFinal.Append(w.width);
                mapFinal.Append(w.depth);
                mapFinal.Append(w.height);
                this.SendPacket(mapFinal);

                //Spawn player
                this.x = (short)(w.spawnx * 32 + 16);
                this.y = (short)(w.spawny * 32);
                this.z = (short)(w.spawnz * 32 + 16);
                this.SpawnPlayer(this, true);

                //Spawn other players
                foreach (Player p in Program.server.playerlist)
                {
                    if (p != null && p.loggedIn && p != this)
                    {
                        this.SpawnPlayer(p, false);
                    }
                }

                //Spawn self
                GlobalSpawnPlayer(this);

                this.world = w;
            }
            catch (IOException) { }
            catch (SocketException) { }
            catch (Exception e) { Program.server.logger.log(e); }

        }

        public void SpawnPlayer(Player p, bool self)
        {
            try
            {
                Packet spawn = new Packet(74);
                spawn.Append((byte)ServerPacket.SpawnEntity);
                if (self) { spawn.Append((byte)255); }
                else { spawn.Append(p.id); }

                spawn.Append(Rank.GetColor(p.rank) + p.username); //username
                spawn.Append((short)p.x); //x position
                spawn.Append((short)p.y); //y position
                spawn.Append((short)p.z); //z position

                spawn.Append(p.rotx); //x rotation
                spawn.Append(p.roty); //y rotation
                this.SendPacket(spawn);
            }

            catch (IOException) { }
            catch (SocketException) { }
            catch (Exception e) { Program.server.logger.log(e); }
        }

        public void SendSpawn(short[] pos, byte[] rot)
        {
            try
            {
                Packet spawn = new Packet(10);
                //Now move+rotate (teleport)
                spawn.Append((byte)ServerPacket.MoveRotate); //Move+Rotate
                spawn.Append((byte)255); // Self

                spawn.Append(pos[0]); //x position
                spawn.Append(pos[1]); //y position
                spawn.Append(pos[2]); //z position

                spawn.Append(rot[0]); //x rotation
                spawn.Append(rot[1]); //y rotation
                this.SendPacket(spawn);
            }

            catch (IOException) { }
            catch (SocketException) { }
            catch (Exception e) { Program.server.logger.log(e); }
        }

        public void SendPlayerPositionChange(Player p)
        {
            try
            {
                Packet posChange = new Packet(10);
                posChange.Append((byte)ServerPacket.MoveRotate);
                posChange.Append(p.id);

                posChange.Append(p.x);
                posChange.Append(p.y);
                posChange.Append(p.z);

                posChange.Append(p.rotx);
                posChange.Append(p.roty);
                this.SendPacket(posChange);
            }

            catch (IOException) { }
            catch (SocketException) { }
            catch (Exception e) { Program.server.logger.log(e); }
        }

        public void SendPacket(Packet p)
        {
            lock (queueLock)
            {
                this.outQueue.Enqueue(p);
            }
        }

        #endregion

        #region Global Stuff

        public static void GlobalMessage(string message)
        {
            foreach (Player p in Program.server.playerlist)
            {
                try
                {
                    if (p != null && p.loggedIn && !p.disconnected)
                    {
                        p.SendMessage(0xFF, message);
                    }
                }
                catch
                {
                    Program.server.logger.log("Failed to send Global Message to " + p.username, Logger.LogType.Warning);
                    p.Disconnect(false);
                }
            }
            Program.server.logger.log("(Global) " + message, Logger.LogType.Chat);
        }

        public static void GlobalSpawnPlayer(Player p)
        {
            foreach (Player pl in Program.server.playerlist)
            {
                try
                {
                    if (pl != null && pl.loggedIn && pl != p)
                    {
                        pl.SpawnPlayer(p, false);
                    }
                }
                catch { }
            }
        }

        #endregion

        #region Disconnecting
        public void Disconnect(bool silent)
        {
            if (this.disconnected) { return; }
            this.loggedIn = false;
            this.disconnected = true;
            if (!silent)
            {
                GlobalMessage("[ " + Rank.GetColor(rank) + prefix + username + "&e disconnected. ]");
                foreach (Player pl in Program.server.playerlist)
                {
                    if (pl != null && pl.loggedIn)
                    {
                        pl.outputWriter.Write((byte)ServerPacket.PlayerDie);
                        pl.outputWriter.Write(this.id);
                    }
                }
            }
            Program.server.logger.log(username + "(" + ip + ") disconnected.");
            Program.server.playerlist[id] = null;
            if (this.plyClient.Connected) { this.plyClient.Close(); }
        }
        #endregion

        #region Data Handlers

        public static List<string> SplitLines(string message)
        {
            List<string> lines = new List<string>();
            message = Regex.Replace(message, @"(&[0-9a-f])+(&[0-9a-f])", "$2");
            message = Regex.Replace(message, @"(&[0-9a-f])+$", "");
            int limit = 64; string color = "";
            while (message.Length > 0)
            {
                if (lines.Count > 0) { message = "> " + color + message.Trim(); }
                if (message.Length <= limit) { lines.Add(message); break; }
                for (int i = limit - 1; i > limit - 9; --i)
                {
                    if (message[i] == ' ') 
					{
						lines.Add(message.Substring(0, i)); goto Next; 
					}
                } 
				lines.Add(message.Substring(0, limit));
			Next: message = message.Substring(lines[lines.Count - 1].Length);
				if (lines.Count == 1)
				{
					limit = 60;
				}
                int index = lines[lines.Count - 1].LastIndexOf('&');
				if (index != -1)
				{
					if (index < lines[lines.Count - 1].Length - 1)
					{
						char next = lines[lines.Count - 1][index + 1];
						if ("0123456789abcdef".IndexOf(next) != -1) { color = "&" + next; }
						if (index == lines[lines.Count - 1].Length - 1)
						{
							lines[lines.Count - 1] = lines[lines.Count - 1].
								Substring(0, lines[lines.Count - 1].Length - 2);
						}
					}
					else if (message.Length != 0)
					{
						char next = message[0];
						if ("0123456789abcdef".IndexOf(next) != -1)
						{
							color = "&" + next;
						}
						lines[lines.Count - 1] = lines[lines.Count - 1].
							Substring(0, lines[lines.Count - 1].Length - 1);
						message = message.Substring(1);
					}
				}
            } return lines;
        }

        public static byte[] GZip(byte[] bytes)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            GZipStream gs = new GZipStream(ms, CompressionMode.Compress, true);
            gs.Write(bytes, 0, bytes.Length); 
            gs.Close();
            ms.Position = 0; 
            bytes = new byte[ms.Length];
            ms.Read(bytes, 0, (int)ms.Length); 
            ms.Close();
            return bytes;
        }

        public static byte[] byteArraySlice(ref byte[] b, int start, int length)  //Get a piece of a byte[]
        {
            byte[] ret = new byte[length];
            for (int i = start; i < start + length; i++)
            {
                ret[i - start] = b[i];
            }
            return ret;
        }
        #endregion

    }
}