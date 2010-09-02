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
using System.Net.Sockets;
using System.Text;

namespace uMiner
{
    public class Player
    {
        public short x, y, z;
        public byte rotx, roty;
        public string username;
        public string prefix;
        public string ip;
        public Socket socket;
        public byte[] buffer;
        public int readSize = 0;

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
                    break;
                default:
                    Program.server.logger.log("Packet handler not implemented for " + buffer[0].ToString(), Logger.LogType.Warning);
                    break;
            }
        }

        public void Login()
        {
            if (buffer[1] != Protocol.version)  //Shouldn't happen
            {
                Program.server.logger.log("Wrong protocol version received from " + ip, Logger.LogType.Warning);
                Kick("Wrong Protocol Version");
            }
            //Read username
            this.username = StripExcess(Encoding.ASCII.GetString(byteArraySlice(ref buffer, 2, 64)));
            //There is also an mppass for name verification but we don't care (yet)

            //Send a response
            byte[] outdata = new byte[131];
            outdata[0] = 0x00;  //0x00 = handshake
            outdata[1] = Protocol.version;  //server version
            Encoding.ASCII.GetBytes(Program.server.serverName).CopyTo(outdata, 2);  //server name
            Encoding.ASCII.GetBytes(Program.server.motd).CopyTo(outdata, 66);  //motd
            outdata[130] = 0x00;  //0x00 = non-admin (for now this can be constant)
            this.socket.Send(outdata, SocketFlags.None);
            Program.server.logger.log(ip + " logged in as " + username);

            if (Program.server.playerlist.Count > Program.server.maxPlayers) //Do you really?
            {
                Kick("Server is full!");
                return;
            }

            //Erm, not much else to do at the moment
            Kick("Good bye from uMiner!");
            
        }

        public void Kick(string reason)  //Disconnect someone
        {
            if (!this.socket.Connected) //Oops
            {
                Program.server.logger.log("Player " + username + " has already disconnected.", Logger.LogType.Warning);
                return;
            }
            //Send kick (0x0e + kick message)
            byte[] outdata = new byte[65];
            outdata[0] = 0x0e;
            Encoding.ASCII.GetBytes(reason).CopyTo(outdata, 1);
            this.socket.Send(outdata, SocketFlags.None);
            this.socket.Disconnect(false);
            //Get rid of them from the playerlist
            Program.server.playerlist.Remove(this);
            Program.server.logger.log("Player " + username + " kicked (" + reason + ")");
        }

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

        byte[] byteArraySlice(ref byte[] b, int start, int length)  //Get a piece of a byte[]
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
