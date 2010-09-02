using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace uMiner
{
    class Heartbeat
    {
        BeatType beatType;
        Server server;
        public Heartbeat(BeatType bType, ref Server serv)
        {
            this.beatType = bType;
            this.server = serv;
        }
        public bool Beat()
        {
            if (beatType == BeatType.Minecraft)
            {
                try
                {
                    HttpWebRequest beatRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://www.minecraft.net/heartbeat.jsp"));
                    string args = "port=" + server.port + "&max=" + server.maxPlayers + "&name=" + Uri.EscapeUriString(server.serverName) + "&public=True" + "&version=" + Protocol.version + "&salt=aaaaaaaaaaaaaaaa&users=" + server.plyCount;
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
                        Console.WriteLine("Received URL: " + Encoding.ASCII.GetString(responseBytes));
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine("Error occurred during heartbeat: ");
                    Console.WriteLine(e.Message + "\n"  + e.StackTrace);
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
