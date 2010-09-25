using System;
using System.Collections.Generic;
using System.Text;

namespace uMiner
{
    public class HelpCommand
    {
        public static void Execute(Player p, string message)
        {
            if (!message.Trim().Equals(""))
            {
                Command.HelpMessage(p, message.Trim());
                return;
            }
            p.SendMessage(0xFF, "Running uMiner Rev. &a10");
            p.SendMessage(0xFF, "---------------------");
            p.SendMessage(0xFF, "People with @ before their names are owners, people with + are operators.");
            p.SendMessage(0xFF, "Visit &fhttp://github.com/calzoneman/uMiner&e for downloads and source code.");
            p.SendMessage(0xFF, "---------------------");
            StringBuilder availableCmds = new StringBuilder();
            availableCmds.Append("Available commands:");
            foreach (KeyValuePair<string, Command> cmd in Command.commands)
            {
                if (cmd.Value.minRank <= p.rank)
                {
                    availableCmds.Append(" ");
                    availableCmds.Append(Rank.GetColor(cmd.Value.minRank));
                    availableCmds.Append(cmd.Key);
                }
            }
            p.SendMessage(0xFF, availableCmds.ToString());

        }
    }
}
