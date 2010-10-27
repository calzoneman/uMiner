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
    public class Program
    {
        public static Server server = new Server(ServerType.ClassicBuild);
        public const int revision = 21;
        static void Main(string[] args)
        {
            server.Init();
            server.Run();
        }
    }

    public enum ServerType
    {
        ClassicBuild
    }
}
