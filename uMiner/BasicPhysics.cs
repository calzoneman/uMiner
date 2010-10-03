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
                                break;
                            case Blocks.lava:
                                GenericSpread(block.x, block.y, block.z, block.type, false);
                                break;
                            case Blocks.sponge:
                                NewSponge(block.x, block.y, block.z);
                                break;
                            default:
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
                    return 100;
                case Blocks.lava:
                    return 800;
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
        #endregion

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
