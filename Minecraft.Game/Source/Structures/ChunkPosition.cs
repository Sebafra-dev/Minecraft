using System;
using System.Diagnostics.CodeAnalysis;

namespace Minecraft.Source.Structures
{
    public struct ChunkPosition
    {
        public int X;
        public int Z;

        public ChunkPosition(int x, int z)
        {
            X = x;
            Z = z;
        }

        public override readonly bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj is not ChunkPosition chunkPosition) 
                return false;

            return chunkPosition.X == X && chunkPosition.Z == Z;
        }

        public static bool operator ==(ChunkPosition left, ChunkPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ChunkPosition left, ChunkPosition right)
        {
            return !(left == right);
        }

        public static ChunkPosition operator +(ChunkPosition left, ChunkPosition right)
        {
            return new(left.X + right.X, left.Z + right.Z);
        }

        public readonly int GetDist(ChunkPosition chunkPosition2) => Math.Abs(X - chunkPosition2.X) + Math.Abs(Z - chunkPosition2.Z);

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Z);
        }
    }
}
