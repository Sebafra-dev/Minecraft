using System.Numerics;

namespace Minecraft.Source.Structures
{
    public struct IntPosition
    {
        public int X; 
        public int Y; 
        public int Z;

        public IntPosition(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public readonly Vector3 ToVec3() => new(X, Y, Z);

        public readonly override string ToString() => $"X: {X} Y: {Y} Z: {Z}";
    }
}
