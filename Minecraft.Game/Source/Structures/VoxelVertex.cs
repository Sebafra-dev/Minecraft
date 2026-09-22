using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minecraft.Source.Structures
{
    public struct VoxelVertex : IVertexType
    {
        public Vector3 Position;
        public Vector2 AtlasUV;
        public Vector2 TileUV;

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
                    1)
            );

        readonly VertexDeclaration IVertexType.VertexDeclaration =>
            VertexDeclaration;
    }
}
