using Minecraft.Source.Objects;
using Minecraft.Source.Structures;
using System.Collections.Generic;
using System.Threading;
using static Minecraft.Source.Objects.Block;

namespace Minecraft.Source
{
    public class Chunk
    {
        public const int WIDTH = 16;
        public const int HEIGHT = 256;
        public const int DEPTH = 16;

        private Block[,,] _blocks;
        private readonly HashSet<Block> _visibleBlocks;
        private readonly (short, short) _position;

        private bool _generated = false;

        public Chunk((short, short) position)
        {
            _position = position;

            _blocks = new Block[WIDTH, HEIGHT, DEPTH];
            _visibleBlocks = [];

            GenerateChunk();
        }

        ~Chunk()
        {
            _blocks = null;
            _visibleBlocks.Clear();
        }

        public (short, short) GetPosition() => _position;

        private void GenerateChunk()
        {
            var chunkX = _position.Item1 * WIDTH;
            var chunkZ = _position.Item2 * DEPTH;
            for (int y = 0; y < HEIGHT; y++)
            {
                if (y > 70)
                    continue;

                for (int x = 0; x < WIDTH; x++)
                {
                    for (int z = 0; z < DEPTH; z++)
                    {
                        AddBlockOnChunk(y > 67 ? BlockType.Grass : BlockType.Stone, new IntPosition(x + chunkX, y, z + chunkZ));
                    }
                }
            }
        }

        public void AddBlockOnChunk(BlockType blockType, IntPosition position)
        {
            var (chunkX, chunkY, chunkZ) = (position.X.Mod(WIDTH), position.Y.Mod(HEIGHT), position.Z.Mod(DEPTH));
            if (IsBlock(chunkX, chunkY, chunkZ))
                return;

            _blocks[chunkX, chunkY, chunkZ] = new Block(blockType, position);
        }

        public void RefreshVisibleBlocks()
        {
            _visibleBlocks.Clear();

            foreach (var block in _blocks)
            {
                if (block == null || !block.IsVisible())
                    continue;

                _visibleBlocks.Add(block);
            }
        }

        public HashSet<Block> GetVisibleBlocks() => _visibleBlocks; 

        public bool IsBlock(int x, int y, int z) => _blocks[x, y, z] != null;

        public bool IsActive() => _generated;

        public void SetActive()
        {
            _generated = true;
        }
    }
}
