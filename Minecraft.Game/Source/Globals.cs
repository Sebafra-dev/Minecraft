using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Minecraft.Source.Objects;
using Minecraft.Source.UI;

namespace Minecraft.Source
{
    public static class Globals
    {
        private static GraphicsDeviceManager _graphics;
        private static SpriteBatch _spriteBatch;
        private static SpriteFont _font;
        private static Camera _camera;
        private static Texture2D _texture;
        private static Map _map;
        private static HUD _hud;
        private static Player _player;
        private static Controlling _controlling;
        private readonly static FastNoiseLite _noise = new(SEED);
        private static Effect _voxelEffect;
        private static Matrix _projection;

        public const int SEED = 1000;
        public const int SEC_TO_MS = 1000;

        public static GraphicsDeviceManager GetGraphics() => _graphics;
        public static SpriteBatch GetSpriteBatch() => _spriteBatch;
        public static SpriteFont GetFont() => _font;
        public static Camera GetCamera() => _camera;
        public static Texture2D GetTexture() => _texture;
        public static Map GetMap() => _map;
        public static HUD GetHud() => _hud;
        public static Player GetPlayer() => _player;
        public static Controlling GetControlling() => _controlling;
        public static FastNoiseLite GetNoise() => _noise;
        public static Effect GetEffect() => _voxelEffect;
        public static Matrix GetProjection() => _projection;

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

        public static void SetMap(Map map)
        {
            _map = map;
        }

        public static void SetHud(HUD hud)
        {
            _hud = hud;
        }

        public static void SetPlayer(Player player)
        {
            _player = player;
        }

        public static void SetControlling(Controlling controlling) 
        { 
            _controlling = controlling; 
        }

        public static void SetEffect(Effect effect)
        {
            _voxelEffect = effect;
        }

        public static void SetProjection(Matrix projection)
        {
            _projection = projection;
        }
    }
}
