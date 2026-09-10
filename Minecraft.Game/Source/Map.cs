using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Minecraft.Source.Objects;
using Minecraft.Source.Structures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static Minecraft.Source.Objects.Block;

namespace Minecraft.Source
{
    public sealed class Map
    {
        private readonly BasicEffect _effect;

        private readonly Dictionary<(short, short), Chunk> _chunks;
        private readonly List<(short, short)> _visibleChunks;
        private readonly List<Block> _visisbleBlocks;
        private readonly List<VertexPositionTexture> _vertices;
        private readonly List<int> _indices;
        private readonly List<Entity> _entities;

        private (short, short)? _previousPlayerChunk;

        private bool _initialized = false;

        private const int RENDER_DISTANCE = 3;

        public Map()
        {
            var graphicsDevice = Globals.GetGraphics().GraphicsDevice;

            _effect = new BasicEffect(graphicsDevice)
            {
                TextureEnabled = true,
                World = Matrix.Identity,
                Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), graphicsDevice.Viewport.AspectRatio, 0.1f, 1000f)
            };

            _visisbleBlocks = [];
            _chunks = [];
            _visibleChunks = [];

            _vertices = [];
            _indices = [];

            _entities = [];

            var player = new Player();
            player.SetPosition(0, 75, 0);
            Globals.SetPlayer(player);
            _entities.Add(player);
        }

        ~Map()
        {
            _visisbleBlocks.Clear();
            _chunks.Clear();
            _visibleChunks.Clear();
            _vertices.Clear();
            _indices.Clear();
            _entities.Clear();
        }

        private List<Chunk> GetVisibleChunks()
        {
            BoundingFrustum frustum = new(_effect.View * _effect.Projection);
            var chunks = new List<Chunk>();

            foreach (var chunkId in _visibleChunks)
            {
                var chunk = _chunks[chunkId];
                if (!chunk.IsActive())
                    continue;

                var chunkPos = chunk.GetPosition();
                var x = chunkPos.Item1 * Chunk.WIDTH;
                var z = chunkPos.Item2 * Chunk.DEPTH;

                var min = new Vector3(x, 0, z);
                var max = new Vector3(x + Chunk.WIDTH, Chunk.HEIGHT, z + Chunk.DEPTH);

                var bounds = new BoundingBox(min, max);

                if (frustum.Contains(bounds) == ContainmentType.Disjoint)
                    continue;

                chunks.Add(chunk);
            }

            return chunks;
        }

        private static (short, short) GetChunkIdFromPos(Vector3 pos) => GetChunkIdFromPos(pos.X, pos.Y, pos.Z);

        private static (short, short) GetChunkIdFromPos(float x, float y, float z) => ((short)Math.Floor(x / Chunk.WIDTH), (short)Math.Floor(z / Chunk.DEPTH));

        public void AddBlockOnMap(BlockType blockType, IntPosition position)
        {
            Chunk chunk;
            if ((chunk = GetChunkOnPos(position)) == null)
                return;

            chunk.AddBlockOnChunk(blockType, position);
        }

        public bool IsBlock(int x, int y, int z)
        {
            Chunk chunk;
            if ((chunk = GetChunkOnPos(new IntPosition(x, y, z))) == null) 
                return false;

            return chunk.IsBlock(x.Mod(Chunk.WIDTH), y.Mod(Chunk.HEIGHT), z.Mod(Chunk.DEPTH));
        }

        public Chunk GetChunkOnPos(float x, float y, float z) => GetChunkOnPos(new Vector3(x, y, z));

        public Chunk GetChunkOnPos(IntPosition position) => GetChunkOnPos(position.ToVec3());

        public Chunk GetChunkOnPos(Vector3 pos)
        {
            var id = GetChunkIdFromPos(pos.X, pos.Y, pos.Z);

            if (!_chunks.TryGetValue(id, out Chunk chunk))
                return null;

            return chunk;
        }

        public Chunk GetChunkOnChunkPos((short, short) chunkPos)
        {
            if (!_chunks.TryGetValue(chunkPos, out Chunk chunk))
                return null;

            return chunk;
        }

        public async Task CreateChunks(Vector3 pos)
        {
            var chunkIds = new List<(short, short)>();

            for (int i = -RENDER_DISTANCE; i <= RENDER_DISTANCE; i++)
            {
                for (int j = -RENDER_DISTANCE; j <= RENDER_DISTANCE; j++)
                {
                    var x = pos.X + i * Chunk.WIDTH;
                    var z = pos.Z + j * Chunk.DEPTH;

                    var id = GetChunkIdFromPos(x, pos.Y, z);
                    chunkIds.Add(id);

                    if (_chunks.ContainsKey(id))
                        continue;

                    var chunk = await Task.Run(() => new Chunk(id));

                    _chunks.Add(id, chunk);
                }
            }

            _visibleChunks.Clear();
            _visibleChunks.AddRange(chunkIds);

            foreach (var id in _visibleChunks)
            {
                var chunk = _chunks[id];
                chunk.RefreshVisibleBlocks();
                chunk.SetActive();
            }
        }

        public void Update(GameTime gameTime) 
        {
            var pos = Globals.GetPlayer().GetPosition();
            var chunkPos = GetChunkIdFromPos(pos);
            

            if (_previousPlayerChunk == null || _previousPlayerChunk != chunkPos)
            {
                CreateChunks(pos);
                _previousPlayerChunk = chunkPos;
            }

            foreach (var entity in _entities) //TODO update per active chunk not entire map
            {
                entity.Update(gameTime);
            }
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

            _visisbleBlocks.Clear();

            var visibleChunks = GetVisibleChunks();

            foreach (var chunk in visibleChunks)
                _visisbleBlocks.AddRange(chunk.GetVisibleBlocks());

            var sortedBlocks = _visisbleBlocks; // _visisbleBlocks.OrderByDescending(_ => _.GetDist(camera.GetCameraPos()));

            _vertices.Clear();
            _indices.Clear();
            var ids = 0;

            foreach (var block in sortedBlocks)
            {
                var blockVertices = block?.GetVertices();
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
