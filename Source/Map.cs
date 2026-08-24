using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Minecraft.Source.Objects;

namespace Minecraft.Source
{
    public class Map
    {
        private readonly BasicEffect _effect;

        private Block block1;

        private bool _initialized = false;

        public Map()
        {
            _effect = new BasicEffect(Globals.GetGraphics().GraphicsDevice)
            {
                TextureEnabled = true,
                World = Matrix.Identity,
                Projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.ToRadians(60),
                    Globals.GetGraphics().GraphicsDevice.Viewport.AspectRatio,
                    0.1f,
                    100f
                )
            };

            block1 = new Block();
        }

        public void Draw()
        {
            if (!_initialized)
            {
                _initialized = true;
                _effect.Texture = Globals.GetTexture();
            }

            _effect.View = Globals.GetCamera().GetView();

            Globals.GetGraphics().GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            block1.Draw(_effect);
        }
    }
}
