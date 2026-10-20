using Minecraft.Source.Objects;
using Minecraft.Source.Structures;
using System;
using static Minecraft.Source.BlockProperties;
using static Minecraft.Source.GreedyMesher;

namespace Minecraft.Source
{
    public class Chunk
    {
        public const int WIDTH = 16;
        public const int HEIGHT = 256;
        public const int DEPTH = 16;

        private ChunkPosition _position;

        private bool _generated = false;

        public ChunkSection[] ChunkSections { get; private set; }

        public Chunk()
        {
            ChunkSections = new ChunkSection[HEIGHT / ChunkSection.HEIGHT];
        }

        ~Chunk()
        {
            ChunkSections = null;
        }

        public void ResetChunk(ChunkPosition position)
        {
            SetInactive();
            _position = position;

            foreach (var chunkSection in ChunkSections)
                chunkSection?.ResetBlocks();

            GenerateChunk();
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
                    int terrainHeight = baseHeight + (int)((noiseValue + 0.2f) * maxVariancy);
                    terrainHeight = Math.Clamp(terrainHeight, 0, HEIGHT - 1);

                    for (int y = 0; y <= terrainHeight; y++)
                    {
                        BlockType blockType = (y == terrainHeight) ? BlockType.Grass : BlockType.Stone;

                        AddBlockOnChunk(blockType, new(globalX, y, globalZ));
                    }

                    for (int y = terrainHeight + 1; y < baseHeight; y++)
                    {
                        AddBlockOnChunk(BlockType.Water, new(globalX, y, globalZ));
                    }

                    if (GetBlock(x, terrainHeight + 1, z).GetBlockType() != BlockType.Water)
                    {
                        //TREE
                        if (Utils.GetHashOnPosition(globalX, globalZ) < 0.004)
                        {
                            SpawnTree(new(globalX, terrainHeight, globalZ));
                        }
                        //RED FLOWER
                        else if (Utils.GetHashOnPosition(globalX, globalZ) < 0.007)
                        {
                            AddBlockOnChunk(BlockType.RedFlower, new(globalX, terrainHeight + 1, globalZ));
                        }
                        //YELLOW FLOWER
                        else if (Utils.GetHashOnPosition(globalX, globalZ) < 0.010)
                        {
                            AddBlockOnChunk(BlockType.YellowFlower, new(globalX, terrainHeight + 1, globalZ));
                        }
                        //SMALL GRASS
                        else if (Utils.GetHashOnPosition(globalX, globalZ) < 0.024)
                        {
                            AddBlockOnChunk(BlockType.SmallGrass, new(globalX, terrainHeight + 1, globalZ));
                        }
                    }
                }
            }
        }

        public void SwitchBlockOnChunk(BlockType blockType, IntPosition position)
        {
            var (chunkX, chunkY, chunkZ) = (position.X.Mod(WIDTH), position.Y, position.Z.Mod(DEPTH));

            GetBlock(chunkX, chunkY, chunkZ).SetBlockType(blockType);
        }

        public void RemoveBlockOnChunk(IntPosition position, bool updateSection = false)
        {
            if (position.X >= _position.X * WIDTH + WIDTH || position.X < _position.X * WIDTH ||
                position.Z >= _position.Z * DEPTH + DEPTH || position.Z < _position.Z * DEPTH)
                return;

            var (chunkX, chunkY, chunkZ) = (position.X.Mod(WIDTH), position.Y, position.Z.Mod(DEPTH));
            if (!IsBlock(chunkX, chunkY, chunkZ, checkObject: false))
                return;

            RemoveBlock(chunkX, chunkY, chunkZ, updateSection);
        }

        public void AddBlockOnChunk(BlockType blockType, IntPosition position, bool updateSection = false)
        {
            if (position.X >= _position.X * WIDTH + WIDTH || position.X < _position.X * WIDTH ||
                position.Z >= _position.Z * DEPTH + DEPTH || position.Z < _position.Z * DEPTH)
                return;

            var (chunkX, chunkY, chunkZ) = (position.X.Mod(WIDTH), position.Y, position.Z.Mod(DEPTH));
            if (IsBlock(chunkX, chunkY, chunkZ))
                return;

            SetBlock(chunkX, chunkY, chunkZ, new Block(blockType), updateSection);
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
                        AddBlockOnChunk(BlockType.Leaf, startPos + new IntPosition(dx, 4 + dy, dz));
                    }

            for (int dx = -1; dx <= 1; dx++)
                for (int dz = -1; dz <= 1; dz++)
                {
                    AddBlockOnChunk(BlockType.Leaf, startPos + new IntPosition(dx, 4 + 3, dz));
                }
        }

        private void RefreshChunkSectionMesh(int sectionIndex)
        {
            var chunkSection = GetSection(sectionIndex);
            if (chunkSection == null)
                return;

            var mesh = Build(this, sectionIndex);

            Globals.GetMap().AwaitingMeshData.Enqueue((chunkSection, mesh));
        }

        public void RefreshMesh()
        {
            for (int sectionIndex = 0; sectionIndex < ChunkSections.Length; sectionIndex++)
            {
                var chunkSection = ChunkSections[sectionIndex];
                if (chunkSection == null)
                    continue;

                RefreshChunkSectionMesh(sectionIndex);
            }
        }

        public bool IsBlock(int x, int y, int z, bool checkTransparent = false, bool checkObject = true)
        {
            if (y < 0 || y >= HEIGHT)
                return false;

            var block = GetBlock(x, y, z);
            var blockProperties = block.GetProperties();

            if (checkObject && blockProperties.Object)
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

        private ChunkSection GetSection(int sectionIndex, bool create = false)
        {
            var section = ChunkSections[sectionIndex];

            if (section == null)
            {
                section = new ChunkSection();
                ChunkSections[sectionIndex] = section;
            }

            return section;
        }

        private static int GetChunkSectionIdFromY(int y) => y / ChunkSection.HEIGHT;

        public Block GetBlock(int x, int y, int z)
        {
            int localY = y % ChunkSection.HEIGHT;

            return GetSection(GetChunkSectionIdFromY(y)).GetBlock(x, localY, z);
        }

        private void UpdateSection(int sectionIndex, int x, int localY, int z)
        {
            var map = Globals.GetMap();
            RefreshChunkSectionMesh(sectionIndex);

            if (localY == 0 && sectionIndex > 0)
                RefreshChunkSectionMesh(sectionIndex - 1);

            if (localY == ChunkSection.HEIGHT - 1 && sectionIndex < HEIGHT / ChunkSection.HEIGHT - 2)
                RefreshChunkSectionMesh(sectionIndex + 1);

            void refresh(ChunkPosition offset)
            {
                var chunk = map.GetChunkOnChunkPos(_position + offset);
                if (chunk == null)
                    return;

                chunk.RefreshChunkSectionMesh(sectionIndex);
            }

            if (x == 0)
                refresh(new(-1, 0));

            if (x == 15)
                refresh(new(1, 0));

            if (z == 0)
                refresh(new(0, -1));

            if (z == 15)
                refresh(new(0, 1));
        }

        private void RemoveBlock(int x, int y, int z, bool updateSection)
        {
            int localY = y % ChunkSection.HEIGHT;
            var sectionIndex = GetChunkSectionIdFromY(y);
            var section = GetSection(sectionIndex);

            section.RemoveBlock(x, localY, z);

            if (updateSection)
            {
                UpdateSection(sectionIndex, x, localY, z);
            }
        }

        private void SetBlock(int x, int y, int z, Block block, bool updateSection)
        {
            int localY = y % ChunkSection.HEIGHT;
            var sectionIndex = GetChunkSectionIdFromY(y);
            var section = GetSection(sectionIndex);

            section.SetBlock(x, localY, z, block);

            if (updateSection)
            {
                UpdateSection(sectionIndex, x, localY, z);
            }
        }
    }
}
