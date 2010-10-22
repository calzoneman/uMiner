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
    //Pseudo-player object for executing console commands
    class ConsolePlayer : Player
    {
        public ConsolePlayer() : base(null, "127.0.0.1", 0xFF)
        {
            base.rank = 0xFF;
            base.username = "[console]";
        }

        public override void SendMessage(byte opcode, string message)
        {
            Program.server.logger.log(message, Logger.LogType.CCmd);
        }

        public void ExecuteCommand(string input)
        {
            if (input[0] != '/')
            {
                SendMessage(0xFF, "Invalid command entered!");
                return;
            }
            string cmd = input.Substring(1).Trim().Split(' ')[0];
            string message = input.Trim().Substring(cmd.Length + 1).Trim();
            if (Command.consoleSafe.ContainsKey(cmd))
            {
                Command.consoleSafe[cmd].handler(this, message);
            }
            else if (Command.commands.ContainsKey(cmd))
            {
                SendMessage(0xFF, "This command is not console safe!");
            }
            else
            {
                SendMessage(0xFF, "Command does not exist");
            }
        }
    }
}
