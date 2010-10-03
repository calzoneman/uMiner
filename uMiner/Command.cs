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
    public class Command
    {
        public delegate void CommandHandler(Player p, string message);
        public static Dictionary<string, Command> commands = new Dictionary<string, Command>();

        public static void Init()
        {
            commands.Add("ban", new Command(DisconnectCommand.Ban, Rank.RankLevel("operator")));
            commands.Add("colors", new Command(ColorCommand.Colors, Rank.RankLevel("operator")));
            commands.Add("fetch", new Command(TeleportCommand.Fetch, Rank.RankLevel("operator")));
            commands.Add("guest", new Command(ChangeRankCommand.Guest, Rank.RankLevel("operator")));
            commands.Add("help", new Command(HelpCommand.Help, Rank.RankLevel("guest")));
            commands.Add("ipban", new Command(DisconnectCommand.IpBan, Rank.RankLevel("operator")));
            commands.Add("kick", new Command(DisconnectCommand.Kick, Rank.RankLevel("operator")));
            commands.Add("newworld", new Command(WorldCommand.NewWorld, Rank.RankLevel("owner")));
            commands.Add("operator", new Command(ChangeRankCommand.Operator, Rank.RankLevel("owner")));
            commands.Add("place", new Command(PlaceCommand.Place, Rank.RankLevel("player")));
            commands.Add("player", new Command(ChangeRankCommand.Player, Rank.RankLevel("operator")));
            commands.Add("ranks", new Command(HelpCommand.Ranks, Rank.RankLevel("guest")));
            commands.Add("say", new Command(SayCommand.Say, Rank.RankLevel("operator")));
            commands.Add("tp", new Command(TeleportCommand.Tp, Rank.RankLevel("player")));
            commands.Add("unban", new Command(DisconnectCommand.Unban, Rank.RankLevel("operator")));
            commands.Add("unipban", new Command(DisconnectCommand.UnbanIp, Rank.RankLevel("operator")));
            commands.Add("where", new Command(WhereCommand.Where, Rank.RankLevel("player")));
        }

        public static void HandleCommand(Player p, string cmd, string msg)
        {
            if(commands.ContainsKey(cmd))
            {
                if(p.rank < commands[cmd].minRank)
                {
                    p.SendMessage(0xFF, "You can't use that command!");
                    return;
                }
                commands[cmd].handler(p, msg);
                Program.server.logger.log(p.username + " uses /" + cmd);
            }
            else
            {
                p.SendMessage(0xFF, "No such command &c/" + cmd);
            }
        }

        public static void HelpMessage(Player p, string cmd)
        {
            switch (cmd)
            {
                case "colors":
                    ColorCommand.Help(p, cmd);
                    break;
                case "fetch":
                case "tp":
                    TeleportCommand.Help(p, cmd);
                    break;
                case "guest":
                case "player":
                case "operator":
                    ChangeRankCommand.Help(p, cmd);
                    break;
                case "help":
                    p.SendMessage(0xFF, "/help - Displays a help menu");
                    break;
                case "ranks":
                    p.SendMessage(0xFF, "/ranks - Displays information regarding ranks");
                    break;
                case "kick":
                case "ban":
                case "ipban":
                case "unban":
                case "unipban":
                    DisconnectCommand.Help(p, cmd);
                    break;
                case "newworld":
                    WorldCommand.Help(p, "newworld");
                    break;
                case "place":
                    PlaceCommand.Help(p);
                    break;
                case "say":
                    SayCommand.Help(p);
                    break;
                case "where":
                    WhereCommand.Help(p);
                    break;
                default:
                    break;
            }
        }

        public CommandHandler handler;
        public byte minRank;

        public Command(CommandHandler handler, byte minRank)
        {
            this.handler = handler;
            this.minRank = minRank;
        }

    }
}
