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
    public class WorldGenerator
    {
        public static byte[] GenerateFlatgrass(short width, short height, short depth)
        {
            byte[] map = new byte[width * height * depth];
            for (short x = 0; x < width; x++)
            {
                for (short z = 0; z < depth; z++)
                {
                    for (short y = 0; y < height; y++)
                    {
                        int index = (y * depth + z) * width + x;
                        if (y < height / 2 - 9)
                        {
                            map[index] = (byte)1;
                        }
                        else if (y < height / 2 - 1)
                        {
                            map[index] = (byte)3;
                        }
                        else if (y == height / 2 - 1)
                        {
                            map[index] = (byte)2;
                        }
                        else
                        {
                            map[index] = (byte)0;
                        }
                    }
                }
            }
            return map;
        }
    }
}
