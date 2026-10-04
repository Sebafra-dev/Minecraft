using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Minecraft.Source.Structures
{
    public struct VoxelVertex : IVertexType
    {
        public Vector3 Position;
        public HalfVector2 TileUV;
        public Color Data;

        public static readonly VertexDeclaration VertexDeclaration =
            new(
                new VertexElement(
                    0,
                    VertexElementFormat.Vector3,
                    VertexElementUsage.Position,
                    0),

                new VertexElement(
                    12,
                    VertexElementFormat.HalfVector2,
                    VertexElementUsage.TextureCoordinate,
                    0),
                new VertexElement(
                    16,
                    VertexElementFormat.Color,
                    VertexElementUsage.Color,
                    0)
            );

        readonly VertexDeclaration IVertexType.VertexDeclaration =>
            VertexDeclaration;
    }
}
