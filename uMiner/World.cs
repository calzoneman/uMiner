/**
 * uMiner - A lightweight custom Minecraft Classic server written in C#
 * Copyright 2010 Calvin "calzoneman" Montgomery
 * 
 * Licensed under the Creative Commons Attribution-ShareAlike 3.0 Unported License
 * (see http://creativecommons.org/licenses/by-sa/3.0/, or LICENSE.txt for a full license
 */

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Text;

namespace uMiner
{
    public class World
    {
        public short width, height, depth;
        public short spawnx, spawny, spawnz;
        public byte srotx, sroty; //Spawn rotation
        public byte[] blocks;
        public string name;
        private string filename;
        public bool saveToImage = false;

        public World(string filename)
        {
            try
            {
                if (filename.Substring(filename.LastIndexOf('.') + 1, 3).Equals("umo"))
                {
                    LoadOld(filename);
                }
                else
                {
                    Load(filename);
                }
            }
            catch
            {
                Program.server.logger.log("Error while loading map.");
            }
        }

        public World(short width, short height, short depth)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;
            this.name = "default";
            this.filename = "default.umw";
            this.blocks = WorldGenerator.GenerateFlatgrass(width, height, depth);
            this.spawnx = (short)(this.width / 2);
            this.spawny = (short)(this.height / 2 + 2);
            this.spawnz = (short)(this.depth / 2);
            Console.WriteLine(spawnx + ", " + spawny + ", " + spawnz);
        }

