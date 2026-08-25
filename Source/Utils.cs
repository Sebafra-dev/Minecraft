using Microsoft.Xna.Framework;

namespace Minecraft.Source
{
    public static class Utils
    {
        public static Vector2[] GetUV(int tileId, int tileSize = 8, int atlasSize = 128)
        {
            var tileX = tileId % 16;
            var tileY = tileId / 16;

            float u1 = (float)(tileX * tileSize) / atlasSize;
            float v1 = (float)(tileY * tileSize) / atlasSize;

            float u2 = (float)((tileX + 1) * tileSize) / atlasSize;
            float v2 = (float)((tileY + 1) * tileSize) / atlasSize;

            return
            [
                new Vector2(u1, v1),
                new Vector2(u2, v1),
                new Vector2(u2, v2),
                new Vector2(u1, v2)
            ];
        }
    }
}
