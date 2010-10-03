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
using System.Text;

namespace uMiner
{
    public class DisconnectCommand
    {
        public static void Kick(Player p, string message)
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
                string ply = message.Trim().Substring(0, message.Trim().IndexOf(" "));
                string reason = message.Trim().Substring(message.Trim().IndexOf(" ") + 1);
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

        public static void Ban(Player p, string message)
        {
            if (!message.Trim().Contains(" "))
            {
                if (p.username.ToLower().Equals(message.Trim().ToLower()))
                {
                    p.SendMessage(0xFF, "You can't ban yourself!");
                    return;
                }
                foreach (Player pl in Program.server.playerlist)
                {
                    if (pl != null && pl != p && pl.loggedIn && pl.username.ToLower().Equals(message.Trim().ToLower()) && pl.rank < p.rank)
                    {
                        ChangeRankCommand.Base(p, pl.username, Rank.RankLevel("none"));
                        Player.GlobalMessage(Rank.GetColor(p.rank) + p.prefix + p.username + "&e banned " + pl.username);
                        return;
                    }
                    else if (pl != null && pl.rank >= p.rank && pl.username.ToLower().Equals(message.Trim().ToLower()) && pl != p)
                    {
                        p.SendMessage(0xFF, "You can't ban that person!");
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
                if (p.username.ToLower().Equals(ply.Trim().ToLower()))
                {
                    p.SendMessage(0xFF, "You can't ban yourself!");
                    return;
                }
                foreach (Player pl in Program.server.playerlist)
                {
                    if (pl != null && pl != p && pl.loggedIn && pl.username.ToLower().Equals(ply.Trim().ToLower()) && pl.rank < p.rank)
                    {
                        ChangeRankCommand.Base(p, pl.username, Rank.RankLevel("none"));
                        Player.GlobalMessage(Rank.GetColor(p.rank) + p.prefix + p.username + "&e banned " + pl.username + " (" + reason + ")");
                        return;
                    }
                    else if (pl != null && pl.rank >= p.rank && pl.username.ToLower().Equals(ply.Trim().ToLower()) && pl != p)
                    {
                        p.SendMessage(0xFF, "You can't ban that person!");
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

        public static void Unban(Player p, string message)
        {
            message = message.Trim().ToLower();
            if (!Program.server.playerRanksDict.ContainsKey(message))
            {
                p.SendMessage(0xFF, "Cannot find player " + message);
                return;
            }

            bool found = false;
            foreach (Player pl in Program.server.playerlist)
            {
                if (pl != null && pl != p && pl.username.ToLower().Equals(message))
                {
                    ChangeRankCommand.Base(p, message, Rank.RankLevel("guest"));
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Program.server.playerRanksDict[message] = Rank.RankLevel("guest");
                Program.server.saveRanks();
            }

            Player.GlobalMessage(Rank.GetColor(p.rank) + p.prefix + p.username + "&e unbanned " + message);
        }

        public static void IpBan(Player p, string message)
        {
            if (!message.Trim().Contains(" "))
            {
                if (p.username.ToLower().Equals(message.Trim().ToLower()))
                {
                    p.SendMessage(0xFF, "You can't ipban yourself!");
                    return;
                }
                foreach (Player pl in Program.server.playerlist)
                {
                    if (pl != null && pl != p && pl.loggedIn && !pl.disconnected && pl.username.ToLower().Equals(message.Trim().ToLower()) && pl.rank < p.rank)
                    {
                        ChangeRankCommand.Base(p, pl.username, Rank.RankLevel("none"));
                        Program.server.ipbanned.Add(pl.ip);
                        Program.server.saveIpBans();
                        Player.GlobalMessage(Rank.GetColor(p.rank) + p.prefix + p.username + "&e ipbanned " + pl.username);
                        return;
                    }
                    else if (pl != null && pl.rank >= p.rank && pl.username.ToLower().Equals(message.Trim().ToLower()) && pl != p)
                    {
                        p.SendMessage(0xFF, "You can't ipban that person!");
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
                if (p.username.ToLower().Equals(message.Trim().ToLower()))
                {
                    p.SendMessage(0xFF, "You can't ipban yourself!");
                    return;
                }
                foreach (Player pl in Program.server.playerlist)
                {
                    if (pl != null && pl != p && pl.loggedIn && !pl.disconnected && pl.username.ToLower().Equals(ply.Trim().ToLower()) && pl.rank < p.rank)
                    {
                        ChangeRankCommand.Base(p, pl.username, Rank.RankLevel("none"));
                        Program.server.ipbanned.Add(pl.ip);
                        Program.server.saveIpBans();
                        Player.GlobalMessage(Rank.GetColor(p.rank) + p.prefix + p.username + "&e ipbanned " + pl.username + " (" + reason + ")");
                        return;
                    }
                    else if (pl != null && pl.rank >= p.rank && pl.username.ToLower().Equals(ply.Trim().ToLower()) && pl != p)
                    {
                        p.SendMessage(0xFF, "You can't ipban that person!");
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

        public static void UnbanIp(Player p, string message)
        {
            message = message.Trim();
            if (!Program.server.ipbanned.Contains(message))
            {
                p.SendMessage(0xFF, "Cannot find ip " + message);
                return;
            }

            Program.server.ipbanned.Remove(message);

            foreach (Player pl in Program.server.playerlist)
            {
                if (pl != null && pl != p && pl.ip.Equals(message))
                {
                    ChangeRankCommand.Base(p, pl.username, Rank.RankLevel("guest"));
                    break;
                }
            }

            Player.GlobalMessage(Rank.GetColor(p.rank) + p.prefix + p.username + "&e unipbanned " + message);
        }

        public static void Help(Player p, string cmd)
        {
            switch (cmd)
            {
                case "ban":
                    p.SendMessage(0xFF, "/ban player - Removes player's priveleges");
                    p.SendMessage(0xFF, "/ban player reason - Bans player with reason");
                    break;
                case "unban":
                    p.SendMessage(0xFF, "/unban player - Removes the ban on player");
                    break;
                case "ipban":
                    p.SendMessage(0xFF, "/ipban player - Bans player's ip");
                    p.SendMessage(0xFF, "/ipban player reason - Bans player's ip with reason");
                    break;
                case "unipban":
                    p.SendMessage(0xFF, "/unipban ip - Removes the ipban on ip");
                    break;
                case "kick":
                    p.SendMessage(0xFF, "/kick player - Disconnects a player");
                    p.SendMessage(0xFF, "/kick player reason - Disconnects a player with reason");
                    break;
                default:
                    break;
            }
        }
    }
}
