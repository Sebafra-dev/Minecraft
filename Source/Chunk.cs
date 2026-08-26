using Minecraft.Source.Objects;
using System;
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

        public Chunk()
        {
            _blockMap = new bool[WIDTH, HEIGHT, DEPTH];
            _blocks = [];
        }

        ~Chunk()
        {
            _blocks.Clear();
            _blockMap = null;
        }

        public void AddBlockOnChunk(BlockType blockType, int x, int y, int z)
        {
            _blocks.Add(new Block(blockType, x, y, z));
            _blockMap[x.Mod(WIDTH), y.Mod(HEIGHT), z.Mod(DEPTH)] = true;
        }

        public List<Block> GetVisibleBlocks() 
        { 
            return _blocks; 
        }

        public bool IsBlock(int x, int y, int z) => _blockMap[x, y, z];
    }
}
