using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Minecraft.Source.Structures;
using System;
using System.Collections.Generic;

namespace Minecraft.Source.Objects
{
    public class Block
    {
        public enum BlockType
        {
            Grass,
            Stone
        }

        private readonly BlockType _id;

        private readonly IntPosition _position;

        public Block(BlockType id, IntPosition position)
        {
            _id = id;
            _position = position;
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

        public VertexPositionTexture[] GetVertices()
        {
            List<VertexPositionTexture> vertices = [];

            var ids = GetIdsOnAtlas();
            Vector2[] top = Utils.GetUV(ids.Item1);
            Vector2[] side = Utils.GetUV(ids.Item2);

            var map = Globals.GetMap();

            var (x, y, z) = (_position.X, _position.Y, _position.Z);

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

        private static readonly List<IntPosition> _offsets = [
            new(1, 0, 0),
            new(0, 1, 0),
            new(-1, 0, 0),
            new(0, -1, 0),
            new(0, 0, 1),
            new(1, 0, 0)
        ];

        public bool IsVisible()
        {
            var map = Globals.GetMap();

            foreach (var offset in _offsets)
            {
                if (!map.IsBlock(_position.X + offset.X, _position.Y + offset.Y, _position.Z + offset.Z))
                    return true;
            }

            return false;
        }

        public float GetDist(Vector3 pos) 
        {
            var (x, y, z) = (_position.X, _position.Y, _position.Z);

            return Math.Abs(pos.X - x + 0.5f) + Math.Abs(pos.Y - y + 0.5f) + Math.Abs(pos.Z - z + 0.5f);
        }

    }
}
