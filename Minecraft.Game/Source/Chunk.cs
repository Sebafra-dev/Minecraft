using Minecraft.Source.Objects;
using Minecraft.Source.Structures;
using System.Collections.Generic;
using static Minecraft.Source.Objects.Block;

namespace Minecraft.Source
{
    public class Chunk
    {
        public const int WIDTH = 16;
        public const int HEIGHT = 256;
        public const int DEPTH = 16;

        private bool[,,] _blockMap;
        private readonly List<Block> _blocks;
        private readonly List<Block> _visibleBlocks;
        private readonly (short, short) _position;

        public Chunk((short, short) position)
        {
            _position = position;

            _blockMap = new bool[WIDTH, HEIGHT, DEPTH];
            _blocks = [];
            _visibleBlocks = [];

            GenerateChunk();
        }

        ~Chunk()
        {
            _blocks.Clear();
            _visibleBlocks.Clear();
            _blockMap = null;
        }

        public (short, short) GetPosition() => _position;

        private void GenerateChunk()
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                if (y > 70)
                    continue;

                for (int x = 0; x < WIDTH; x++)
                {
                    for (int z = 0; z < DEPTH; z++)
                    {
                        AddBlockOnChunk(y > 67 ? BlockType.Grass : BlockType.Stone, new IntPosition(x + _position.Item1 * WIDTH, y, z + _position.Item2 * DEPTH));
                    }
                }
            }
        }

        public void AddBlockOnChunk(BlockType blockType, IntPosition position, bool ignoreRefresh = true) //TODO fix visibleBlocks
        {
            var (chunkX, chunkY, chunkZ) = (position.X.Mod(WIDTH), position.Y.Mod(HEIGHT), position.Z.Mod(DEPTH));
            if (IsBlock(chunkX, chunkY, chunkZ))
                return;

            _blocks.Add(new Block(blockType, position));
            _blockMap[chunkX, chunkY, chunkZ] = true;

            if (!ignoreRefresh)
                RefreshVisibleBlocks();
        }

        public void RefreshVisibleBlocks()
        {
            _visibleBlocks.Clear();
            foreach (var block in _blocks)
            {
                if (block.GetVertices().Length > 0)
                {
                    _visibleBlocks.Add(block);
                }
            }
        }

        public List<Block> GetVisibleBlocks() 
        { 
            return _visibleBlocks; 
        }

        public bool IsBlock(int x, int y, int z) => _blockMap[x, y, z];
    }
}
