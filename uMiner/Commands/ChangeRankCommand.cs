using System;
using System.Collections.Generic;
using System.Text;

namespace uMiner
{
    public class ChangeRankCommand
    {
        public static void ExecuteGuest(Player p, string message)
        {
            ExecuteBase(p, message, Rank.RankLevel("guest"));
        }

        public static void ExecutePlayer(Player p, string message)
        {
            ExecuteBase(p, message, Rank.RankLevel("player"));
        }

        public static void ExecuteOperator(Player p, string message)
        {
            ExecuteBase(p, message, Rank.RankLevel("operator"));
        }

        public static void ExecuteBase(Player p, string message, byte newrank)
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
                        Player.GlobalSpawnPlayer(pl);
                        Player.GlobalMessage(Rank.GetColor(p.rank) + p.prefix + p.username + "&e set " + Rank.GetColor(pl.rank) + pl.prefix + pl.username + "&e's rank to " + Rank.GetColor(newrank) + Rank.RankName(newrank) + "&e");
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
    }
}
