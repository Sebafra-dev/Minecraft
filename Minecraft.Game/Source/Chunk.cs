using Minecraft.Source.Objects;
using Minecraft.Source.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using static Minecraft.Source.Objects.Block;

namespace Minecraft.Source
{
    public class Chunk
    {
        public const int WIDTH = 16;
        public const int HEIGHT = 256;
        public const int DEPTH = 16;

        private Block[,,] _blocks;
        private HashSet<Block> _visibleBlocks;
        private readonly ChunkPosition _position;

        private bool _generated = false;

        public Chunk(ChunkPosition position)
        {
            _position = position;

            _blocks = new Block[WIDTH, HEIGHT, DEPTH];
            _visibleBlocks = [];

            GenerateChunk();
        }

        ~Chunk()
        {
            _blocks = null;
            _visibleBlocks.Clear();
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

        public void RefreshVisibleBlocks()
        {
            _visibleBlocks.Clear();

            foreach (var block in _blocks)
            {
                if (block == null || !block.IsVisible(_position))
                    continue;

                _visibleBlocks.Add(block);
            }
        }

        public enum BoundaryType
        {
            PlusX,
            MinusX,
            PlusZ,
            MinusZ
        }

        public void RefreshBoundaries(BoundaryType boundaryType)
        {
            var visibleBlocks = _visibleBlocks.ToHashSet();

            int boundary;
            bool isXBoundary;

            switch (boundaryType)
            {
                case BoundaryType.PlusX:
                    boundary = 0;
                    isXBoundary = true;
                    break;

                case BoundaryType.MinusX:
                    boundary = WIDTH - 1;
                    isXBoundary = true;
                    break;

                case BoundaryType.PlusZ:
                    boundary = 0;
                    isXBoundary = false;
                    break;

                case BoundaryType.MinusZ:
                    boundary = DEPTH - 1;
                    isXBoundary = false;
                    break;

                default:
                    return;
            }

            if (isXBoundary)
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    for (int z = 0; z < DEPTH; z++)
                    {
                        var block = _blocks[boundary, y, z];

                        if (block != null)
                        {
                            if (block.IsVisible(_position))
                            {
                                visibleBlocks.Add(block);
                            }
                            else
                            {
                                visibleBlocks.Remove(block);
                            }
                        }
                    }
                }
            }
            else
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    for (int x = 0; x < WIDTH; x++)
                    {
                        var block = _blocks[x, y, boundary];

                        if (block != null)
                        {
                            if (block.IsVisible(_position))
                            {
                                visibleBlocks.Add(block);
                            }
                            else
                            {
                                visibleBlocks.Remove(block);
                            }
                        }
                    }
                }
            }

            //if (_visibleBlocks.Count != visibleBlocks.Count)
            //Debug.WriteLine($"{_visibleBlocks.Count} -> {visibleBlocks.Count}");

            _visibleBlocks = visibleBlocks;
        }

        public HashSet<Block> GetVisibleBlocks() => _visibleBlocks; 

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
    }
}
