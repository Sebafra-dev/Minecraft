using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Minecraft.Source.Objects;
using Minecraft.Source.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using static Minecraft.Source.BlockProperties;

namespace Minecraft.Source
{
    public static class GreedyMesher
    {
        public class MeshData
        {
            public VoxelVertex[] Vertices { get; set; } = [];
            public int[] Indices { get; set; } = [];
            public int VerticesCount { get; set; }
            public int IndicesCount { get; set; }
        }

        private enum Face
        {
            PlusX,
            MinusX,
            PlusY,
            MinusY,
            PlusZ,
            MinusZ
        }

        private struct MaskCell
        {
            public bool Active;
            public BlockType Type;
            public Block Block;
        }

        public static MeshData Build(Chunk chunk, int chunkSectionIndex)
        {
            var mesh = new MeshData();

            BuildFace(chunk, chunkSectionIndex, mesh, Face.PlusX);
            BuildFace(chunk, chunkSectionIndex, mesh, Face.MinusX);

            BuildFace(chunk, chunkSectionIndex, mesh, Face.PlusY);
            BuildFace(chunk, chunkSectionIndex, mesh, Face.MinusY);

            BuildFace(chunk, chunkSectionIndex, mesh, Face.PlusZ);
            BuildFace(chunk, chunkSectionIndex, mesh, Face.MinusZ);

            BuildObjects(chunk, chunkSectionIndex, mesh);

            return mesh;
        }

        private static void BuildObjects(Chunk chunk, int chunkSectionIndex, MeshData mesh)
        {
            var chunkSection = chunk.ChunkSections[chunkSectionIndex];

            for (int x = 0; x < ChunkSection.WIDTH; x++)
            {
                for (int y = 0; y < ChunkSection.HEIGHT; y++)
                {
                    for (int z = 0; z < ChunkSection.DEPTH; z++)
                    {
                        Block block = chunkSection.GetBlock(x, y, z);
                        if (!block.GetProperties().Object)
                            continue;

                        AddObject(chunk, chunkSectionIndex, mesh, block, x, y, z);
                    }
                }
            }
        }

        private static void BuildFace(Chunk chunk, int chunkSectionIndex, MeshData mesh, Face face)
        {
            int slices;
            int width;
            int height;

            var chunkSection = chunk.ChunkSections[chunkSectionIndex];

            switch (face)
            {
                case Face.PlusX:
                case Face.MinusX:
                    slices = ChunkSection.WIDTH;

                    width = ChunkSection.DEPTH;
                    height = ChunkSection.HEIGHT;
                    break;

                case Face.PlusY:
                case Face.MinusY:
                    slices = ChunkSection.HEIGHT;

                    width = ChunkSection.WIDTH;
                    height = ChunkSection.DEPTH;
                    break;

                default:
                    slices = ChunkSection.DEPTH;

                    width = ChunkSection.WIDTH;
                    height = ChunkSection.HEIGHT;
                    break;
            }

            var mask = new MaskCell[width * height];

            for (int slice = 0; slice < slices; slice++)
            {
                for (int v = 0; v < height; v++)
                {
                    for (int u = 0; u < width; u++)
                    {
                        GetCoordinates(face, slice, u, v, out int x, out int y, out int z);

                        Block block = chunkSection.GetBlock(x, y, z);
                        var blockProperties = block.GetProperties();

                        int index = v * width + u;

                        if (!blockProperties.Visible || blockProperties.Object || 
                            !IsFaceVisible(chunk, chunkSectionIndex, face, x, y, z))
                        {
                            mask[index] = default;
                            continue;
                        }

                        mask[index] = new MaskCell
                        {
                            Active = true,
                            Type = block.GetBlockType(),
                            Block = block
                        };
                    }
                }

                for (int v = 0; v < height; v++)
                {
                    for (int u = 0; u < width;)
                    {
                        int index = v * width + u;
                        MaskCell cell = mask[index];

                        if (!cell.Active)
                        {
                            u++;
                            continue;
                        }

                        int rectWidth = 1;

                        while (u + rectWidth < width && Same(mask[v * width + u + rectWidth], cell))
                        {
                            rectWidth++;
                        }

                        int rectHeight = 1;

                        while (v + rectHeight < height)
                        {
                            bool canGrow = true;

                            for (int x = 0; x < rectWidth; x++)
                            {
                                MaskCell test = mask[(v + rectHeight) * width + u + x];

                                if (!Same(test, cell))
                                {
                                    canGrow = false;
                                    break;
                                }
                            }

                            if (!canGrow)
                                break;

                            rectHeight++;
                        }

                        AddQuad(
                            chunk,
                            chunkSectionIndex,
                            mesh,
                            face,
                            slice,
                            u,
                            v,
                            rectWidth,
                            rectHeight,
                            cell.Block);

                        for (int yy = 0; yy < rectHeight; yy++)
                        {
                            for (int xx = 0; xx < rectWidth; xx++)
                            {
                                int usedIndex =
                                    (v + yy) * width + (u + xx);

                                mask[usedIndex].Active = false;
                            }
                        }

                        u += rectWidth;
                    }
                }
            }
        }

