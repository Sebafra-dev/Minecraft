using Microsoft.Xna.Framework.Graphics;
using Minecraft.Source.Objects;
using Minecraft.Source.Structures;
using System;
using static Minecraft.Source.GreedyMesher;
using static Minecraft.Source.Objects.Block;

namespace Minecraft.Source
{
    public class Chunk
    {
        public const int WIDTH = 16;
        public const int HEIGHT = 256;
        public const int DEPTH = 16;

        private Block[] _blocks;
        private readonly ChunkPosition _position;

        private bool _generated = false;

        public VertexBuffer VertexBuffer { get; private set; }
        public IndexBuffer IndexBuffer { get; private set; }
        public int IndexCount { get; private set; }

        public Chunk(ChunkPosition position)
        {
            _position = position;

            _blocks = new Block[WIDTH * HEIGHT * DEPTH];

            GenerateChunk();
        }

        ~Chunk()
        {
            _blocks = null;
        }

        public ChunkPosition GetPosition() => _position;

        private void GenerateChunk()
        {
            var chunkX = _position.X * WIDTH;
            var chunkZ = _position.Z * DEPTH;
            for (int x = 0; x < WIDTH; x++)
            {
                for (int z = 0; z < DEPTH; z++)
                {
                    int globalX = x + chunkX;
                    int globalZ = z + chunkZ;

                    float noiseValue = Globals.GetNoise().GetNoise(globalX, globalZ);

                    int baseHeight = 70;
                    int maxVariancy = 40;
                    int terrainHeight = baseHeight + (int)(noiseValue * maxVariancy);
                    terrainHeight = Math.Clamp(terrainHeight, 0, HEIGHT - 1);

                    for (int y = 0; y <= terrainHeight; y++)
                    {
                        BlockType blockType = (y == terrainHeight) ? BlockType.Grass : BlockType.Stone;

                        AddBlockOnChunk(blockType, new IntPosition(globalX, y, globalZ));
                    }

                    //TREE
                    if (Utils.GetHashOnPosition(globalX, globalZ, Globals.SEED) < 0.004)
                    {
                        SpawnTree(new(globalX, terrainHeight, globalZ));
                    }
                    //RED FLOWER
                    else if (Utils.GetHashOnPosition(globalX, globalZ, Globals.SEED) < 0.008)
                    {
                        AddBlockOnChunk(BlockType.RedFlower, new(globalX, terrainHeight + 1, globalZ));
                    }
                    //YELLOW FLOWER
                    else if (Utils.GetHashOnPosition(globalX, globalZ, Globals.SEED) < 0.012)
                    {
                        AddBlockOnChunk(BlockType.YellowFlower, new(globalX, terrainHeight + 1, globalZ));
                    }
                    //SMALL GRASS
                    else if (Utils.GetHashOnPosition(globalX, globalZ, Globals.SEED) < 0.024)
                    {
                        AddBlockOnChunk(BlockType.SmallGrass, new(globalX, terrainHeight + 1, globalZ));
                    }
                }
            }
        }

        public void SwitchBlockOnChunk(BlockType blockType, IntPosition position)
        {
            var (chunkX, chunkY, chunkZ) = (position.X.Mod(WIDTH), position.Y, position.Z.Mod(DEPTH));

            _blocks[GetBlockIndex(chunkX, chunkY, chunkZ)].SetBlockType(blockType);
        }

        public void AddBlockOnChunk(BlockType blockType, IntPosition position)
        {
            if (position.X >= _position.X * WIDTH + WIDTH || position.X < _position.X * WIDTH ||
                position.Z >= _position.Z * DEPTH + DEPTH || position.Z < _position.Z * DEPTH)
                return;

            var (chunkX, chunkY, chunkZ) = (position.X.Mod(WIDTH), position.Y, position.Z.Mod(DEPTH));
            if (IsBlock(chunkX, chunkY, chunkZ))
                return;

            _blocks[GetBlockIndex(chunkX, chunkY, chunkZ)] = new Block(blockType);
        }

        private void SpawnTree(IntPosition startPos)
        {
            SwitchBlockOnChunk(BlockType.Dirt, startPos);

            for (int dy = 1; dy <= 5; dy++)
                AddBlockOnChunk(BlockType.Wood, startPos + new IntPosition(0, dy, 0));

            for (int dy = 0; dy < 3; dy++)
                for (int dx = -2; dx <= 2; dx++)
                    for (int dz = -2; dz <= 2; dz++)
                    {
                        AddBlockOnChunk(BlockType.Leaf, startPos + new IntPosition(dx, 5 + dy, dz));
                    }

            for (int dx = -1; dx <= 1; dx++)
                for (int dz = -1; dz <= 1; dz++)
                {
                    AddBlockOnChunk(BlockType.Leaf, startPos + new IntPosition(dx, 5 + 3, dz));
                }
        }

        public void RefreshMesh()
        {
            Globals.GetMap().AwaitingMeshData.Enqueue((this, Build(this)));
        }

        public void UpdateMeshData(MeshData mesh)
        {
            var graphicsDevice = Globals.GetGraphics().GraphicsDevice;

            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();

            IndexCount = mesh.Indices.Count;

            if (IndexCount == 0)
                return;

            VertexBuffer = new VertexBuffer(graphicsDevice, VoxelVertex.VertexDeclaration, mesh.Vertices.Count, BufferUsage.WriteOnly);
            VertexBuffer.SetData(mesh.Vertices.ToArray());

            IndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, mesh.Indices.Count, BufferUsage.WriteOnly);
            IndexBuffer.SetData(mesh.Indices.ToArray());
        }

        private static int GetBlockIndex(int x, int y, int z) => x + (z << 4) + (y << 8);

        public bool IsBlock(int x, int y, int z, bool checkTransparent = false)
        {
            if (y < 0 || y >= HEIGHT)
                return false;

            var block = GetBlock(x, y, z);
            var blockProperties = block.GetProperties();

            if (blockProperties.Object)
                return false;

            if (!blockProperties.Visible)
                return false;

            if (checkTransparent && blockProperties.Transparent) 
                return false;

            return true;
        }

        public bool IsActive() => _generated;

        public void SetInactive()
        {
            _generated = false;
        }

        public void SetActive()
        {
            _generated = true;
        }

        public Block GetBlock(int x, int y, int z) => _blocks[GetBlockIndex(x, y, z)];
    }
}
