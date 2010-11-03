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
                Player pl = Player.FindPlayer(p, message.Trim(), false);
                if (pl != null && (pl.rank <= p.rank || p.username.Equals("[console]")))
                {
                    pl.Kick("Kicked by " + p.username + "&e", false);
                }
                else if(pl != null && pl.rank > p.rank)
                {
                    p.SendMessage(0xFF, "You can't kick that person!");
                }
                return;
            }
            else
            {
                string ply = message.Trim().Substring(0, message.Trim().IndexOf(" "));
                string reason = message.Trim().Substring(message.Trim().IndexOf(" ") + 1);
                Player pl = Player.FindPlayer(p, ply, false);
                if (pl != null && (pl.rank <= p.rank || p.username.Equals("[console]")))
                {
                    pl.Kick(reason, false);
                }
                else if(pl.rank > p.rank)
                {
                    p.SendMessage(0xFF, "You can't kick that person!");
                }
                return;
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
                Player pl = Player.FindPlayer(p, message.Trim(), false);
                if (pl != null && (pl.rank < p.rank || p.username.Equals("[console]")))
                {
                    ChangeRankCommand.Base(p, pl.username, Rank.RankLevel("none"));
                    Player.GlobalMessage(p.GetFormattedName() + "&e banned " + pl.username);
                    return;
                }
                else if (pl != null && pl.rank >= p.rank)
                {
                    p.SendMessage(0xFF, "You can't ban that person!");
                }
                else if (pl == null && Program.server.playerRanksDict.ContainsKey(message.Trim().ToLower()))
                {
                    Program.server.playerRanksDict[message.Trim().ToLower()] = Rank.RankLevel("none");
                    Program.server.saveRanks();
                    Player.GlobalMessage(p.GetFormattedName() + "&e banned " + message.Trim() + "&f(offline)&e");
                }
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
                Player pl = Player.FindPlayer(p, ply, false);
                if (pl != null && (pl.rank < p.rank || p.username.Equals("[console]")))
                {
                    ChangeRankCommand.Base(p, pl.username, Rank.RankLevel("none"));
                    Player.GlobalMessage(p.GetFormattedName() + "&e banned " + pl.username + " (" + reason + ")");
                }
                else if (pl.rank >= p.rank)
                {
                    p.SendMessage(0xFF, "You can't ban that person!");
                }
            }
        }

        public static void Unban(Player p, string message)
        {
            message = message.Trim().ToLower();

            Player pl = Player.FindPlayer(p, message.Trim().ToLower(), false);
            if(pl != null)
            {
                ChangeRankCommand.Base(p, message, Rank.RankLevel("guest"));
            }

            else
            {
                if (!Program.server.playerRanksDict.ContainsKey(message))
                {
                    p.SendMessage(0xFF, "Cannot find player " + message);
                    return;
                }
                Program.server.playerRanksDict[message] = Rank.RankLevel("guest");
                Program.server.saveRanks();
            }

            Player.GlobalMessage(p.GetFormattedName() + "&e unbanned " + message);
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
                Player pl = Player.FindPlayer(p, message.Trim(), false);
                if (pl != null && (pl.rank < p.rank || p.username.Equals("[console]")))
                {
                    ChangeRankCommand.Base(p, pl.username, Rank.RankLevel("none"));
                    Program.server.ipbanned.Add(pl.ip);
                    Program.server.saveIpBans();
                    Player.GlobalMessage(p.GetFormattedName() + "&e ipbanned " + pl.username);
                    return;
                }
                else if (pl != null && pl.rank >= p.rank)
                {
                    p.SendMessage(0xFF, "You can't IPBan that person!");
                }
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
                Player pl = Player.FindPlayer(p, ply, false);
                if (pl != null && (pl.rank < p.rank || p.username.Equals("[console]")) && !pl.ip.Equals("127.0.0.1"))
                {
                    ChangeRankCommand.Base(p, pl.username, Rank.RankLevel("none"));
                    Program.server.ipbanned.Add(pl.ip);
                    Program.server.saveIpBans();
                    Player.GlobalMessage(p.GetFormattedName() + "&e ipbanned " + pl.username + " (" + reason + ")");
                    return;
                }
                else if (pl.rank >= p.rank || pl.ip.Equals("127.0.0.1"))
                {
                    p.SendMessage(0xFF, "You can't IPBan that person!");
                }
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
            Program.server.saveIpBans();

            foreach (Player pl in Program.server.playerlist)
            {
                if (pl != null && pl != p && pl.ip.Equals(message))
                {
                    ChangeRankCommand.Base(p, pl.username, Rank.RankLevel("guest"));
                    break;
                }
            }

            Player.GlobalMessage(p.GetFormattedName() + "&e unipbanned " + message);
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
