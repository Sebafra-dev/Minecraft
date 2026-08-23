using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Minecraft.Source.Objects
{
    public class Block
    {
        // Ustaw te indeksy w klasie Block (zgodnie z ruchem wskazówek zegara dla zewnętrznych ścian):
        private readonly static short[] _indices =
        [
            0, 2, 1,   0, 3, 2,
            4, 6, 5,   4, 7, 6,
            8, 10, 9,  8, 11, 10,
            12, 14, 13, 12, 15, 14,
            16, 18, 17, 16, 19, 18,
            20, 22, 21, 20, 23, 22
        ];

        private static VertexPositionTexture[] CreateVertices()
        {
            Vector2[] top = Utils.GetUV(0, 0);
            Vector2[] side = Utils.GetUV(1, 0);

            return
            [
                // FRONT
                new(new Vector3(-1, -1,  1), side[0]),
                new(new Vector3( 1, -1,  1), side[1]),
                new(new Vector3( 1,  1,  1), side[2]),
                new(new Vector3(-1,  1,  1), side[3]),

                // RIGHT
                new(new Vector3( 1, -1,  1), side[0]),
                new(new Vector3( 1, -1, -1), side[1]),
                new(new Vector3( 1,  1, -1), side[2]),
                new(new Vector3( 1,  1,  1), side[3]),

                // BACK
                new(new Vector3( 1, -1, -1), side[0]),
                new(new Vector3(-1, -1, -1), side[1]),
                new(new Vector3(-1,  1, -1), side[2]),
                new(new Vector3( 1,  1, -1), side[3]),

                // LEFT
                new(new Vector3(-1, -1, -1), side[0]),
                new(new Vector3(-1, -1,  1), side[1]),
                new(new Vector3(-1,  1,  1), side[2]),
                new(new Vector3(-1,  1, -1), side[3]),

                // TOP
                new(new Vector3(-1,  1,  1), top[0]),
                new(new Vector3( 1,  1,  1), top[1]),
                new(new Vector3( 1,  1, -1), top[2]),
                new(new Vector3(-1,  1, -1), top[3]),

                // BOTTOM
                new(new Vector3(-1, -1, -1), side[0]),
                new(new Vector3( 1, -1, -1), side[1]),
                new(new Vector3( 1, -1,  1), side[2]),
                new(new Vector3(-1, -1,  1), side[3]),
            ];
        }

        public Block()
        {

        }

        public void Draw(BasicEffect effect)
        {
            var vertices = CreateVertices();

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Globals.GetGraphics().GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    vertices,
                    0,
                    vertices.Length,
                    _indices,
                    0,
                    _indices.Length / 3
                );
            }
        }
    }
}
