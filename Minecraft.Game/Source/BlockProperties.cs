using System.Collections.Generic;

namespace Minecraft.Source
{
    public class BlockProperties
    {
        public bool Visible { get; private set; }
        public bool Transparent { get; private set; }
        public bool Object { get; private set; }
        public bool NoCollision { get; private set; }
        public byte Alpha { get; private set; }
        public bool Liquid { get; private set; }

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
            SmallGrass,
            Water
        }

        public readonly static Dictionary<BlockType, BlockProperties> Blocks = new() {
            [BlockType.Air] = new BlockProperties(visible: false, noCollision: true), 
            [BlockType.Grass] = new BlockProperties(),
            [BlockType.Dirt] = new BlockProperties(),
            [BlockType.Stone] = new BlockProperties(),
            [BlockType.Wood] = new BlockProperties(),
            [BlockType.Leaf] = new BlockProperties(transparent: true),
            [BlockType.RedFlower] = new BlockProperties(@object: true, noCollision: true),
            [BlockType.YellowFlower] = new BlockProperties(@object: true, noCollision: true),
            [BlockType.SmallGrass] = new BlockProperties(@object: true, noCollision: true),
            [BlockType.Water] = new BlockProperties(transparent: true, noCollision: true, alpha: 200, liquid: true),
        };

        public BlockProperties(bool visible = true, bool transparent = false, bool @object = false, bool noCollision = false,
            byte alpha = 255, bool liquid = false)
        {
            Visible = visible;
            Transparent = transparent;
            Object = @object;
            NoCollision = noCollision;
            Alpha = alpha;
            Liquid = liquid;
        }
    }
}
