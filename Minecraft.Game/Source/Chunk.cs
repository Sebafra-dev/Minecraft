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

        private Block[,,] _blocks;
        private readonly ChunkPosition _position;

        private bool _generated = false;

        private MeshData _mesh;

        public Chunk(ChunkPosition position)
        {
            _position = position;

            _blocks = new Block[WIDTH, HEIGHT, DEPTH];

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
                }
            }
        }

        public void AddBlockOnChunk(BlockType blockType, IntPosition position)
        {
            var (chunkX, chunkY, chunkZ) = (position.X.Mod(WIDTH), position.Y, position.Z.Mod(DEPTH));
            if (IsBlock(chunkX, chunkY, chunkZ))
                return;

            _blocks[chunkX, chunkY, chunkZ] = new Block(blockType, new(chunkX, chunkY, chunkZ));
        }

        public void RefreshMesh()
        {
            _mesh = Build(this);
        } 

        public bool IsBlock(int x, int y, int z) => y < 0 || y >= HEIGHT || _blocks[x, y, z] != null;

        public bool IsActive() => _generated;

        public void SetInactive()
        {
            _generated = false;
        }

        public void SetActive()
        {
            _generated = true;
        }

        public Block GetBlock(int x, int y, int z)
        {
            if (x < 0 || x >= WIDTH ||
                y < 0 || y >= HEIGHT ||
                z < 0 || z >= DEPTH)
                return null;

            return _blocks[x, y, z];
        }

        public MeshData GetMeshData() => _mesh;
    }
}
