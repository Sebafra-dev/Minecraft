using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Minecraft.Source.Structures
{
    public struct VoxelVertex : IVertexType
    {
        public Vector3 Position;
        public Vector2 AtlasUV;
        public Vector2 TileUV;
        public float Alpha;

        public static readonly VertexDeclaration VertexDeclaration =
            new(
                new VertexElement(
                    0,
                    VertexElementFormat.Vector3,
                    VertexElementUsage.Position,
                    0),

                new VertexElement(
                    12,
                    VertexElementFormat.Vector2,
                    VertexElementUsage.TextureCoordinate,
                    0),

                new VertexElement(
                    20,
                    VertexElementFormat.Vector2,
                    VertexElementUsage.TextureCoordinate,
                    1),
                new VertexElement(
                    28,
                    VertexElementFormat.Single,
                    VertexElementUsage.TextureCoordinate,
                    2)
            );

        readonly VertexDeclaration IVertexType.VertexDeclaration =>
            VertexDeclaration;
    }
}
