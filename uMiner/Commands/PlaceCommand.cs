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
    public class PlaceCommand
    {
        public static void Place(Player p, string message)
        {
            switch (message.Trim())
            {
                case "off":
                case "none":
                    p.binding = Bindings.None;
                    p.SendMessage(0xFF, "Binding set to &fnone");
                    break;
                case "grass":
                    p.binding = Bindings.Grass;
                    p.SendMessage(0xFF, "Binding set to &agrass");
                    break;
                case "admin":
                case "adminium":
                case "admincrete":
                    if (p.rank >= Rank.RankLevel("operator"))
                    {
                        p.binding = Bindings.Adminium;
                        p.SendMessage(0xFF, "Binding set to &0adminium");
                    }
                    else
                    {
                        p.SendMessage(0xFF, "You are not allowed to use that binding!");
                    }
                    break;
                case "sw":
                case "safewater":
                    p.binding = Bindings.SafeWater;
                    p.SendMessage(0xFF, "Binding set to &9safewater");
                    break;
                case "aw":
                case "activewater":
                    if (p.rank >= Rank.RankLevel("operator"))
                    {
                        p.binding = Bindings.ActiveWater;
                        p.SendMessage(0xFF, "Binding set to &1activewater");
                    }
                    else
                    {
                        p.SendMessage(0xFF, "You are not allowed to use that binding!");
                    }
                    break;
                case "sl":
                case "safelava":
                    p.binding = Bindings.SafeLava;
                    p.SendMessage(0xFF, "Binding set to &csafelava");
                    break;
                case "al":
                case "activelava":
                    if (p.rank >= Rank.RankLevel("operator"))
                    {
                        p.binding = Bindings.ActiveLava;
                        p.SendMessage(0xFF, "Binding set to &4activelava");
                    }
                    else
                    {
                        p.SendMessage(0xFF, "You are not allowed to use that binding!");
                    }
                    break;
                default:
                    Help(p);
                    break;
            }
        }


        public static void Help(Player p)
        {
            p.SendMessage(0xFF, "/place binding - Binds stone to binding");
            p.SendMessage(0xFF, "-> Bindings in () are alternatives that do the same thing");
            string available = "&9safewater(sw)&e, &csafelava(sl)&e, &fnone(off)&e, &agrass";
            if (p.rank >= Rank.RankLevel("operator"))
            {
                available += "&e, &0admin(adminium, admincrete)&e, &1activewater(aw)&e, &4activelava(al)";
            }
            p.SendMessage(0xFF, "-> Available bindings: " + available);
        }
    }


    public enum Bindings
    {
        None = 1,
        Grass = 2,
        Adminium = 7,
        ActiveWater = 8,
        SafeWater = 9,
        ActiveLava = 10,
        SafeLava = 11
    }
}
