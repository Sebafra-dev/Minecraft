using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Minecraft.Source.Structures;
using System;
using System.Collections.Generic;

namespace Minecraft.Source.Objects
{
    public class Block
    {
        public enum BlockType : byte
        {
            Grass,
            Stone
        }

        private readonly BlockType _id;

        private readonly BlockPosition _position;

        public Block(BlockType id, IntPosition position)
        {
            _id = id;
            _position = new(position.X, position.Y, position.Z);
        }

        private Tuple<int, int> GetIdsOnAtlas()
        {
            switch (_id)
            {
                case BlockType.Grass:
                    return new(0, 1);
                case BlockType.Stone:
                    return new(2, 2);
                default:
                    break;
            }

            return new(0, 0);
        }

        public VertexPositionTexture[] GetVertices(ChunkPosition chunkPos)
        {
            List<VertexPositionTexture> vertices = [];

            var ids = GetIdsOnAtlas();
            Vector2[] top = Utils.GetUV(ids.Item1);
            Vector2[] side = Utils.GetUV(ids.Item2);

            var map = Globals.GetMap();

            var pos = _position.ToIntPosition();
            var (x, y, z) = (pos.X + chunkPos.X * Chunk.WIDTH, pos.Y, pos.Z + chunkPos.Z * Chunk.DEPTH);

            if (!map.IsBlock(x, y, z + 1))
            {
                vertices.Add(new(new Vector3(x, y, z + 1), side[0]));
                vertices.Add(new(new Vector3(x + 1, y, z + 1), side[1]));
                vertices.Add(new(new Vector3(x + 1, y + 1, z + 1), side[2]));
                vertices.Add(new(new Vector3(x, y + 1, z + 1), side[3]));
            }

            if (!map.IsBlock(x + 1, y, z))
            {
                vertices.Add(new(new Vector3(x + 1, y, z + 1), side[0])); 
                vertices.Add(new(new Vector3(x + 1, y, z), side[1])); 
                vertices.Add(new(new Vector3(x + 1, y + 1, z), side[2])); 
                vertices.Add(new(new Vector3(x + 1, y + 1, z + 1), side[3]));
            }

            if (!map.IsBlock(x, y, z - 1))
            {
                vertices.Add(new(new Vector3(x + 1, y, z), side[0])); 
                vertices.Add(new(new Vector3(x, y, z), side[1])); 
                vertices.Add(new(new Vector3(x, y + 1, z), side[2])); 
                vertices.Add(new(new Vector3(x + 1, y + 1, z), side[3]));
            }

            if (!map.IsBlock(x - 1, y, z))
            {
                vertices.Add(new(new Vector3(x, y, z), side[0])); 
                vertices.Add(new(new Vector3(x, y, z + 1), side[1])); 
                vertices.Add(new(new Vector3(x, y + 1, z + 1), side[2])); 
                vertices.Add(new(new Vector3(x, y + 1, z), side[3]));
            }

            if (!map.IsBlock(x, y + 1, z))
            {
                vertices.Add(new(new Vector3(x, y + 1, z + 1), top[0])); 
                vertices.Add(new(new Vector3(x + 1, y + 1, z + 1), top[1])); 
                vertices.Add(new(new Vector3(x + 1, y + 1, z), top[2])); 
                vertices.Add(new(new Vector3(x, y + 1, z), top[3]));
            }

            if (!map.IsBlock(x, y - 1, z))
            {
                vertices.Add(new(new Vector3(x, y, z), side[0])); 
                vertices.Add(new(new Vector3(x + 1, y, z), side[1])); 
                vertices.Add(new(new Vector3(x + 1, y, z + 1), side[2])); 
                vertices.Add(new(new Vector3(x, y, z + 1), side[3]));
            }

            return [.. vertices];
        }

        private static readonly IntPosition[] _offsets = [
            new(1, 0, 0),
            new(-1, 0, 0),
            new(0, 1, 0),
            new(0, -1, 0),
            new(0, 0, 1),
            new(0, 0, -1) 
        ];

        public bool IsVisible(ChunkPosition chunkPos)
        {
            var map = Globals.GetMap();
            var pos = _position.ToIntPosition();
            pos.X += chunkPos.X * Chunk.WIDTH;
            pos.Z += chunkPos.Z * Chunk.DEPTH;

            ReadOnlySpan<IntPosition> offsetsSpan = _offsets;

            for (int i = 0; i < offsetsSpan.Length; i++)
            {
                var offset = offsetsSpan[i];
                if (!map.IsBlock(pos.X + offset.X, pos.Y + offset.Y, pos.Z + offset.Z))
                    return true;
            }

            return false;
        }

        public float GetDist(ChunkPosition chunkPos, Vector3 pos2) 
        {
            var pos = _position.ToIntPosition();
            pos.X += chunkPos.X * Chunk.WIDTH;
            pos.Z += chunkPos.Z * Chunk.DEPTH;

            return Math.Abs(pos2.X - pos.X + 0.5f) + Math.Abs(pos2.Y - pos.Y + 0.5f) + Math.Abs(pos2.Z - pos.Z + 0.5f);
        }

    }
}
