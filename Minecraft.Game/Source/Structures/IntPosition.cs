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

        public static IntPosition operator +(IntPosition left, IntPosition right)
        {
            return new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        public readonly override string ToString() => $"X: {X} Y: {Y} Z: {Z}";
    }
}
