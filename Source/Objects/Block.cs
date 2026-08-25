using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Minecraft.Source.Objects
{
    public class Block
    {
        private enum VerticeType
        {
            Front, //+z
            Right, //+x
            Back, //-z
            Left, //-x
            Top, //+y
            Bottom // -y
        }

        public enum BlockType
        {
            Grass
        }

        private readonly BlockType _id;
        private readonly int _x;
        private readonly int _y;
        private readonly int _z;

        private readonly VertexPositionTexture[] _vertices;

        public Block(BlockType id, int x, int y, int z)
        {
            _id = id;
            _x = x;
            _y = y;
            _z = z;
            _vertices = CreateVertices();
        }

        private VertexPositionTexture[] CreateVertices()
        {
            var ids = GetIdsOnAtlas();

            Vector2[] top = Utils.GetUV(ids.Item1);
            Vector2[] side = Utils.GetUV(ids.Item2);

            return
            [
                // FRONT
                new(new Vector3(_x, _y, _z + 1), side[0]),
                new(new Vector3(_x + 1, _y, _z + 1), side[1]),
                new(new Vector3(_x + 1, _y + 1, _z + 1), side[2]),
                new(new Vector3(_x, _y + 1, _z + 1), side[3]),

                // RIGHT
                new(new Vector3(_x + 1, _y, _z + 1), side[0]),
                new(new Vector3(_x + 1, _y, _z), side[1]),
                new(new Vector3(_x + 1, _y + 1, _z), side[2]),
                new(new Vector3(_x + 1, _y + 1, _z + 1), side[3]),

                // BACK
                new(new Vector3(_x + 1, _y, _z), side[0]),
                new(new Vector3(_x, _y, _z), side[1]),
                new(new Vector3(_x, _y + 1, _z), side[2]),
                new(new Vector3(_x + 1, _y + 1, _z), side[3]),

                // LEFT
                new(new Vector3(_x, _y, _z), side[0]),
                new(new Vector3(_x, _y, _z + 1), side[1]),
                new(new Vector3(_x, _y + 1, _z + 1), side[2]),
                new(new Vector3(_x, _y + 1, _z), side[3]),

                // TOP
                new(new Vector3(_x, _y + 1, _z + 1), top[0]),
                new(new Vector3(_x + 1, _y + 1, _z + 1), top[1]),
                new(new Vector3(_x + 1, _y + 1, _z), top[2]),
                new(new Vector3(_x, _y + 1, _z), top[3]),

                // BOTTOM
                new(new Vector3(_x, _y, _z), side[0]),
                new(new Vector3(_x + 1, _y, _z), side[1]),
                new(new Vector3(_x + 1, _y, _z + 1), side[2]),
                new(new Vector3(_x, _y, _z + 1), side[3]),
            ];
        }

        private Tuple<int, int> GetIdsOnAtlas()
        {
            switch (_id)
            {
                case BlockType.Grass:
                    return new(0, 1);
                default:
                    break;
            }

            return new(0, 0);
        }

        public VertexPositionTexture[] GetVertices()
        {
            List<VertexPositionTexture> vertices = [];
            var map = Globals.GetMap();

            foreach (var verticeType in Enum.GetValues<VerticeType>())
            {
                if (verticeType == VerticeType.Front && map.IsBlock(_x, _y, _z + 1))
                    continue;

                if (verticeType == VerticeType.Right && map.IsBlock(_x + 1, _y, _z))
                    continue;

                if (verticeType == VerticeType.Back && map.IsBlock(_x, _y, _z - 1))
                    continue;

                if (verticeType == VerticeType.Left && map.IsBlock(_x - 1, _y, _z))
                    continue;

                if (verticeType == VerticeType.Top && map.IsBlock(_x, _y + 1, _z))
                    continue;

                if (verticeType == VerticeType.Bottom && map.IsBlock(_x, _y - 1, _z))
                    continue;

                vertices.Add(_vertices[(int)verticeType * 4]);
                vertices.Add(_vertices[(int)verticeType * 4 + 1]);
                vertices.Add(_vertices[(int)verticeType * 4 + 2]);
                vertices.Add(_vertices[(int)verticeType * 4 + 3]);
            }

            return [.. vertices];
        }

        public float GetDist(Vector3 pos) => Math.Abs(pos.X - _x + 0.5f) + Math.Abs(pos.Y - _y + 0.5f) + Math.Abs(pos.Z  - _z + 0.5f);
    }
}
