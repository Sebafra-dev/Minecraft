using Minecraft.Source.Structures;
using System;

namespace Minecraft.Source.Objects
{
    public struct Block
    {
        public enum BlockType : byte
        {
            Air,
            Grass,
            Dirt,
            Stone,
            Wood,
            Leaf,
            RedFlower,
            YellowFlower,
            SmallGrass
        }

        private BlockType _id;

        public Block(BlockType id)
        {
            SetBlockType(id);
        }

        public void SetBlockType(BlockType blockType)
        {
            _id = blockType;
        }

        public readonly BlockType GetBlockType() => _id;

        public readonly Tuple<int, int> GetAtlasIds()
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
                case BlockType.RedFlower:
                    return new(7, 7);
                case BlockType.YellowFlower:
                    return new(8, 8);
                case BlockType.SmallGrass:
                    return new(9, 9);
                default:
                    break;
            }

            return new(0, 0);
        }

        public readonly BlockProperties GetProperties() => BlockProperties.Blocks[GetBlockType()];

    }
}
