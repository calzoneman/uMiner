using System;
using System.Collections.Generic;
using System.Text;

namespace uMiner
{
    public class Rank
    {
        public static string RankName(byte permissionLevel)
        {
            switch (permissionLevel)
            {
                case 0:
                    return "none";
                case 1:
                    return "guest";
                case 16:
                    return "player";
                case 128:
                    return "operator";
                case 255:
                    return "owner";
                default:
                    return "none";
            }
        }

        public static byte RankLevel(string name)
        {
            switch (name)
            {
                case "none":
                    return 0;
                case "guest":
                    return 1;
                case "player":
                    return 16;
                case "operator":
                    return 128;
                case "owner":
                    return 255;
                default:
                    return 0;
            }
        }

        public static string GetColor(byte ranklevel)
        {
            switch (ranklevel)
            {
                case 1:
                    return "&7";
                case 16:
                    return "&f";
                case 128:
                    return "&9";
                case 255:
                    return "&4";
                default:
                    return "&7";
            }
        }

        public static string GetColor(string rankName)
        {
            switch (rankName)
            {
                case "guest":
                    return "&7";
                case "player":
                    return "&f";
                case "operator":
                    return "&9";
                case "owner":
                    return "&4";
                default:
                    return "&7";
            }
        }
    }
}
