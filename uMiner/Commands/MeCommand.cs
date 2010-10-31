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
    public class MeCommand
    {
        public static void Me(Player p, string message)
        {
            if (!message.Trim().Equals(""))
            {
                Player.GlobalMessage("* " + p.GetFormattedName() + "&e " + message);
            }
        }

        public static void Help(Player p)
        {
            p.SendMessage(0xFF, "/me action - Displays a roleplaying action");
        }
    }
}
