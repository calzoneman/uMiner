/**
 * uMiner - A lightweight custom Minecraft Classic server written in C#
 * Copyright 2010 Calvin "calzoneman" Montgomery
 * 
 * Licensed under the Creative Commons Attribution-ShareAlike 3.0 Unported License
 * (see http://creativecommons.org/licenses/by-sa/3.0/, or LICENSE.txt for a full license
 */


using System;
using System.Collections.Generic;
using System.Text;

namespace uMiner
{
    public class DisconnectCommand
    {
        public static void ExecuteKick(Player p, string message)
        {
            if (!message.Trim().Contains(" "))
            {
                foreach (Player pl in Program.server.playerlist)
                {
                    if (pl != null && pl.loggedIn && !pl.disconnected && pl.username.ToLower().Equals(message.Trim().ToLower()) && pl.rank <= p.rank)
                    {
                        pl.Kick("Kicked by " + p.username, false);
                        return;
                    }
                    else if (pl != null && p.loggedIn && !pl.disconnected && pl.username.Substring(0, message.Length).ToLower().Equals(message.ToLower().Trim()))
                    {
                        p.SendMessage(0xFF, "->" + Rank.GetColor(pl.rank) + pl.prefix + pl.username);
                    }
                }
                p.SendMessage(0xFF, "Could not find player " + message);
            }
            else
            {
                string ply = message.Trim().Substring(0, message.IndexOf(" "));
                string reason = message.Trim().Substring(message.IndexOf(" ") + 1);
                foreach (Player pl in Program.server.playerlist)
                {
                    if (pl != null && pl.loggedIn && !pl.disconnected && pl.username.ToLower().Equals(ply.Trim().ToLower()) && pl.rank <= p.rank)
                    {
                        pl.Kick(reason, false);
                        return;
                    }
                    else if (pl != null && p.loggedIn && !pl.disconnected && pl.username.Substring(0, ply.Length).ToLower().Equals(ply.ToLower().Trim()))
                    {
                        p.SendMessage(0xFF, "->" + Rank.GetColor(pl.rank) + pl.prefix + pl.username);
                    }
                }
                p.SendMessage(0xFF, "Could not find player " + message);
            }
        }

    }
}
