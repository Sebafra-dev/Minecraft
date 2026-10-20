using Microsoft.Xna.Framework;
using System;

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

        public static float GetHashOnPosition(int x, int z)
        {
            unchecked
            {
                uint hash = Globals.SEED;

                hash ^= (uint)x * 374761393u;
                hash = (hash << 13) | (hash >> 19);

                hash ^= (uint)z * 668265263u;
                hash *= 1274126177u;

                hash ^= hash >> 16;
                hash *= 2246822519u;
                hash ^= hash >> 13;

                return hash / (float)uint.MaxValue;
            }
        }
    }
}
