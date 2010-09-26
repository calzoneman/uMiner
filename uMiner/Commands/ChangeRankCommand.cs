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
    public class ChangeRankCommand
    {
        public static void Guest(Player p, string message)
        {
            Base(p, message, Rank.RankLevel("guest"));
        }

        public static void Player(Player p, string message)
        {
            Base(p, message, Rank.RankLevel("player"));
        }

        public static void Operator(Player p, string message)
        {
            Base(p, message, Rank.RankLevel("operator"));
        }

        public static void Base(Player p, string message, byte newrank)
        {
            if (message.Trim().Equals(""))
            {
                p.SendMessage(0xFF, "No player specified");
                return;
            }

            foreach (Player pl in Program.server.playerlist)
            {
                if (pl != null && pl.loggedIn && !pl.disconnected && pl.username.ToLower().Equals(message.Trim().ToLower()))
                {
                    if (pl.username.Equals(p.username))
                    {
                        p.SendMessage(0xFF, "You can't change your own rank!");
                    }
                    if (pl.rank < newrank || p.rank > pl.rank)
                    {
                        pl.rank = newrank;
                        Program.server.playerRanksDict[pl.username.ToLower()] = pl.rank;
                        Program.server.saveRanks();

                        if (newrank == 0)
                        {
                            pl.prefix = "[:(]";
                        }
                        else if (newrank < Rank.RankLevel("operator"))
                        {
                            pl.prefix = "";
                        }
                        else if (newrank == Rank.RankLevel("operator"))
                        {
                            pl.prefix = "+";
                        }

                        if (newrank < Rank.RankLevel("player"))
                        {
                            pl.binding = Bindings.None;
                        }

                        //Despawn and respawn player
                        Packet despawn = new Packet(2);
                        despawn.Append((byte)ServerPacket.PlayerDie);
                        despawn.Append(pl.id);
                        foreach (Player ply in Program.server.playerlist)
                        {
                            if (ply != null && ply != pl && ply.loggedIn && !ply.disconnected)
                            {
                                ply.SendPacket(despawn);
                            }
                        }
                        uMiner.Player.GlobalSpawnPlayer(pl);
                        uMiner.Player.GlobalMessage(Rank.GetColor(p.rank) + p.prefix + p.username + "&e set " + Rank.GetColor(pl.rank) + pl.prefix + pl.username + "&e's rank to " + Rank.GetColor(newrank) + Rank.RankName(newrank) + "&e");
                    }
                    else
                    {
                        p.SendMessage(0xFF, "Player " + Rank.GetColor(pl.rank) + pl.prefix + pl.username + "&e cannot have rank set to " + Rank.GetColor(newrank) + Rank.RankName(newrank));
                    }
                    return;
                }
                else if (pl != null && pl.loggedIn && !pl.disconnected && pl.username.Substring(0, message.Length).ToLower().Equals(message.ToLower().Trim()))
                {
                    p.SendMessage(0xFF, "-> " + Rank.GetColor(pl.rank) + pl.prefix + pl.username);
                }
            }

            p.SendMessage(0xFF, "Could not find player " + message);
        }

        public static void Help(Player p, string cmd)
        {
            p.SendMessage(0xFF, "/" + cmd + " player - set player's rank to " + cmd);
        }
    }
}
