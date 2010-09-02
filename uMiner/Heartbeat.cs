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
using System.Net.Sockets;
using System.Text;

namespace uMiner
{
    public class Heartbeat
    {
        BeatType beatType;
        public Heartbeat(BeatType bType)
        {
            this.beatType = bType;
        }
        public bool Beat(bool initial)
        {
            if (beatType == BeatType.Minecraft)
            {
                try
                {
                    HttpWebRequest beatRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://www.minecraft.net/heartbeat.jsp"));
                    string args = "port=" + Program.server.port + "&max=" + Program.server.maxPlayers + "&name=" + Uri.EscapeUriString(Program.server.serverName) + "&public=True" + "&version=" + Protocol.version + "&salt=aaaaaaaaaaaaaaaa&users=" + Program.server.plyCount;
                    beatRequest.Method = "POST";
                    beatRequest.ContentType = "application/x-www-form-urlencoded";
                    beatRequest.ContentLength = args.Length;
                    using (Stream rStream = beatRequest.GetRequestStream())
                    {
                        rStream.Write(Encoding.ASCII.GetBytes(args), 0, args.Length);
                    }
                    using (Stream responseStream = beatRequest.GetResponse().GetResponseStream())
                    {
                        byte[] responseBytes = new byte[73]; //URL should be 73 characters long
                        responseStream.Read(responseBytes, 0, 73);
                        if (initial)
                        {
                            Program.server.logger.log("Received URL: " + Encoding.ASCII.GetString(responseBytes));
                            StreamWriter fileWriter = new StreamWriter(File.OpenWrite("externalurl.txt"));
                            fileWriter.Write(Encoding.ASCII.GetString(responseBytes));
                            fileWriter.Close();
                        }
                    }
                }
                catch(Exception e)
                {
                    Program.server.logger.log("Error occurred during heartbeat: ", Logger.LogType.Error);
                    Program.server.logger.log(e);
                    return false;
                }
                return true;

            }
            return false;
        }

    }

    public enum BeatType
    {
        Minecraft
    }
}
