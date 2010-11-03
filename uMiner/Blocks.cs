using System;
using System.Collections.Generic;
using System.Text;

namespace uMiner
{
    public class Blocks
    {

        #region Block Definitions
        public const byte air 				= (byte)0;
  	    public const byte stone 			= (byte)1;
  	    public const byte grass 			= (byte)2;
  	    public const byte dirt 				= (byte)3;
  	    public const byte cobblestone    	= (byte)4;
  	    public const byte wood 				= (byte)5;
  	    public const byte shrub 			= (byte)6;
  	    public const byte adminium 			= (byte)7;
  	    public const byte water 			= (byte)8;
  	    public const byte waterstill 		= (byte)9;
  	    public const byte lava 				= (byte)10;
  	    public const byte lavastill 		= (byte)11;
  	    public const byte sand 				= (byte)12;
  	    public const byte gravel 			= (byte)13;
  	    public const byte goldore 			= (byte)14;
  	    public const byte ironore 			= (byte)15;
  	    public const byte coal 				= (byte)16;
  	    public const byte trunk 			= (byte)17;
  	    public const byte leaf 				= (byte)18;
  	    public const byte sponge 			= (byte)19;
  	    public const byte glass 			= (byte)20;
  	    public const byte red 				= (byte)21;
  	    public const byte orange 			= (byte)22;
  	    public const byte yellow 			= (byte)23;
  	    public const byte lightgreen 		= (byte)24;
  	    public const byte green 			= (byte)25;
  	    public const byte aquagreen 		= (byte)26;
  	    public const byte cyan 				= (byte)27;
  	    public const byte lightblue 		= (byte)28;
  	    public const byte blue 				= (byte)29;
  	    public const byte purple 			= (byte)30;
  	    public const byte lightpurple 		= (byte)31;
  	    public const byte pink 				= (byte)32;
  	    public const byte darkpink 			= (byte)33;
  	    public const byte darkgrey 			= (byte)34;
  	    public const byte lightgrey 		= (byte)35;
  	    public const byte white 			= (byte)36;
  	    public const byte yellowflower 		= (byte)37;
  	    public const byte redflower 		= (byte)38;
  	    public const byte mushroom 			= (byte)39;
  	    public const byte redmushroom 		= (byte)40;
  	    public const byte goldsolid 		= (byte)41;
  	    public const byte ironsolid			= (byte)42;
  	    public const byte staircasefull 	= (byte)43;
  	    public const byte staircasestep 	= (byte)44;
  	    public const byte brick 			= (byte)45;
  	    public const byte tnt 				= (byte)46;
  	    public const byte bookcase 			= (byte)47;
  	    public const byte mossycobble		= (byte)48;
  	    public const byte obsidian 			= (byte)49;

        //Custom blocks
        public const byte unflood           = (byte)100;
        #endregion

        public static Dictionary<string, byte> blockNames = new Dictionary<string, byte>();
        public static Dictionary<byte, byte> conversions = new Dictionary<byte, byte>();

