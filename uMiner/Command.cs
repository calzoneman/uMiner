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
            commands.Add("fetch", new Command(TeleportCommand.ExecuteFetch, Rank.RankLevel("operator")));
            commands.Add("guest", new Command(ChangeRankCommand.ExecuteGuest, Rank.RankLevel("operator")));
            commands.Add("help", new Command(HelpCommand.Execute, Rank.RankLevel("guest")));
            commands.Add("kick", new Command(DisconnectCommand.ExecuteKick, Rank.RankLevel("operator")));
            commands.Add("operator", new Command(ChangeRankCommand.ExecuteOperator, Rank.RankLevel("owner")));
            commands.Add("player", new Command(ChangeRankCommand.ExecutePlayer, Rank.RankLevel("operator")));
            commands.Add("say", new Command(SayCommand.Execute, Rank.RankLevel("operator")));
            commands.Add("tp", new Command(TeleportCommand.ExecuteTp, Rank.RankLevel("player")));
            commands.Add("where", new Command(WhereCommand.Execute, Rank.RankLevel("player")));
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
            string message = "";
            switch (cmd)
            {
                case "fetch":
                    message = "/fetch player - Brings player to your location";
                    break;
                case "guest":
                    message = "/guest player - Sets player's rank to guest (if they are lower ranked than you)";
                    break;
                case "help":
                    message = "/help [command] - Displays a help menu [about command]";
                    break;
                case "kick":
                    message = "/kick player [reason] - Disconnects player [with reason]";
                    break;
                case "operator":
                    message = "/operator player - Sets player's rank to operator (if they are lower ranked than you)";
                    break;
                case "player":
                    message = "/player player - Sets player's rank to player";
                    break;
                case "say":
                    message = "/say message - Displays message as a global message (use %0-9a-f for color codes)";
                    break;
                case "tp":
                    message = "/tp player - Teleports you to player's location";
                    break;
                case "where":
                    message = "/where [player] - Displays your location, or player's if specified";
                    break;
                default:
                    break;
            }
            p.SendMessage(0xFF, message);
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