        private static bool Same(MaskCell a, MaskCell b)
        {
            return
                a.Active &&
                b.Active &&
                a.Type == b.Type;
        }

        private static void GetCoordinates(
            Face face,
            int slice,
            int u,
            int v,
            out int x,
            out int y,
            out int z)
        {
            switch (face)
            {
                case Face.PlusX:
                case Face.MinusX:
                    x = slice;
                    y = v;
                    z = u;
                    break;

                case Face.PlusY:
                case Face.MinusY:
                    x = u;
                    y = slice;
                    z = v;
                    break;

                default:
                    x = u;
                    y = v;
                    z = slice;
                    break;
            }
        }

        private static bool IsFaceVisible(Chunk chunk, int chunkSectionIndex, Face face, int x, int y, int z)
        {
            ChunkPosition chunkPos = chunk.GetPosition();

            int globalX = chunkPos.X * Chunk.WIDTH + x;
            int globalY = chunkSectionIndex * ChunkSection.HEIGHT + y;
            int globalZ = chunkPos.Z * Chunk.DEPTH + z;

            var map = Globals.GetMap();

            Block block = map.GetBlock(globalX, globalY, globalZ);

            if (block.GetProperties().Liquid)
            {
                return face switch
                {
                    Face.PlusY => !map.IsBlock(globalX, globalY + 1, globalZ, false),
                    _ => false
                };
            }

            var checkTransparent = true;

            return face switch
            {
                Face.PlusX => !map.IsBlock(globalX + 1, globalY, globalZ, checkTransparent),

                Face.MinusX => !map.IsBlock(globalX - 1, globalY, globalZ, checkTransparent),

                Face.PlusY => !map.IsBlock(globalX, globalY + 1, globalZ, checkTransparent),

                Face.MinusY => !map.IsBlock(globalX, globalY - 1, globalZ, checkTransparent),

                Face.PlusZ => !map.IsBlock(globalX, globalY, globalZ + 1, checkTransparent),

                Face.MinusZ => !map.IsBlock(globalX, globalY, globalZ - 1, checkTransparent),

                _ => false
            };
        }

