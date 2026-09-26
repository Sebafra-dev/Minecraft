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
            Stone,
            Wood,
            Leaf
        }

        private BlockType _id;

        public Block(BlockType id)
        {
            _id = id;
        }

        public void SetBlockType(BlockType blockType)
        {
            _id = blockType;
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
                case BlockType.Wood:
                    return new(5, 4);
                case BlockType.Leaf:
                    return new(6, 6);
                default:
                    break;
            }

            return new(0, 0);
        }

        public BlockType GetBlockType() => _id;

        public Tuple<int, int> GetAtlasIds() => GetIdsOnAtlas();

    }
}