        public World(string filename, short width, short height, short depth)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;
            this.name = filename.Substring(0, filename.LastIndexOf('.'));
            this.filename = filename;
            this.blocks = WorldGenerator.GenerateFlatgrass(width, height, depth);
            this.spawnx = (short)(this.width / 2);
            this.spawny = (short)(this.height / 2 + 2);
            this.spawnz = (short)(this.depth / 2);
            Console.WriteLine(spawnx + ", " + spawny + ", " + spawnz);
        }

        public byte GetTile(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= this.width || y >= this.height || z >= this.depth)
            {
                return 0xFF;
            }
            return this.blocks[(y * this.depth + z) * this.width + x];
        }

        public void SetTile(int x, int y, int z, byte type)
        {
            if (x < 0 || y < 0 || z < 0 || x >= this.width || y >= this.height || z >= this.depth || type < 0 || type > 49)
            {
                return;
            }
            if (Blocks.BasicPhysics(type))
            {
                if (Blocks.AffectedBySponges(type) && Program.server.physics.FindSponge(x, y, z))
                {
                    return;
                }
                Program.server.physics.Queue(x, y, z, type);
            }
            if (GetTile(x, y, z) == Blocks.sponge && type == 0)
            {
                Program.server.physics.DeleteSponge(x, y, z);
            }
            this.blocks[(y * this.depth + z) * this.width + x] = type;
            Player.GlobalBlockchange((short)x, (short)y, (short)z, type);
            
        }

        public void Save()
        {
            try
            {
                GZipStream gzout = new GZipStream(new FileStream("maps/" + filename, FileMode.OpenOrCreate), CompressionMode.Compress);
                gzout.Write(BitConverter.GetBytes(0xebabefac), 0, 4);
                gzout.Write(BitConverter.GetBytes(width), 0, 2);
                gzout.Write(BitConverter.GetBytes(height), 0, 2);
                gzout.Write(BitConverter.GetBytes(depth), 0, 2);
                gzout.Write(BitConverter.GetBytes(spawnx), 0, 2);
                gzout.Write(BitConverter.GetBytes(spawny), 0, 2);
                gzout.Write(BitConverter.GetBytes(spawnz), 0, 2);
                gzout.WriteByte(this.srotx);
                gzout.WriteByte(this.sroty);
                gzout.Write(this.blocks, 0, this.blocks.Length);

                //gzout.BaseStream.Close();
                gzout.Close();
                Program.server.logger.log("Level \"" + this.name + "\" saved");
            }
            catch (Exception e)
            {
                Program.server.logger.log("Error occurred while saving map", Logger.LogType.Error);
                Program.server.logger.log(e);
            }
        }                

        public bool LoadOld(string filename)
        {
            try
            {
                this.filename = filename;
                GZipStream gzin = new GZipStream(new FileStream("maps/" + filename, FileMode.Open), CompressionMode.Decompress);

                byte[] magicnumbytes = new byte[4];
                gzin.Read(magicnumbytes, 0, 4);
                if (!(BitConverter.ToUInt32(magicnumbytes, 0) == 0xebabefac))
                {
                    Program.server.logger.log("Wrong magic number in level file: " + BitConverter.ToUInt32(magicnumbytes, 0), Logger.LogType.Error);
                    return false;
                }

                byte[] leveldimensions = new byte[6];
                gzin.Read(leveldimensions, 0, 6);
                this.width = BitConverter.ToInt16(leveldimensions, 0);
                this.height = BitConverter.ToInt16(leveldimensions, 2);
                this.depth = BitConverter.ToInt16(leveldimensions, 4);

                byte[] spawnpoint = new byte[6];
                gzin.Read(spawnpoint, 0, 6);
                this.spawnx = BitConverter.ToInt16(spawnpoint, 0);
                this.spawny = BitConverter.ToInt16(spawnpoint, 2);
                this.spawnz = BitConverter.ToInt16(spawnpoint, 4);

                this.srotx = 0;
                this.sroty = 0;

                this.blocks = new byte[this.width * this.height * this.depth];
                gzin.Read(blocks, 0, this.width * this.height * this.depth);

                //gzin.BaseStream.Close();
                gzin.Close();

                this.name = filename.Substring(0, filename.IndexOf(".umo"));
                this.filename = this.name + ".umw";

                Program.server.logger.log("Loaded world from " + filename);
                return true;
            }
            catch(Exception e)
            {
                Program.server.logger.log("Error occurred while loading map", Logger.LogType.Error);
                Program.server.logger.log(e);
                return false;
            }
        }

        public bool Load(string filename)
        {
            try
            {
                this.filename = filename;
                GZipStream gzin = new GZipStream(new FileStream("maps/" + filename, FileMode.Open), CompressionMode.Decompress);

                byte[] magicnumbytes = new byte[4];
                gzin.Read(magicnumbytes, 0, 4);
                if (!(BitConverter.ToUInt32(magicnumbytes, 0) == 0xebabefac))
                {
                    Program.server.logger.log("Wrong magic number in level file: " + BitConverter.ToUInt32(magicnumbytes, 0), Logger.LogType.Error);
                    return false;
                }

                byte[] leveldimensions = new byte[6];
                gzin.Read(leveldimensions, 0, 6);
                this.width = BitConverter.ToInt16(leveldimensions, 0);
                this.height = BitConverter.ToInt16(leveldimensions, 2);
                this.depth = BitConverter.ToInt16(leveldimensions, 4);

                byte[] spawnpoint = new byte[6];
                gzin.Read(spawnpoint, 0, 6);
                this.spawnx = BitConverter.ToInt16(spawnpoint, 0);
                this.spawny = BitConverter.ToInt16(spawnpoint, 2);
                this.spawnz = BitConverter.ToInt16(spawnpoint, 4);

                this.srotx = (byte)gzin.ReadByte();
                this.sroty = (byte)gzin.ReadByte();

                this.blocks = new byte[this.width * this.height * this.depth];
                gzin.Read(blocks, 0, this.width * this.height * this.depth);

                //gzin.BaseStream.Close();
                gzin.Close();

                this.name = filename.Substring(0, filename.IndexOf(".umw"));

                Program.server.logger.log("Loaded world from " + filename);
                return true;
            }
            catch (Exception e)
            {
                Program.server.logger.log("Error occurred while loading map", Logger.LogType.Error);
                Program.server.logger.log(e);
                return false;
            }
        } 

    }
}
