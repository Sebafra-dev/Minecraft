using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Minecraft.Source
{
    public static class Globals
    {
        private static GraphicsDeviceManager _graphics;
        private static SpriteBatch _spriteBatch;
        private static SpriteFont _font;
        private static Camera _camera;
        private static Texture2D _texture;

        public readonly static int SEC_TO_MS = 1000;

        public static GraphicsDeviceManager GetGraphics() => _graphics;
        public static SpriteBatch GetSpriteBatch() => _spriteBatch;
        public static SpriteFont GetFont() => _font;
        public static Camera GetCamera() => _camera;
        public static Texture2D GetTexture() => _texture;

        public static void SetGraphics(GraphicsDeviceManager graphics)
        {
            _graphics = graphics;
        }

        public static void SetSpriteBatch(SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch;
        }

        public static void SetFont(SpriteFont font)
        {
            _font = font;
        }

        public static void SetCamera(Camera camera)
        {
            _camera = camera;
        }

        public static void SetTexture(Texture2D texture)
        {
            _texture = texture;
        }
    }
}
