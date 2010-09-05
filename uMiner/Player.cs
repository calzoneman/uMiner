/**
 * uMiner - A lightweight custom Minecraft Classic server written in C#
 * Copyright 2010 Calvin "calzoneman" Montgomery
 * 
 * Licensed under the Creative Commons Attribution-ShareAlike 3.0 Unported License
 * (see http://creativecommons.org/licenses/by-sa/3.0/, or LICENSE.txt for a full license
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Text;

namespace uMiner
{
    public class Player
    {
        public ushort x, y, z;
        public byte rotx, roty;
        public string username;
        public string prefix = "";
        public string ip;
        public Socket socket;
        public byte[] buffer;
        public int readSize = 0;
        public System.Threading.Thread inputThread;
        public bool loggedIn = false;
        public int loginTmr = 0;

        public byte rank = 0x01; //Guest

        public Player(Socket socket)
        {
            this.username = "player";
            this.socket = socket;
            this.buffer = new byte[131];
            this.x = 0;
            this.y = 0;
            this.z = 0;
            this.rotx = 0;
            this.roty = 0;
            this.prefix = "";
            this.ip = socket.RemoteEndPoint.ToString().Substring(0, socket.RemoteEndPoint.ToString().IndexOf(':'));
            this.inputThread = new System.Threading.Thread(new System.Threading.ThreadStart(ScanForInput));
            this.inputThread.Start();
        }

        public void ScanForInput()
        {
            while (true)
            {
                try
                {
                    SendRaw(0x01);
                }
                catch
                {
                    Disconnect();
                }
                if (!loggedIn)
                {
                    loginTmr += 10;
                    if (loginTmr > 1000)
                    {
                        Disconnect();
                    }
                }
                if (socket.Connected)
                {
                    try
                    {
                        if (readSize == 0)
                        {
                            readSize++;
                            socket.Receive(buffer, 1, SocketFlags.None);
                            int length = Protocol.incomingPacketLengths[buffer[0]];
                            readSize += socket.Receive(buffer, 1, length - 1, SocketFlags.None);
                            Process();
                        }
                    }
                    catch (Exception e)
                    {
                        Program.server.logger.log(e);
                    }
                }
                System.Threading.Thread.Sleep(10);
            }
        }

        public void Process()
        {
            if (readSize == 0 || buffer == null)
            {
                Program.server.logger.log("Bad attempt at Player.Process()", Logger.LogType.Warning);
                return;
            }
            switch (buffer[0])
            {
                case 0x00:
                    Login();
                    byte[] fakemap = new byte[16 * 16 * 16];
                    for (int i = 0; i < 256; i++) { fakemap[i] = 0x02; }
                    for (int i = 256; i < fakemap.Length; i++) { fakemap[i] = 0x00; }
                    SendMap(ref fakemap, 16, 16, 16);
                    SendMessage(0xFF, "Welcome to uMiner!");
                    break;
                case 0x08:
                    PositionChange();
                    break;
                case 0xd:
                    PlayerMessage();
                    break;
                default:
                    Program.server.logger.log("Packet handler not implemented for " + buffer[0].ToString(), Logger.LogType.Warning);
                    break;
            }
            readSize = 0;
            buffer = new byte[131];
        }

        #region Received Data
        public void Login()
        {
            try
            {
                if (buffer[1] != Protocol.version)  //Shouldn't happen
                {
                    Program.server.logger.log("Wrong protocol version received from " + ip, Logger.LogType.Warning);
                    Program.server.logger.log("Login packet: " + Encoding.ASCII.GetString(buffer), Logger.LogType.Warning);
                    Kick("Wrong Protocol Version");
                    return;
                }
                //Read username
                this.username = StripExcess(Encoding.ASCII.GetString(byteArraySlice(ref buffer, 2, 64)));
                //There is also an mppass for name verification but we don't care (yet)

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
                byte[] outdata = new byte[130];
                outdata[0] = Protocol.version;  //server version
                Encoding.ASCII.GetBytes(Program.server.serverName).CopyTo(outdata, 1);  //server name
                Encoding.ASCII.GetBytes(Program.server.motd).CopyTo(outdata, 65);  //motd
                outdata[129] = 0x00;  //0x00 = non-admin
                if (rank > 0x20) { outdata[129] = 0x64; } //0x64 = OP+
                SendRaw(0x00, outdata);
                Program.server.logger.log(ip + " logged in as " + username);

                if (Program.server.playerlist.Count > Program.server.maxPlayers) //Do you really?
                {
                    Kick("Server is full!");
                    return;
                }

                if (rank >= 128) { prefix = "+"; }
                if (rank == 255) { prefix = "@"; }

                string loginMessage = "[ " + Rank.GetColor(rank);
                if(!prefix.Equals(""))
                {
                    loginMessage += prefix;
                }
                loginMessage += username + "&e joined the game ]";
                loggedIn = true;
                GlobalMessage(loginMessage);

            }
            catch
            {
                Kick("Error occurred during login");
            }
            //Erm, not much else to do at the moment
            //Kick("Good bye from uMiner!");
            
        }

        public void PositionChange()
        {
            this.x = NTHO(byteArraySlice(ref buffer, 2, 2), 0);
            this.y = NTHO(byteArraySlice(ref buffer, 4, 2), 0);
            this.z = NTHO(byteArraySlice(ref buffer, 6, 2), 0);
            this.rotx = buffer[8];
            this.roty = buffer[9];
        }

        public void PlayerMessage()
        {
            string rawmsg = StripExcess(Encoding.ASCII.GetString(byteArraySlice(ref buffer, 2, 64)));
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
                p.SendMessage((byte)Program.server.playerlist.IndexOf(this), message);
            }

        }
        #endregion

        #region Sending
        public void SendMessage(byte id, string message)
        {
            byte[] buffer = new byte[65];
            unchecked { buffer[0] = id; }
            foreach (string line in SplitLines(message))
            {
                StringFormat(line, 64).CopyTo(buffer, 1);
                SendRaw(0x0d, buffer);
            }
        }

        public void Kick(string reason)  //Disconnect someone
        {
            if (!this.socket.Connected) //Oops
            {
                Program.server.logger.log("Player " + username + " has already disconnected.", Logger.LogType.Warning);
                return;
            }
            //Send kick (0x0e + kick message)
            byte[] outdata = new byte[64];
            Encoding.ASCII.GetBytes(reason).CopyTo(outdata, 0);
            SendRaw(0x0e, outdata);
            this.socket.Disconnect(false);
            Program.server.logger.log("Player " + username + " kicked (" + reason + ")");
            Disconnect();
        }

        public void SendMap(ref byte[] leveldata, ushort width, ushort height, ushort depth)
        {
            this.socket.Send(new byte[] { 0x2 }, SocketFlags.None);
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
                short length = (short)Math.Min(buffer.Length, 1024);
                byte[] send = new byte[1027];
                HTNO(length).CopyTo(send, 0);
                Buffer.BlockCopy(buffer, 0, send, 2, length);
                byte[] tempbuffer = new byte[buffer.Length - length];
                Buffer.BlockCopy(buffer, length, tempbuffer, 0, buffer.Length - length);
                buffer = tempbuffer;
                send[1026] = (byte)(i * 100 / number);
                SendRaw(0x03, send);
                System.Threading.Thread.Sleep(10);
            } buffer = new byte[6];
            HTNO((short)width).CopyTo(buffer, 0);
            HTNO((short)depth).CopyTo(buffer, 2);
            HTNO((short)height).CopyTo(buffer, 4);
            SendRaw(0x04, buffer);
        }
        #endregion

        #region Global Stuff

        public static void GlobalMessage(string message)
        {
            foreach (Player p in Program.server.playerlist)
            {
                try
                {
                    if (p.loggedIn)
                    {
                        p.SendMessage(0xFF, message);
                    }
                }
                catch
                {
                    Program.server.logger.log("Failed to send Global Message to " + p.username, Logger.LogType.Warning);
                    p.Disconnect();
                }
            }
            Program.server.logger.log("(Global) " + message, Logger.LogType.Chat);
        }

        #endregion

        #region Socket I/O
        public void SendRaw(int id)
        {
            SendRaw(id, new byte[0]);
        }
        public void SendRaw(int id, byte[] send)
        {
            byte[] buffer = new byte[send.Length + 1];
            buffer[0] = (byte)id;
            Buffer.BlockCopy(send, 0, buffer, 1, send.Length);
            try
            {
                socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, delegate(IAsyncResult result) { }, null);
            }
            catch (SocketException)
            {
                Disconnect();
            }
        }
        public void Disconnect()
        {
            //GlobalMessage(username + " disconnected.");
            Program.server.logger.log(username + "(" + ip + ") disconnected.");
            Program.server.playerlist.Remove(this);
            this.inputThread.Abort();
        }
        #endregion

        #region Data Handlers
        public string StripExcess(string s)  //Strip excess spaces from the end of a string
        {
            int index = s.Length - 1;
            for (int i = s.Length - 1; i >= 0; i--)
            {
                if (s[i] == (char)0x20)
                {
                    index--;
                }
                else
                {
                    break;
                }
            }
            return s.Substring(0, index + 1);
        }

        public static byte[] StringFormat(string str, int size)
        {
            byte[] bytes = new byte[size];
            bytes = Encoding.ASCII.GetBytes(str.PadRight(size).Substring(0, size));
            return bytes;
        }

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

        public static byte[] byteArraySlice(ref byte[] b, int start, int length)  //Get a piece of a byte[]
        {
            byte[] ret = new byte[length];
            for (int i = start; i < start + length; i++)
            {
                ret[i - start] = b[i];
            }
            return ret;
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

        byte[] HTNO(ushort x)
        {
            byte[] y = BitConverter.GetBytes(x); Array.Reverse(y); return y;
        }
        ushort NTHO(byte[] x, int offset)
        {
            byte[] y = new byte[2];
            Buffer.BlockCopy(x, offset, y, 0, 2); Array.Reverse(y);
            return BitConverter.ToUInt16(y, 0);
        }
        byte[] HTNO(short x)
        {
            byte[] y = BitConverter.GetBytes(x); Array.Reverse(y); return y;
        }
        #endregion

    }
}
