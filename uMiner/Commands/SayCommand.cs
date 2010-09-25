using System;
using System.Collections.Generic;
using System.Text;

namespace uMiner
{
    public class SayCommand
    {
        public static void Execute(Player p, string message)
        {
            StringBuilder finalMsg = new StringBuilder();
            message = message.Trim();
            for (int i = 0; i < message.Length; i++)
            {
                char ch = message[i];
                if (ch == '%' && i + 1 < message.Length && "0123456789abcdef".Contains(message[i + 1].ToString()) && i + 2 < message.Length)
                {
                    ch = '&';
                }
                finalMsg.Append(ch);
            }
            finalMsg.Append("&e");
            Player.GlobalMessage(finalMsg.ToString());
        }
    }
}
