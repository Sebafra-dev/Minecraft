using Microsoft.Xna.Framework;

namespace Minecraft.Source
{
    public static class Utils
    {
        public static Vector2 GetUV(int tileId, int tileSize = 8, int atlasSize = 128)
        {
            var tileX = tileId % 16;
            var tileY = tileId / 16;

            return new Vector2(
                (float)(tileX * tileSize) / atlasSize,
                (float)(tileY * tileSize) / atlasSize
            );
        }

        public static int Mod(this int a, int b)
        {
            return (a % b + b) % b;
        }
    }
}
