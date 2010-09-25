using System;
using System.Collections.Generic;
using System.Text;

namespace uMiner
{
    public class WhereCommand
    {
        public static void Execute(Player p, string message)
        {
            if (message.Equals(""))
            {
                p.SendMessage(0xFF, String.Format("X: {0}, Y: {1}, Z:{2}", p.x/32, p.y/32, p.z/32));
            }
            else
            {
                bool found = false;
                foreach (Player pl in Program.server.playerlist)
                {
                    if (pl != null && pl.loggedIn && !pl.disconnected && pl.username.ToLower().Equals(message.Trim().ToLower()))
                    {
                        p.SendMessage(0xFF, String.Format("-> " + Rank.GetColor(pl.rank) + pl.prefix + pl.username + "&e X: {0}, Y: {1}, Z: {2}", pl.x/32, pl.y/32, pl.z/32));
                        return;
                    }
                    else if (pl != null && pl.loggedIn && !pl.disconnected && pl.username.Substring(0, message.Length).ToLower().Equals(message.ToLower().Trim()))
                    {
                        p.SendMessage(0xFF, String.Format("-> " + Rank.GetColor(pl.rank) + pl.prefix + pl.username + "&e X: {0}, Y: {1}, Z: {2}", pl.x / 32, pl.y / 32, pl.z / 32));
                        found = true;
                    }
                }
                if (!found)
                {
                    p.SendMessage(0xFF, "Could not find player " + message);
                }
            }
        }
    }
}
