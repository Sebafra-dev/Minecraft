using System.Collections.Generic;

namespace Minecraft.Source
{
    public class BlockProperties
    {
        public bool Visible { get; private set; }
        public bool Transparent { get; private set; }
        public bool Object { get; private set; }

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

        public readonly static Dictionary<BlockType, BlockProperties> Blocks = new() {
            [BlockType.Air] = new BlockProperties(visible: false), 
            [BlockType.Grass] = new BlockProperties(),
            [BlockType.Dirt] = new BlockProperties(),
            [BlockType.Stone] = new BlockProperties(),
            [BlockType.Wood] = new BlockProperties(),
            [BlockType.Leaf] = new BlockProperties(transparent: true),
            [BlockType.RedFlower] = new BlockProperties(@object: true),
            [BlockType.YellowFlower] = new BlockProperties(@object: true),
            [BlockType.SmallGrass] = new BlockProperties(@object: true),
        };

        public BlockProperties(bool visible = true, bool transparent = false, bool @object = false)
        {
            Visible = visible;
            Transparent = transparent;
            Object = @object;
        }
    }
}
