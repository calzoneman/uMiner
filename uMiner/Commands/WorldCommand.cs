using System;
using System.Collections.Generic;
using System.Text;

namespace uMiner
{
    public class WorldCommand
    {
        public static void NewWorld(Player p, string message)
        {
            if (message.Trim().Equals(""))
            {
                Help(p, "newworld");
                return;
            }

            string[] args = message.Trim().Split(' ');
            if (args.Length != 4)
            {
                Help(p, "newworld");
                return;
            }

            string worldname = args[0];
            short w = Int16.Parse(args[1]);
            short h = Int16.Parse(args[2]);
            short d = Int16.Parse(args[3]);
            worldname += ".umw";

            if (System.IO.File.Exists("maps/" + worldname))
            {
                p.SendMessage(0xFF, "That filename already exists!");
                return;
            }

            World newworld = new World(worldname, w, h, d);
            newworld.Save();

            p.SendMessage(0xFF, "Created new world: " + worldname);
        }

        public static void Help(Player p, string cmd)
        {
            switch (cmd)
            {
                case "newworld":
                    p.SendMessage(0xFF, "/newworld name x y z - Creates a x*y*z size map with the specified name");
                    //p.SendMessage(0xFF, "Available modes: normal, image");
                    break;
                default:
                    break;
            }
        }

    }
}
