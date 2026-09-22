using Minecraft.Source.Structures;
using System;

namespace Minecraft.Source.Objects
{
    public class Block
    {
        public enum BlockType : byte
        {
            Grass,
            Dirt,
            Stone
        }

        private readonly BlockType _id;

        public Block(BlockType id, IntPosition position)
        {
            _id = id;
        }

        private Tuple<int, int> GetIdsOnAtlas()
        {
            switch (_id)
            {
                case BlockType.Grass:
                    return new(0, 1);
                case BlockType.Dirt:
                    return new(3, 3);
                case BlockType.Stone:
                    return new(2, 2);
                default:
                    break;
            }

            return new(0, 0);
        }

        public BlockType GetBlockType() => _id;

        public Tuple<int, int> GetAtlasIds() => GetIdsOnAtlas();

    }
}
