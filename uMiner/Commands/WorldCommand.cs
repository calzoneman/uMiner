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

        public static void SetSpawn(Player p, string message)
        {
            Program.server.world.spawnx = (short)(p.x >> 5);
            Program.server.world.spawny = (short)(p.y >> 5);
            Program.server.world.spawnz = (short)(p.z >> 5);

            Program.server.world.srotx = p.rotx;
            Program.server.world.sroty = p.roty;

            Program.server.world.Save();

            p.SendMessage(0xFF, "Spawnpoint saved");
        }

        public static void Spawn(Player p, string message)
        {
            short x = (short)(Program.server.world.spawnx << 5);
            short y = (short)(Program.server.world.spawny << 5);
            short z = (short)(Program.server.world.spawnz << 5);
            p.SendSpawn(new short[] { x, y, z }, new byte[] { Program.server.world.srotx, Program.server.world.sroty });
        }

        public static void Help(Player p, string cmd)
        {
            switch (cmd)
            {
                case "newworld":
                    p.SendMessage(0xFF, "/newworld name x y z - Creates a x*y*z size map with the specified name");
                    //p.SendMessage(0xFF, "Available modes: normal, image");
                    break;
                case "setspawn":
                    p.SendMessage(0xFF, "/setspawn - Sets the map's spawnpoint to your location");
                    break;
                case "spawn":
                    p.SendMessage(0xFF, "/spawn - Returns you to the map's spawnpoint");
                    break;
                default:
                    break;
            }
        }

    }
}