        private static void AddObject(Chunk chunk, int chunkSectionIndex, MeshData mesh, Block block,  int x, int y, int z)
        {
            var ids = block.GetAtlasIds();
            var atlasId = ids.Item2;
            var alpha = block.GetProperties().Alpha;

            ChunkPosition chunkPos = chunk.GetPosition();

            int globalX = chunkPos.X * Chunk.WIDTH + x;
            int globalY = chunkSectionIndex * ChunkSection.HEIGHT + y;
            int globalZ = chunkPos.Z * Chunk.DEPTH + z;

            int vertexOffset = mesh.VerticesCount; // 1

            mesh.AddVertice(new VoxelVertex
            {
                Position = new Vector3(
                    globalX,
                    globalY,
                    globalZ + 1),
                TileUV = new HalfVector2(1, 1),
                Data = new((byte)atlasId, (byte)0, (byte)0, alpha)
            });

            mesh.AddVertice(new VoxelVertex
            {
                Position = new Vector3(
                    globalX + 1,
                    globalY,
                    globalZ),
                TileUV = new HalfVector2(0, 1),
                Data = new((byte)atlasId, (byte)0, (byte)0, alpha)
            });

            mesh.AddVertice(new VoxelVertex
            {
                Position = new Vector3(
                    globalX,
                    globalY + 1,
                    globalZ + 1),
                TileUV = new HalfVector2(1, 0),
                Data = new((byte)atlasId, (byte)0, (byte)0, alpha)
            });

            mesh.AddVertice(new VoxelVertex
            {
                Position = new Vector3(
                    globalX + 1,
                    globalY + 1,
                    globalZ),
                TileUV = new HalfVector2(0, 0),
                Data = new((byte)atlasId, (byte)0, (byte)0, alpha)
            });

            mesh.AddIndice(vertexOffset);
            mesh.AddIndice(vertexOffset + 1);
            mesh.AddIndice(vertexOffset + 2);

            mesh.AddIndice(vertexOffset + 1);
            mesh.AddIndice(vertexOffset + 3);
            mesh.AddIndice(vertexOffset + 2);

            mesh.AddIndice(vertexOffset + 2);
            mesh.AddIndice(vertexOffset + 1);
            mesh.AddIndice(vertexOffset);

            mesh.AddIndice(vertexOffset + 2);
            mesh.AddIndice(vertexOffset + 3);
            mesh.AddIndice(vertexOffset + 1);

            vertexOffset = mesh.VerticesCount; // 2

            mesh.AddVertice(new VoxelVertex
            {
                Position = new Vector3(
                    globalX,
                    globalY,
                    globalZ),
                TileUV = new HalfVector2(1, 1),
                Data = new((byte)atlasId, (byte)0, (byte)0, alpha)
            });

            mesh.AddVertice(new VoxelVertex
            {
                Position = new Vector3(
                    globalX + 1,
                    globalY,
                    globalZ + 1),
                TileUV = new HalfVector2(0, 1),
                Data = new((byte)atlasId, (byte)0, (byte)0, alpha)
            });

            mesh.AddVertice(new VoxelVertex
            {
                Position = new Vector3(
                    globalX,
                    globalY + 1,
                    globalZ),
                TileUV = new HalfVector2(1, 0),
                Data = new((byte)atlasId, (byte)0, (byte)0, alpha)
            });

            mesh.AddVertice(new VoxelVertex
            {
                Position = new Vector3(
                    globalX + 1,
                    globalY + 1,
                    globalZ + 1),
                TileUV = new HalfVector2(0, 0),
                Data = new((byte)atlasId, (byte)0, (byte)0, alpha)
            });

            mesh.AddIndice(vertexOffset);
            mesh.AddIndice(vertexOffset + 2);
            mesh.AddIndice(vertexOffset + 1);

            mesh.AddIndice(vertexOffset + 1);
            mesh.AddIndice(vertexOffset + 2);
            mesh.AddIndice(vertexOffset + 3);

            mesh.AddIndice(vertexOffset + 1);
            mesh.AddIndice(vertexOffset + 2);
            mesh.AddIndice(vertexOffset);

            mesh.AddIndice(vertexOffset + 3);
            mesh.AddIndice(vertexOffset + 2);
            mesh.AddIndice(vertexOffset + 1);
        }

