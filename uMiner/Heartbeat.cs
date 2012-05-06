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
                    string args = "port=" + Program.server.port + "&max=" + Program.server.maxPlayers + "&name=" + Uri.EscapeUriString(Program.server.serverName) + "&public=" + Program.server.isPublic.ToString() + "&version=" + Protocol.version + "&salt=" + Program.server.salt + "&users=" + Program.server.plyCount;
                    HttpWebRequest beatRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://minecraft.net/heartbeat.jsp?" + args));
                    beatRequest.Method = "GET";
                    beatRequest.ContentType = "application/x-www-form-urlencoded";
                    beatRequest.ContentLength = args.Length;
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
                catch (WebException)
                {
                    Program.server.logger.log("Unable to make heartbeat", Logger.LogType.Warning);
                    if (initial)
                    {
                        Program.server.verify_names = false;
                        Program.server.logger.log("Initial heartbeat failed.  Turning verify-names off");
                    }
                    return false;
                }
                catch (Exception e)
                {
                    Program.server.logger.log("Error occurred during heartbeat: ", Logger.LogType.Error);
                    Program.server.logger.log(e);
                    if (initial)
                    {
                        Program.server.verify_names = true;
                        Program.server.logger.log("Initial heartbeat failed.  Turning verify-names off");
                    }
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
