using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Minecraft.Source.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Minecraft.Source.Objects.Block;

namespace Minecraft.Source
{
    public sealed class Map
    {
        private readonly BasicEffect _effect;

        private readonly Dictionary<(short, short), Chunk> _chunks;
        private readonly List<Block> _visisbleBlocks;
        private readonly List<VertexPositionTexture> _vertices;
        private readonly List<int> _indices;

        private bool _initialized = false;

        public Map()
        {
            var graphicsDevice = Globals.GetGraphics().GraphicsDevice;

            _effect = new BasicEffect(graphicsDevice)
            {
                TextureEnabled = true,
                World = Matrix.Identity,
                Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), graphicsDevice.Viewport.AspectRatio, 0.1f, 100f)
            };

            _visisbleBlocks = [];
            _chunks = [];

            for (int x = -100; x < 100; x++)
                for (int z = -100; z < 100; z++)
                    for (int y = 0; y < 70; y++)
                    {
                        AddBlockOnMap(y > 67 ? BlockType.Grass : BlockType.Stone, x, y, z);
                    }

            _vertices = [];
            _indices = [];
        }

        ~Map()
        {
            _visisbleBlocks.Clear();
            _chunks.Clear();
            _vertices.Clear();
            _indices.Clear();
        }

        private void RefreshAllChunks()
        {
            foreach (var chunk in _chunks.Values)
                chunk.RefreshVisibleBlocks();
        }

        private List<Chunk> GetVisibleChunks()
        {
            BoundingFrustum frustum = new(_effect.View * _effect.Projection);
            var chunks = new List<Chunk>();

            foreach (var chunk in _chunks)
            {
                var x = chunk.Key.Item1 * Chunk.WIDTH;
                var z = chunk.Key.Item2 * Chunk.DEPTH;

                var min = new Vector3(x, 0, z);
                var max = new Vector3(x + Chunk.WIDTH, Chunk.HEIGHT, z + Chunk.DEPTH);

                var bounds = new BoundingBox(min, max);

                if (frustum.Contains(bounds) == ContainmentType.Disjoint)
                    continue;

                chunks.Add(chunk.Value);
            }

            return chunks;
        }

        private static (short, short) GetChunkId(int x, int y, int z) => ((short)Math.Ceiling((float)x / Chunk.WIDTH), (short)Math.Ceiling((float)z / Chunk.DEPTH));

        public void AddBlockOnMap(BlockType blockType, int x, int y, int z)
        {
            var id = GetChunkId(x, y, z);

            if (!_chunks.ContainsKey(id))
                _chunks.Add(id, new Chunk());

            if (IsBlock(x, y, z))
                return;

            _chunks[id].AddBlockOnChunk(blockType, x, y, z);
        }

        public bool IsBlock(int x, int y, int z)
        {
            var id = GetChunkId(x, y, z);

            if (!_chunks.TryGetValue(id, out Chunk value))
                return false;

            return value.IsBlock(x.Mod(Chunk.WIDTH), y.Mod(Chunk.HEIGHT), z.Mod(Chunk.DEPTH));
        }   

        public void Draw()
        {
            var graphicsDevice = Globals.GetGraphics().GraphicsDevice;
            var camera = Globals.GetCamera();

            if (!_initialized)
            {
                _initialized = true;
                _effect.Texture = Globals.GetTexture();
                RefreshAllChunks();
            }

            _effect.View = camera.GetView();

            graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            _visisbleBlocks.Clear();

            var visibleChunks = GetVisibleChunks();

            foreach (var chunk in visibleChunks)
                _visisbleBlocks.AddRange(chunk.GetVisibleBlocks());

            var sortedBlocks = _visisbleBlocks.OrderByDescending(_ => _.GetDist(camera.GetCameraPos()));

            _vertices.Clear();
            _indices.Clear();
            var ids = 0;

            foreach (var block in sortedBlocks)
            {
                var blockVertices = block.GetVertices();
                _vertices.AddRange(blockVertices);

                for (int i = 0; i < blockVertices.Length / 4; i++)
                {
                    int offset = ids + i * 4;

                    _indices.AddRange([
                        offset, offset + 2, offset + 1,
                        offset, offset + 3, offset + 2
                    ]);
                }

                ids += blockVertices.Length;
            }

            Globals.GetHud().OnRenderData(_chunks.Count, visibleChunks.Count, _visisbleBlocks.Count, _vertices.Count);

            if (_vertices.Count > 0)
            {
                var vertices = _vertices.ToArray();
                var indices = _indices.ToArray();

                foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    Globals.GetGraphics().GraphicsDevice.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        vertices,
                        0,
                        _vertices.Count,
                        indices,
                        0,
                        _indices.Count / 3
                    );
                }
            }
        }
    }
}
