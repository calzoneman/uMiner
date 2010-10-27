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
    public class ColorCommand
    {
        public static void Colors(Player p, string message)
        {
            p.SendMessage(0xFF, "Color codes: ");
            p.SendMessage(0, "&0%0 &1%1 &2%2 &3%3 &4%4 &5%5 &6%6 &7%7");
            p.SendMessage(0, "&8%8 &9%9 &a%a &b%b &c%c &d%d &e%e &f%f");
        }

        public static void Chars(Player p, string message)
        {
            p.SendMessage(0xFF, "Character Codes: ");
            string temp = String.Empty;
            foreach (KeyValuePair<string, char> rule in Player.specialChars)
            {
                temp += (("&c" + rule.Key + "&e ").PadRight(8) + rule.Value).PadRight(15);
                if (temp.Length >= 60)
                {
                    p.SendMessage(0x0, temp);
                    temp = String.Empty;
                }
            }
            p.SendMessage(0x0, temp);
            //p.SendMessage(0x0, @"&c\# &e Displays the character with byte id #");
        }

        public static void Help(Player p, string cmd)
        {
            switch (cmd)
            {
                case "colors":
                    p.SendMessage(0xFF, "/colors - displays information about color codes");
                    break;
                case "chars":
                    p.SendMessage(0xFF, "/chars - displays information about special characters");
                    break;
                default:
                    break;
            }
        }
    }
}
