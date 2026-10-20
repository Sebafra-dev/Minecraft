using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Minecraft.Source.Objects
{
    public class SelectedBlock
    {
        private static BasicEffect _effect;
        private readonly VertexPositionColor[] _vertices;
        private readonly short[] _indices;

        private bool _draw = false;

        public SelectedBlock()
        {
            _effect ??= new BasicEffect(Globals.GetGraphics().GraphicsDevice)
            {
                LightingEnabled = false,
                VertexColorEnabled = true,
                World = Matrix.Identity
            };

            _vertices = new VertexPositionColor[8];

            _indices =
            [
                0,1, 1,2, 2,3, 3,0,
                4,5, 5,6, 6,7, 7,4,
                0,4, 1,5, 2,6, 3,7
            ];
        }

        public void Update()
        {
            var selectedBlockPos = Globals.GetSelectedBlock();
            if (selectedBlockPos == null)
            {
                _draw = false;
                return;
            }

            var pos = selectedBlockPos.Value;
            var color = Color.Black;

            _draw = true;

            _vertices[0] = new VertexPositionColor(
                new Vector3(pos.X, pos.Y, pos.Z),
                color
            );

            _vertices[1] = new VertexPositionColor(
                new Vector3(pos.X + 1, pos.Y, pos.Z),
                color
            );

            _vertices[2] = new VertexPositionColor(
                new Vector3(pos.X + 1, pos.Y + 1, pos.Z),
                color
            );

            _vertices[3] = new VertexPositionColor(
                new Vector3(pos.X, pos.Y + 1, pos.Z),
                color
            );

            _vertices[4] = new VertexPositionColor(
                new Vector3(pos.X, pos.Y, pos.Z + 1),
                color
            );

            _vertices[5] = new VertexPositionColor(
                new Vector3(pos.X + 1, pos.Y, pos.Z + 1),
                color
            );

            _vertices[6] = new VertexPositionColor(
                new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1),
                color
            );

            _vertices[7] = new VertexPositionColor(
                new Vector3(pos.X, pos.Y + 1, pos.Z + 1),
                color
            );

            _effect.View = Globals.GetCamera().GetView();
        }

        public void Draw()
        {
            if (!_draw)
                return;

            var graphicsDevice = Globals.GetGraphics().GraphicsDevice;

            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.RasterizerState = RasterizerState.CullNone;

            _effect.View = Globals.GetCamera().GetView();
            _effect.Projection = Globals.GetProjection();

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.LineList,
                    _vertices,
                    0,
                    _vertices.Length,
                    _indices,
                    0,
                    _indices.Length / 2
                );
            }

            graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
    }
}
