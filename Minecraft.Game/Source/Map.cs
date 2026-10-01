using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Minecraft.Source.Objects;
using Minecraft.Source.Structures;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Minecraft.Source.BlockProperties;

namespace Minecraft.Source
{
    public sealed class Map
    {
        private readonly Dictionary<ChunkPosition, Chunk> _chunks;
        private readonly ConcurrentBag<ChunkPosition> _visibleChunks;
        private readonly List<Entity> _entities;

        public ChunkPosition? PreviousPlayerChunk;

        private const int RENDER_DISTANCE = 40;

        private List<ChunkPosition> _chunkOrder;
        private readonly SemaphoreSlim _chunkCreationLock = new(1, 1);
        private bool _firstChunkCreation = true;

        public ConcurrentQueue<(Chunk, GreedyMesher.MeshData)> AwaitingMeshData = [];

        public Map()
        {
            _chunks = [];
            _visibleChunks = [];

            _entities = [];

            GenerateChunkOrder();

            var player = new Player();
            player.SetPosition(0, 75, 0);
            Globals.SetPlayer(player);
            _entities.Add(player);

            var noise = Globals.GetNoise();
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFrequency(0.005f);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetFractalOctaves(4);
        }

        ~Map()
        {
            _chunks.Clear();
            _visibleChunks.Clear();
            _entities.Clear();
        }

        private List<Chunk> GetVisibleChunks()
        {
            var view = Globals.GetCamera().GetView();

            BoundingFrustum frustum = new(view * Globals.GetProjection());
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

        public Block GetBlock(int x, int y, int z)
        {
            Chunk chunk;
            if ((chunk = GetChunkOnPos(new IntPosition(x, y, z))) == null)
                return default;

            return chunk.GetBlock(x.Mod(Chunk.WIDTH), y, z.Mod(Chunk.DEPTH));
        }

        public bool IsBlock(int x, int y, int z, bool checkTransparent = false)
        {
            Chunk chunk;
            if ((chunk = GetChunkOnPos(new IntPosition(x, y, z))) == null) 
                return false;

            return chunk.IsBlock(x.Mod(Chunk.WIDTH), y, z.Mod(Chunk.DEPTH), checkTransparent);
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
            await _chunkCreationLock.WaitAsync();

            try
            {
                var chunkIds = new List<ChunkPosition>();
                var missingIds = new List<ChunkPosition>();

                foreach (var chunkOrder in _chunkOrder)
                {
                    var x = pos.X + chunkOrder.X * Chunk.WIDTH;
                    var z = pos.Z + chunkOrder.Z * Chunk.DEPTH;

                    var id = GetChunkIdFromPos(x, pos.Y, z);

                    chunkIds.Add(id);

                    if (!_chunks.ContainsKey(id))
                        missingIds.Add(id);
                }

                var generated = await Task.WhenAll(missingIds.Select(id =>
                    Task.Run(() => {
                        var chunk = new Chunk(id);
                        return (Id: id, Chunk: chunk);
                    })
                ));

                foreach (var item in generated)
                    _chunks.Add(item.Id, item.Chunk);

                _visibleChunks.Clear();

                foreach (var id in chunkIds)
                    _visibleChunks.Add(id);

                var chunksMesh = new HashSet<Chunk>();

                foreach (var item in generated)
                {
                    chunksMesh.Add(item.Chunk);

                    if (!_firstChunkCreation)
                    {
                        foreach (var offset in new ChunkPosition[] { new(0, 1), new(1, 0), new(0, -1), new(-1, 0) })
                        {
                            if (!_chunks.TryGetValue(item.Id + offset, out var chunk2))
                                continue;

                            chunksMesh.Add(chunk2);
                        }
                    }
                }

                await Task.WhenAll(chunksMesh.Select(chunk =>
                    Task.Run(() => {
                        chunk.RefreshMesh();
                        chunk.SetActive();
                    })
                ));

                _firstChunkCreation = false;
            }
            finally
            {
                _chunkCreationLock.Release();
            }
        }

        public void Update(GameTime gameTime) 
        {
            var pos = Globals.GetPlayer().GetPosition();
            var chunkPos = GetChunkIdFromPos(pos);
            var graphicsDevice = Globals.GetGraphics().GraphicsDevice;

            while (AwaitingMeshData.TryDequeue(out var meshData))
            {
                meshData.Item1.UpdateMeshData(meshData.Item2);
            }
            
            if (PreviousPlayerChunk == null || PreviousPlayerChunk != chunkPos)
            {
                CreateChunks(pos);
                PreviousPlayerChunk = chunkPos;
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

            Globals.GetEffect().Parameters["View"].SetValue(camera.GetView());

            graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            var visibleChunks = GetVisibleChunks();
            var vertices = 0;

            foreach (EffectPass pass in Globals.GetEffect().CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (var chunk in visibleChunks)
                {
                    if (chunk.IndexCount == 0 || chunk.VertexBuffer == null)
                        continue;

                    graphicsDevice.SetVertexBuffer(chunk.VertexBuffer);
                    graphicsDevice.Indices = chunk.IndexBuffer;

                    vertices += chunk.VertexBuffer.VertexCount;

                    graphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        0,
                        0,
                        chunk.IndexCount / 3
                    );
                }
            }

            Globals.GetHud().OnRenderData(_chunks.Count, visibleChunks.Count, vertices);
        }
    }
}
