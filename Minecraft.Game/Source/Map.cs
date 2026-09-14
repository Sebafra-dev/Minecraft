using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Minecraft.Source.Objects;
using Minecraft.Source.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Minecraft.Source.Objects.Block;

namespace Minecraft.Source
{
    public sealed class Map
    {
        private readonly BasicEffect _effect;

        private readonly Dictionary<ChunkPosition, Chunk> _chunks;
        private readonly List<Tuple<Block, Chunk>> _visibleBlocks;
        private readonly List<ChunkPosition> _visibleChunks;
        private readonly List<VertexPositionTexture> _vertices;
        private readonly List<int> _indices;
        private readonly List<Entity> _entities;

        private ChunkPosition? _previousPlayerChunk;

        private bool _initialized = false;

        private const int RENDER_DISTANCE = 10;

        private List<ChunkPosition> _chunkOrder;

        public Map()
        {
            var graphicsDevice = Globals.GetGraphics().GraphicsDevice;

            _effect = new BasicEffect(graphicsDevice)
            {
                TextureEnabled = true,
                World = Matrix.Identity,
                Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), graphicsDevice.Viewport.AspectRatio, 0.1f, 1000f)
            };

            _chunks = [];
            _visibleBlocks = [];
            _visibleChunks = [];

            _vertices = [];
            _indices = [];

            _entities = [];

            GenerateChunkOrder();

            var player = new Player();
            player.SetPosition(0, 75, 0);
            Globals.SetPlayer(player);
            _entities.Add(player);

            var noise = Globals.GetNoise();
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFrequency(0.01f);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetFractalOctaves(4);
        }

        ~Map()
        {
            _chunks.Clear();
            _visibleBlocks.Clear();
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
                var x = chunkPos.X * Chunk.WIDTH;
                var z = chunkPos.Z * Chunk.DEPTH;

                var min = new Vector3(x, 0, z);
                var max = new Vector3(x + Chunk.WIDTH, Chunk.HEIGHT, z + Chunk.DEPTH);

                var bounds = new BoundingBox(min, max);

                if (frustum.Contains(bounds) == ContainmentType.Disjoint)
                    continue;

                chunks.Add(chunk);
            }

            return chunks;
        }

        private static ChunkPosition GetChunkIdFromPos(Vector3 pos) => GetChunkIdFromPos(pos.X, pos.Y, pos.Z);

        private static ChunkPosition GetChunkIdFromPos(float x, float y, float z) => new((int)Math.Floor(x / Chunk.WIDTH), (int)Math.Floor(z / Chunk.DEPTH));

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

            return chunk.IsBlock(x.Mod(Chunk.WIDTH), y, z.Mod(Chunk.DEPTH));
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

        public Chunk GetChunkOnChunkPos(ChunkPosition chunkPos)
        {
            if (!_chunks.TryGetValue(chunkPos, out Chunk chunk))
                return null;

            return chunk;
        }

        private void GenerateChunkOrder()
        {
            _chunkOrder = [];

            for (int i = -RENDER_DISTANCE; i <= RENDER_DISTANCE; i++)
            {
                for (int j = -RENDER_DISTANCE; j <= RENDER_DISTANCE; j++)
                {
                    _chunkOrder.Add(new(i, j));
                }
            }

            _chunkOrder = [.. _chunkOrder.OrderBy(_ => Math.Abs(_.X) + Math.Abs(_.Z))];
        }

        public async void CreateChunks(Vector3 pos)
        {
            var chunkIds = new List<ChunkPosition>();
            var newChunks = new List<ChunkPosition>();

            foreach (var chunkOrder in _chunkOrder)
            {
                var x = pos.X + chunkOrder.X * Chunk.WIDTH;
                var z = pos.Z + chunkOrder.Z * Chunk.DEPTH;

                var id = GetChunkIdFromPos(x, pos.Y, z);
                chunkIds.Add(id);

                if (_chunks.ContainsKey(id))
                    continue;

                var chunk = await Task.Run(() => new Chunk(id));

                newChunks.Add(id);
                _chunks.Add(id, chunk);

            }

            _visibleChunks.Clear();
            _visibleChunks.AddRange(chunkIds);

            foreach (var newChunkId in newChunks)
            {
                var chunk = _chunks[newChunkId];
                chunk.RefreshVisibleBlocks();
                chunk.SetActive();

                foreach (var chunkOffset in new ChunkPosition[4] { new(0, 1), new(1, 0), new(0, -1), new(-1, 0) })
                {
                    if (!_chunks.TryGetValue(newChunkId + chunkOffset, out Chunk chunk2))
                        continue;

                    if (chunkOffset == new ChunkPosition(1, 0))
                        chunk2.RefreshBoundaries(Chunk.BoundaryType.PlusX);
                    else if(chunkOffset == new ChunkPosition(-1, 0))
                        chunk2.RefreshBoundaries(Chunk.BoundaryType.MinusX);
                    else if (chunkOffset == new ChunkPosition(0, 1))
                        chunk2.RefreshBoundaries(Chunk.BoundaryType.PlusZ);
                    else if (chunkOffset == new ChunkPosition(0, -1))
                        chunk2.RefreshBoundaries(Chunk.BoundaryType.MinusZ);
                }
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

            var visibleChunks = GetVisibleChunks();

            _visibleBlocks.Clear();

            foreach (var chunk in visibleChunks)
            {
                var blocks = chunk.GetVisibleBlocks();
                foreach (var block in blocks)
                {
                    _visibleBlocks.Add(new(block, chunk));
                }
            }

            _vertices.Clear();
            _indices.Clear();
            var ids = 0;

            var sortedBlocks = _visibleBlocks.OrderByDescending(_ => _.Item1.GetDist(_.Item2.GetPosition(), camera.GetCameraPos()));

            foreach (var (block, chunk) in sortedBlocks)
            {
                var blockVertices = block?.GetVertices(chunk.GetPosition());
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

            Globals.GetHud().OnRenderData(_chunks.Count, visibleChunks.Count, _visibleBlocks.Count, _vertices.Count);

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
