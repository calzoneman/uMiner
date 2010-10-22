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
    public class WhoCommand
    {
        public static void Who(Player p, string message)
        {
            Player pl = Player.FindPlayer(p, message.Trim(), true);
            if (pl != null)
            {
                StringBuilder msg = new StringBuilder();
                msg.Append(Rank.GetColor(pl.rank) + pl.prefix + pl.username);
                msg.Append("&e is ranked " + Rank.GetColor(pl.rank) + Rank.RankName(pl.rank));
                msg.Append("&e and is connected from IP &b" + pl.ip);
                p.SendMessage(0x00, msg.ToString());
            }
            else
            {
                p.SendMessage(0xFF, "Command failed (could not find player)");
            }
        }

        public static void Help(Player p)
        {
            p.SendMessage(0xFF, "/who player - Displays information about player");
        }
    }
}
