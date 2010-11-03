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
    public class BasicPhysics
    {
        public World world;
        private Queue<PhysicsBlock> updateQueue;
        private object queueLock = new object();
        public bool lavaSpongeEnabled = false;

        public BasicPhysics(World _world)
        {
            this.world = _world;
            this.updateQueue = new Queue<PhysicsBlock>();
        }

        public BasicPhysics(World _world, bool _lavaSpongeEnabled)
        {
            this.world = _world;
            this.lavaSpongeEnabled = _lavaSpongeEnabled;
            this.updateQueue = new Queue<PhysicsBlock>();
        }

        public void Update()
        {
            if (updateQueue.Count == 0) { return; }
            lock(queueLock)
            {
                int n = updateQueue.Count;
                for (int i = 0; i < n; i++)
                {
                    PhysicsBlock block = updateQueue.Dequeue();
                    if (((TimeSpan)(DateTime.Now - block.startTime)).TotalMilliseconds >= PhysicsTime(block.type))
                    {
                        switch (block.type)
                        {
                            case Blocks.air:
                                AirCheck(block.x, block.y, block.z);
                                break;
                            case Blocks.water:
                                GenericSpread(block.x, block.y, block.z, block.type, false);
                                CheckWaterLavaCollide(block.x, block.y, block.z, block.type);
                                break;
                            case Blocks.lava:
                                GenericSpread(block.x, block.y, block.z, block.type, false);
                                CheckWaterLavaCollide(block.x, block.y, block.z, block.type);
                                break;
                            case Blocks.unflood:
                                Unflood(block.x, block.y, block.z, block.type);
                                break;
                            case Blocks.sponge:
                                NewSponge(block.x, block.y, block.z);
                                break;
                            case Blocks.staircasestep:
                                Staircase(block);
                                break;    
                            default:
                                if (Blocks.AffectedByGravity(block.type))
                                {
                                    SandGravelFall(block.x, block.y, block.z, block.type);
                                }
                                break;
                        }
                    }
                    else
                    {
                        updateQueue.Enqueue(block);
                    }
                }
            }
        }

        public bool Queue(int x, int y, int z, byte type)
        {
            try
            {
                lock(queueLock)
                {
                    this.updateQueue.Enqueue(new PhysicsBlock((short)x, (short)y, (short)z, type));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static int PhysicsTime(byte type)
        {
            switch (type)
            {
                case Blocks.water:
                    return 200;
                case Blocks.lava:
                    return 800;
                case Blocks.unflood:
                    return 200;
                default:
                    return 0;
            }
        }

        #region Individual Physics Handlers
        public void GenericSpread(short x, short y, short z, byte type, bool canGoUp)
        {
            if(world.GetTile(x, y, z) != type)
            {
                return;
            }
            
            if(world.GetTile(x + 1, y, z) == Blocks.air)
            {
                world.SetTile(x + 1, y, z, type);
            }
            if(world.GetTile(x - 1, y, z) == Blocks.air)
            {
                world.SetTile(x - 1, y, z, type);
            }
            if(world.GetTile(x, y, z + 1) == Blocks.air)
            {
                world.SetTile(x, y, z + 1, type);
            }
            if(world.GetTile(x, y, z - 1) == Blocks.air)
            {
                world.SetTile(x, y, z - 1, type);
            }
            if(world.GetTile(x, y - 1, z) == Blocks.air)
            {
                world.SetTile(x, y - 1, z, type);
            }
            if(canGoUp && world.GetTile(x, y + 1, z) == Blocks.air)
            {
                world.SetTile(x, y + 1, z, type);
            }
        }

        public void Unflood(short x, short y, short z, byte type)
        {
            if (world.GetTile(x, y, z) != type)
            {
                return;
            }

            if (Blocks.Liquid(world.GetTile(x + 1, y, z)))
            {
                world.SetTile(x + 1, y, z, type);
            }
            if (Blocks.Liquid(world.GetTile(x - 1, y, z)))
            {
                world.SetTile(x - 1, y, z, type);
            }
            if (Blocks.Liquid(world.GetTile(x, y, z + 1)))
            {
                world.SetTile(x, y, z + 1, type);
            }
            if (Blocks.Liquid(world.GetTile(x, y, z - 1)))
            {
                world.SetTile(x, y, z - 1, type);
            }
            if (Blocks.Liquid(world.GetTile(x, y - 1, z)))
            {
                world.SetTile(x, y - 1, z, type);
            }
            if (Blocks.Liquid(world.GetTile(x, y + 1, z)))
            {
                world.SetTile(x, y + 1, z, type);
            }
            world.SetTileNoPhysics(x, y, z, Blocks.air);
        }

        public void CheckWaterLavaCollide(int x, int y, int z, byte type)
        {
            if (world.GetTile(x, y, z) != type)
            {
                return;
            }
            if (LavaWaterCollide(world.GetTile(x + 1, y, z), type))
            {
                world.SetTile(x + 1, y, z, Blocks.obsidian);
            }
            if (LavaWaterCollide(world.GetTile(x - 1, y, z), type))
            {
                world.SetTile(x - 1, y, z, Blocks.obsidian);
            }
            if (LavaWaterCollide(world.GetTile(x, y, z + 1), type))
            {
                world.SetTile(x, y, z + 1, Blocks.obsidian);
            }
            if (LavaWaterCollide(world.GetTile(x, y, z - 1), type))
            {
                world.SetTile(x, y, z - 1, Blocks.obsidian);
            }
            if (LavaWaterCollide(world.GetTile(x, y - 1, z), type))
            {
                world.SetTile(x, y - 1, z, Blocks.obsidian);
            }
            if (LavaWaterCollide(world.GetTile(x, y + 1, z), type))
            {
                world.SetTile(x, y + 1, z, Blocks.obsidian);
            }
        }

        public void NewSponge(int x, int y, int z)
        {
            for(int dx = -2; dx <= 2; dx++)
            {
                for(int dy = -2; dy <= 2; dy++)
                {
                    for(int dz = -2; dz <= 2; dz++)
                    {
                        if(Blocks.AffectedBySponges(world.GetTile(x + dx, y + dy, z + dz)))
                        {
                            world.SetTile(x + dx, y + dy, z + dz, Blocks.air);
                        }
                    }
                }
            }
        }

        public bool FindSponge(int x, int y, int z)
        {
            for(int dx = -2; dx <= 2; dx++)
            {
                for(int dy = -2; dy <= 2; dy++)
                {
                    for(int dz = -2; dz <= 2; dz++)
                    {
                        if(world.GetTile(x + dx, y + dy, z + dz) == Blocks.sponge)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void DeleteSponge(int x, int y, int z)
        {
            for(int dx = -3; dx <= 3; dx++)
            {
                for(int dy = -3; dy <= 3; dy++)
                {
                    for(int dz = -3; dz <= 3; dz++)
                    {
                        if (Blocks.BasicPhysics(world.GetTile(x + dx, y + dy, z + dz)) && world.GetTile(x + dx, y + dy, z + dz) != Blocks.air)
                        {
                            Queue(x + dx, y + dy, z + dz, world.GetTile(x + dx, y + dy, z + dz));
                        }
                    }
                }
            }
        }

        public void AirCheck(int x, int y, int z)
        {
            if(Blocks.BasicPhysics(world.GetTile(x + 1, y, z)) && world.GetTile(x + 1, y, z) != Blocks.air)
            {
                Queue(x + 1, y, z, world.GetTile(x + 1, y, z));
            }
            if (Blocks.BasicPhysics(world.GetTile(x - 1, y, z)) && world.GetTile(x - 1, y, z) != Blocks.air)
            {
                Queue(x - 1, y, z, world.GetTile(x - 1, y, z));
            }
            if (Blocks.BasicPhysics(world.GetTile(x, y, z + 1)) && world.GetTile(x, y, z + 1) != Blocks.air)
            {
                Queue(x, y, z + 1, world.GetTile(x, y, z + 1));
            }
            if (Blocks.BasicPhysics(world.GetTile(x, y, z - 1)) && world.GetTile(x, y, z - 1) != Blocks.air)
            {
                Queue(x, y, z - 1, world.GetTile(x, y, z - 1));
            }
            if (Blocks.BasicPhysics(world.GetTile(x, y + 1, z)) && world.GetTile(x, y + 1, z) != Blocks.air)
            {
                Queue(x, y + 1, z, world.GetTile(x, y + 1, z));
            }
        }

        public void AirCheckGravity(int x, int y, int z)
        {
            if (Blocks.AffectedByGravity(world.GetTile(x, y + 1, z)) && world.GetTile(x, y + 1, z) != Blocks.air)
            {
                SandGravelFall(x, y + 1, z, world.GetTile(x, y + 1, z));
            }
        }

        public void Staircase(PhysicsBlock block)
        {
            if (world.GetTile(block.x, block.y, block.z) != block.type) { return; }

            if (world.GetTile(block.x, block.y - 1, block.z) == Blocks.staircasestep)
            {
                world.SetTile(block.x, block.y - 1, block.z, Blocks.staircasefull);
                world.SetTile(block.x, block.y, block.z, Blocks.air);
            }
        }

        public void SandGravelFall(int x, int y, int z, byte type)
        {
            if (world.GetTile(x, y, z) != type)
            {
                return;
            }

            int dy = y;
            while (dy > 0 && world.GetTile(x, dy - 1, z) == Blocks.air)
            {
                dy--;
            }
            if (dy != y)
            {
                world.SetTile(x, y, z, Blocks.air);
                world.SetTileNoPhysics(x, dy, z, type);
            }
            
        }

        #endregion

        public bool LavaWaterCollide(byte a, byte b)
        {
            if ((a == Blocks.water && b == Blocks.lava) || (a == Blocks.lava && b == Blocks.water))
            {
                return true;
            }
            return false;
        }
    }

    public class PhysicsBlock
    {
        public short x, y, z;
        public byte type;
        public DateTime startTime;

        public PhysicsBlock(short x, short y, short z, byte type)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.type = type;
            this.startTime = DateTime.Now;
        }
    }
}
