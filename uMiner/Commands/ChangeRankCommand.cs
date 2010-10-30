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

        public static void Owner(Player p, string message)
        {
            Base(p, message, Rank.RankLevel("owner"));
        }

        public static void Base(Player p, string message, byte newrank)
        {
            if (message.Trim().Equals(""))
            {
                p.SendMessage(0xFF, "No player specified");
                return;
            }

            Player pl = uMiner.Player.FindPlayer(p, message, false);
            if (pl != null)
            {
                if (pl.username.Equals(p.username))
                {
                    p.SendMessage(0xFF, "You can't change your own rank!");
                }
                if (pl.rank < newrank || p.rank > pl.rank || p.username.Equals("[console]"))
                {
                    byte oldRank = pl.rank;
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
                    else if (newrank == Rank.RankLevel("owner"))
                    {
                        pl.prefix = "@";
                    }

                    //Clear the player's binding as a safeguard
                    pl.binding = Bindings.None;
                    
                    //If the person was OP before, disable adminium editing
                    //Vice versa as well
                    if (oldRank >= Rank.RankLevel("operator") && newrank < Rank.RankLevel("operator"))
                    {
                        Packet deop = new Packet(2);
                        deop.Append((byte)ServerPacket.RankUpdate);
                        deop.Append((byte)0x0);
                        pl.SendPacket(deop);
                    }
                    else if (oldRank < Rank.RankLevel("operator") && newrank >= Rank.RankLevel("operator"))
                    {
                        Packet deop = new Packet(2);
                        deop.Append((byte)ServerPacket.RankUpdate);
                        deop.Append((byte)0x64);
                        pl.SendPacket(deop);
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
                    uMiner.Player.GlobalMessage(p.GetFormattedName() + "&e set " + pl.GetFormattedName() + "&e's rank to " + Rank.GetColor(newrank) + Rank.RankName(newrank) + "&e");
                }
                else
                {
                    p.SendMessage(0xFF, "Player " + pl.GetFormattedName() + "&e cannot have rank set to " + Rank.GetColor(newrank) + Rank.RankName(newrank));
                }
                return;
            }
        }

        public static void Help(Player p, string cmd)
        {
            p.SendMessage(0xFF, "/" + cmd + " player - set player's rank to " + cmd);
        }
    }
}