        public static void Init()
        {
            blockNames.Add("air", air);
            blockNames.Add("stone", stone);
            blockNames.Add("rock", stone);
            blockNames.Add("grass", grass);
            blockNames.Add("dirt", dirt);
            blockNames.Add("cobblestone", cobblestone);
            blockNames.Add("cobble", cobblestone);
            blockNames.Add("wood", wood);
            blockNames.Add("planks", wood);
            blockNames.Add("shrub", shrub);
            blockNames.Add("tree", shrub);
            blockNames.Add("adminium", adminium);
            blockNames.Add("admin", adminium);
            blockNames.Add("admincrete", adminium);
            blockNames.Add("water", water);
            blockNames.Add("activewater", water);
            blockNames.Add("stillwater", waterstill);
            blockNames.Add("safewater", waterstill);
            blockNames.Add("lava", lava);
            blockNames.Add("activelava", lava);
            blockNames.Add("stilllava", lavastill);
            blockNames.Add("safelava", lavastill);
            blockNames.Add("sand", sand);
            blockNames.Add("gravel", gravel);
            blockNames.Add("goldore", goldore);
            blockNames.Add("ironore", ironore);
            blockNames.Add("coal", coal);
            blockNames.Add("trunk", trunk);
            blockNames.Add("logs", trunk);
            blockNames.Add("leaves", leaf);
            blockNames.Add("leaf", leaf);
            blockNames.Add("sponge", sponge);
            blockNames.Add("glass", glass);
            blockNames.Add("red", red);
            blockNames.Add("orange", orange);
            blockNames.Add("yellow", yellow);
            blockNames.Add("lightgreen", lightgreen);
            blockNames.Add("green", green);
            blockNames.Add("aqua", aquagreen);
            blockNames.Add("cyan", cyan);
            blockNames.Add("lightblue", lightblue);
            blockNames.Add("indigo", blue);
            blockNames.Add("lavender", blue);
            blockNames.Add("purple", purple);
            blockNames.Add("violet", purple);
            blockNames.Add("lightpurple", lightpurple);
            blockNames.Add("lightpink", pink);
            blockNames.Add("pink", darkpink);
            blockNames.Add("darkpink", darkpink);
            blockNames.Add("darkgrey", darkgrey);
            blockNames.Add("black", darkgrey);
            blockNames.Add("grey", lightgrey);
            blockNames.Add("lightgrey", lightgrey);
            blockNames.Add("white", white);
            blockNames.Add("yellowflower", yellowflower);
            blockNames.Add("redflower", redflower);
            blockNames.Add("brownmushroom", mushroom);
            blockNames.Add("redmushroom", redmushroom);
            blockNames.Add("gold", goldsolid);
            blockNames.Add("goldsolid", goldsolid);
            blockNames.Add("iron", ironsolid);
            blockNames.Add("ironsolid", ironsolid);
            blockNames.Add("staircasefull", staircasefull);
            blockNames.Add("doublestair", staircasefull);
            blockNames.Add("doublestep", staircasefull);
            blockNames.Add("staircasestep", staircasestep);
            blockNames.Add("halfstep", staircasestep);
            blockNames.Add("singlestair", staircasestep);
            blockNames.Add("singlestep", staircasestep);
            blockNames.Add("brick", brick);
            blockNames.Add("bricks", brick);
            blockNames.Add("tnt", tnt);
            blockNames.Add("bookcase", bookcase);
            blockNames.Add("shelves", bookcase);
            blockNames.Add("bookshelf", bookcase);
            blockNames.Add("stonevine", mossycobble);
            blockNames.Add("mossycobblestone", mossycobble);
            blockNames.Add("mossycobble", mossycobble);
            blockNames.Add("obsidian", obsidian);
            blockNames.Add("obby", obsidian);

            blockNames.Add("unflood", unflood);
            blockNames.Add("deflood", unflood);
            blockNames.Add("air_flood", unflood);

            conversions.Add(100, 0);
        }

        public static bool BasicPhysics(byte type)
        {
            switch (type)
            {
                case air:
                case water:
                case lava:
                case sponge:
                case staircasestep:
                case unflood:
                case sand:
                case gravel:
                    return true;
                default:
                    return false;
            }
        }

        public static bool Liquid(byte type)
        {
            switch (type)
            {
                case lava:
                case water:
                    return true;
                default:
                    return false;
            }
        }

        public static bool AffectedBySponges(byte type)
        {
            switch (type)
            {
                case water:
                    return true;
                case lava:
                    if (Program.server.physics.lavaSpongeEnabled)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    return false;
            }
        }

        public static bool AffectedByGravity(byte type)
        {
            switch (type)
            {
                case sand:
                case gravel:
                    return true;
                default:
                    return false;
            }
        }

        public static byte ConvertType(byte old)
        {
            if (old <= 49) { return old; }
            if (conversions.ContainsKey(old))
            {
                return conversions[old];
            }
            return 0;
        }
    }
}
