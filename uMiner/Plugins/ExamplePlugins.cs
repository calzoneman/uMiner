using System;
using System.Collections.Generic;
using System.Text;

namespace uMiner
{
    public class ExamplePlugins
    {
        //You can also add these handlers within commands.

        public static void Init(Player p)  //Must call ExamplePlugins.Init(this) on each player when they join
        {
            p.OnLogin += new Player.LoginHandler(LoginMessage);
            //p.ResetLoginHandler(); //Resets the LoginHandler and therefore undoes the above line
        }

        public static void LoginMessage(Player p)
        {
            p.SendMessage(0xFF, "Welcome to my uMiner server!");
            p.OnBlockchange += new Player.BlockHandler(NoBuildMode);
            //p.ResetLoginHandler();  //Resets the BlockHandler and therefore undoes the above line
            p.OnMovement += new Player.PositionChangeHandler(FreezePlayer);
            //p.ResetPositionChangeHandler(); //Resets the PositionChangeHandler and therefore undoes the above line
        }

        public static void NoBuildMode(Player p, int x, int y, int z, byte type)
        {
            p.SendBlock((short)x, (short)y, (short)z, p.world.GetTile(x, y, z));
        }

        public static void FreezePlayer(Player p, short[] oldPos, byte[] oldRot, short[] newPos, byte[] newRot)
        {
            int dx = newPos[0] - oldPos[0];
            int dy = newPos[1] - oldPos[1];
            int dz = newPos[2] - oldPos[2];
            if ((int)(Math.Sqrt((double)(dx*dx + dz*dz))) > 4)
            {
                p.SendSpawn(oldPos, newRot);
            }
        }
    }
}