        private static void AddQuad(Chunk chunk,
            int chunkSectionIndex,
            MeshData mesh,
            Face face,
            int slice,
            int u,
            int v,
            int width,
            int height,
            Block block)
        {
            var chunkPos = chunk.GetPosition();

            float chunkX = chunkPos.X * Chunk.WIDTH;
            float chunkY = chunkSectionIndex * ChunkSection.HEIGHT;
            float chunkZ = chunkPos.Z * Chunk.DEPTH;

            Vector3 a;
            Vector3 b;
            Vector3 c;
            Vector3 d;

            switch (face)
            {
                case Face.PlusX:
                    {
                        float x = chunkX + slice + 1;
                        float y = chunkY + v;
                        float z = chunkZ + u;

                        a = new Vector3(x, y, z + width);
                        b = new Vector3(x, y, z);
                        c = new Vector3(x, y + height, z);
                        d = new Vector3(x, y + height, z + width);
                        break;
                    }

                case Face.MinusX:
                    {
                        float x = chunkX + slice;
                        float y = chunkY + v;
                        float z = chunkZ + u;

                        a = new Vector3(x, y, z);
                        b = new Vector3(x, y, z + width);
                        c = new Vector3(x, y + height, z + width);
                        d = new Vector3(x, y + height, z);
                        break;
                    }

                case Face.PlusY:
                    {
                        float x = chunkX + u;
                        float y = chunkY + slice + 1;
                        float z = chunkZ + v;

                        a = new Vector3(x, y, z + height);
                        b = new Vector3(x + width, y, z + height);
                        c = new Vector3(x + width, y, z);
                        d = new Vector3(x, y, z);
                        break;
                    }

                case Face.MinusY:
                    {
                        float x = chunkX + u;
                        float y = chunkY + slice;
                        float z = chunkZ + v;

                        a = new Vector3(x, y, z);
                        b = new Vector3(x + width, y, z);
                        c = new Vector3(x + width, y, z + height);
                        d = new Vector3(x, y, z + height);
                        break;
                    }

                case Face.PlusZ:
                    {
                        float x = chunkX + u;
                        float y = chunkY + v;
                        float z = chunkZ + slice + 1;

                        a = new Vector3(x, y, z);
                        b = new Vector3(x + width, y, z);
                        c = new Vector3(x + width, y + height, z);
                        d = new Vector3(x, y + height, z);
                        break;
                    }

                case Face.MinusZ:
                    {
                        float x = chunkX + u;
                        float y = chunkY + v;
                        float z = chunkZ + slice;

                        a = new Vector3(x + width, y, z);
                        b = new Vector3(x, y, z);
                        c = new Vector3(x, y + height, z);
                        d = new Vector3(x + width, y + height, z);
                        break;
                    }

                default:
                    return;
            }

            var ids = block.GetAtlasIds();

            var atlasId = face == Face.PlusY ? ids.Item1 : ids.Item2;
            var alpha = block.GetProperties().Alpha;

            int vertexOffset = mesh.VerticesCount;

            mesh.AddVertice(new VoxelVertex
                {
                    Position = a,
                    TileUV = new HalfVector2(width, height),
                    Data = new((byte)atlasId, (byte)0, (byte)0, alpha)
                }
            );

            mesh.AddVertice(new VoxelVertex
                {
                    Position = b,
                    TileUV = new HalfVector2(0, height),
                    Data = new((byte)atlasId, (byte)0, (byte)0, alpha)
                }
            );

            mesh.AddVertice(new VoxelVertex
                {
                    Position = c,
                    TileUV = new HalfVector2(0, 0),
                    Data = new((byte)atlasId, (byte)0, (byte)0, alpha)
                }
            );

            mesh.AddVertice(new VoxelVertex
                {
                    Position = d,
                    TileUV = new HalfVector2(width, 0),
                    Data = new((byte)atlasId, (byte)0, (byte)0, alpha)
                }
            );

            mesh.AddIndice(vertexOffset);
            mesh.AddIndice(vertexOffset + 2);
            mesh.AddIndice(vertexOffset + 1);

            mesh.AddIndice(vertexOffset);
            mesh.AddIndice(vertexOffset + 3);
            mesh.AddIndice(vertexOffset + 2);
        }

        private static void AddVertice(this MeshData mesh, VoxelVertex voxelVertex)
        {
            if (mesh.VerticesCount >= mesh.Vertices.Length)
            {
                var oldArr = mesh.Vertices;
                int newSize = oldArr.Length == 0 ? 4 : oldArr.Length * 2;

                mesh.Vertices = new VoxelVertex[newSize];
                Array.Copy(oldArr, mesh.Vertices, mesh.VerticesCount);
            }

            mesh.Vertices[mesh.VerticesCount++] = voxelVertex;
        }

        private static void AddIndice(this MeshData mesh, int indice)
        {
            if (mesh.IndicesCount >= mesh.Indices.Length)
            {
                var oldArr = mesh.Indices;
                int newSize = oldArr.Length == 0 ? 4 : oldArr.Length * 2;

                mesh.Indices = new int[newSize];
                Array.Copy(oldArr, mesh.Indices, mesh.IndicesCount);
            }

            mesh.Indices[mesh.IndicesCount++] = indice;
        }
    }
}
