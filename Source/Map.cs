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
        private readonly List<Block> _blocks;
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

            _blocks = [];
            _chunks = [];

            for (int x = -15; x < 15; x++)
                for (int z = -15; z < 15; z++)
                    for (int y = 0; y < 70; y++)
                    {
                        AddBlockOnMap(y > 65 ? BlockType.Grass : BlockType.Stone, x, y, z);
                    }

            _vertices = [];
            _indices = [];
        }

        ~Map()
        {
            _blocks.Clear();
            _chunks.Clear();
            _vertices.Clear();
            _indices.Clear();
        }

        private static (short, short) GetChunkId(int x, int y, int z) => ((short)(x / Chunk.WIDTH), (short)(z / Chunk.DEPTH));

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
            }

            _effect.View = camera.GetView();

            graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            _blocks.Clear();

            foreach (var chunk in _chunks.Values)
                _blocks.AddRange(chunk.GetVisibleBlocks());

            var sortedBlocks = _blocks.OrderByDescending(_ => _.GetDist(camera.GetCameraPos()));

            _vertices.Clear();
            _indices.Clear();
            var ids = 0;

            foreach (var block in sortedBlocks)
            {
                var vertices = block.GetVertices();
                _vertices.AddRange(vertices);

                for (int i = 0; i < vertices.Length / 4; i++)
                {
                    int offset = ids + i * 4;

                    _indices.AddRange([
                        offset, offset + 2, offset + 1,
                        offset, offset + 3, offset + 2
                    ]);
                }

                ids += vertices.Length;
            }

            Globals.GetHud().OnRenderData(_chunks.Count, _blocks.Count, _vertices.Count);

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Globals.GetGraphics().GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _vertices.ToArray(),
                    0,
                    _vertices.Count,
                    _indices.ToArray(),
                    0,
                    _indices.Count / 3
                );
            }
        }
    }
}
