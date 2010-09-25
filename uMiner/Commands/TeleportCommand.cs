using System;
using System.Collections.Generic;
using System.Text;

namespace uMiner
{
    public class TeleportCommand
    {
        public static void ExecuteTp(Player p, string message)
        {
            if (message.Trim().Equals(""))
            {
                p.SendMessage(0xFF, "No username specified");
                return;
            }
            foreach (Player pl in Program.server.playerlist)
            {
                if (pl != null && pl.loggedIn && !pl.disconnected && pl.username.ToLower().Equals(message.Trim().ToLower()))
                {
                    p.SendSpawn(new short[3] { pl.x, pl.y, pl.z }, new byte[2] { 0, 0 });
                    p.SendMessage(0xFF, "Teleported to " + Rank.GetColor(pl.rank) + pl.prefix + pl.username);
                    return;
                }
                else if (pl != null && pl.loggedIn && !pl.disconnected && pl.username.Substring(0, message.Length).ToLower().Equals(message.ToLower().Trim()))
                {
                    p.SendSpawn(new short[3] { pl.x, pl.y, pl.z }, new byte[2] { 0, 0 });
                    p.SendMessage(0xFF, "Teleported to " + Rank.GetColor(pl.rank) + pl.prefix + pl.username);
                    return;
                }
            }
            p.SendMessage(0xFF, "Could not find player " + message);
        }

        public static void ExecuteFetch(Player p, string message)
        {
            if (message.Trim().Equals(""))
            {
                p.SendMessage(0xFF, "No username specified");
                return;
            }
            foreach (Player pl in Program.server.playerlist)
            {
                if (pl != null && pl.loggedIn && !pl.disconnected && pl.username.ToLower().Equals(message.Trim().ToLower()))
                {
                    pl.SendSpawn(new short[3] { p.x, p.y, p.z }, new byte[2] { 0, 0 });
                    pl.SendMessage(0xFF, "Fetched by " + Rank.GetColor(p.rank) + p.prefix + p.username);
                    return;
                }
                else if (pl != null && pl.loggedIn && !pl.disconnected && pl.username.Substring(0, message.Length).ToLower().Equals(message.ToLower().Trim()))
                {
                    pl.SendSpawn(new short[3] { p.x, p.y, p.z }, new byte[2] { 0, 0 });
                    pl.SendMessage(0xFF, "Fetched by " + Rank.GetColor(p.rank) + p.prefix + p.username);
                    return;
                }
            }
            p.SendMessage(0xFF, "Could not find player " + message);
        }
    }
}
