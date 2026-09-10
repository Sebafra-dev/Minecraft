using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Minecraft.Source.Structures;
using System;
using System.Collections.Generic;

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
            Grass,
            Stone
        }

        private readonly BlockType _id;

        private readonly IntPosition _position;

        private readonly VertexPositionTexture[] _vertices;

        public Block(BlockType id, IntPosition position)
        {
            _id = id;
            _position = position;
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
                new(new Vector3(_position.X, _position.Y, _position.Z + 1), side[0]),
                new(new Vector3(_position.X + 1, _position.Y, _position.Z + 1), side[1]),
                new(new Vector3(_position.X + 1, _position.Y + 1, _position.Z + 1), side[2]),
                new(new Vector3(_position.X, _position.Y + 1, _position.Z + 1), side[3]),

                // RIGHT
                new(new Vector3(_position.X + 1, _position.Y, _position.Z + 1), side[0]),
                new(new Vector3(_position.X + 1, _position.Y, _position.Z), side[1]),
                new(new Vector3(_position.X + 1, _position.Y + 1, _position.Z), side[2]),
                new(new Vector3(_position.X + 1, _position.Y + 1, _position.Z + 1), side[3]),

                // BACK
                new(new Vector3(_position.X + 1, _position.Y, _position.Z), side[0]),
                new(new Vector3(_position.X, _position.Y, _position.Z), side[1]),
                new(new Vector3(_position.X, _position.Y + 1, _position.Z), side[2]),
                new(new Vector3(_position.X + 1, _position.Y + 1, _position.Z), side[3]),

                // LEFT
                new(new Vector3(_position.X, _position.Y, _position.Z), side[0]),
                new(new Vector3(_position.X, _position.Y, _position.Z + 1), side[1]),
                new(new Vector3(_position.X, _position.Y + 1, _position.Z + 1), side[2]),
                new(new Vector3(_position.X, _position.Y + 1, _position.Z), side[3]),

                // TOP
                new(new Vector3(_position.X, _position.Y + 1, _position.Z + 1), top[0]),
                new(new Vector3(_position.X + 1, _position.Y + 1, _position.Z + 1), top[1]),
                new(new Vector3(_position.X + 1, _position.Y + 1, _position.Z), top[2]),
                new(new Vector3(_position.X, _position.Y + 1, _position.Z), top[3]),

                // BOTTOM
                new(new Vector3(_position.X, _position.Y, _position.Z), side[0]),
                new(new Vector3(_position.X + 1, _position.Y, _position.Z), side[1]),
                new(new Vector3(_position.X + 1, _position.Y, _position.Z + 1), side[2]),
                new(new Vector3(_position.X, _position.Y, _position.Z + 1), side[3]),
            ];
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
            var map = Globals.GetMap();

            foreach (var verticeType in Enum.GetValues<VerticeType>())
            {
                if (verticeType == VerticeType.Front && map.IsBlock(_position.X, _position.Y, _position.Z + 1))
                    continue;

                if (verticeType == VerticeType.Right && map.IsBlock(_position.X + 1, _position.Y, _position.Z))
                    continue;

                if (verticeType == VerticeType.Back && map.IsBlock(_position.X, _position.Y, _position.Z - 1))
                    continue;

                if (verticeType == VerticeType.Left && map.IsBlock(_position.X - 1, _position.Y, _position.Z))
                    continue;

                if (verticeType == VerticeType.Top && map.IsBlock(_position.X, _position.Y + 1, _position.Z))
                    continue;

                if (verticeType == VerticeType.Bottom && map.IsBlock(_position.X, _position.Y - 1, _position.Z))
                    continue;

                vertices.Add(_vertices[(int)verticeType * 4]);
                vertices.Add(_vertices[(int)verticeType * 4 + 1]);
                vertices.Add(_vertices[(int)verticeType * 4 + 2]);
                vertices.Add(_vertices[(int)verticeType * 4 + 3]);
            }

            return [.. vertices];
        }

        public float GetDist(Vector3 pos) 
        {
            return Math.Abs(pos.X - _position.X + 0.5f) + Math.Abs(pos.Y - _position.Y + 0.5f) + Math.Abs(pos.Z - _position.Z + 0.5f);
        }

    }
}
