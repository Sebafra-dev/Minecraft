using Microsoft.Xna.Framework;
using Minecraft.Source.Objects;
using Minecraft.Source.Structures;
using System.Collections.Generic;

namespace Minecraft.Source
{
    public static class GreedyMesher
    {
        public class MeshData
        {
            public List<VoxelVertex> Vertices { get; } = [];
            public List<int> Indices { get; } = [];
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
            public Block.BlockType Type;
            public Block Block;
        }

        public static MeshData Build(Chunk chunk)
        {
            var mesh = new MeshData();

            BuildFace(chunk, mesh, Face.PlusX);
            BuildFace(chunk, mesh, Face.MinusX);

            BuildFace(chunk, mesh, Face.PlusY);
            BuildFace(chunk, mesh, Face.MinusY);

            BuildFace(chunk, mesh, Face.PlusZ);
            BuildFace(chunk, mesh, Face.MinusZ);

            return mesh;
        }

        private static void BuildFace(Chunk chunk, MeshData mesh, Face face)
        {
            int slices;
            int width;
            int height;

            switch (face)
            {
                case Face.PlusX:
                case Face.MinusX:
                    slices = Chunk.WIDTH;

                    width = Chunk.DEPTH;
                    height = Chunk.HEIGHT;
                    break;

                case Face.PlusY:
                case Face.MinusY:
                    slices = Chunk.HEIGHT;

                    width = Chunk.WIDTH;
                    height = Chunk.DEPTH;
                    break;

                default:
                    slices = Chunk.DEPTH;

                    width = Chunk.WIDTH;
                    height = Chunk.HEIGHT;
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

                        Block block = chunk.GetBlock(x, y, z);

                        int index = v * width + u;

                        if (block.IsEmpty || !IsFaceVisible(chunk, face, x, y, z))
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
                                MaskCell test =
                                    mask[(v + rectHeight) * width + u + x];

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

        private static bool IsFaceVisible(Chunk chunk, Face face, int x, int y, int z)
        {
            ChunkPosition chunkPos = chunk.GetPosition();

            int globalX = chunkPos.X * Chunk.WIDTH + x;
            int globalY = y;
            int globalZ = chunkPos.Z * Chunk.DEPTH + z;

            var map = Globals.GetMap();
            var ignoreTransparent = false;

            return face switch
            {
                Face.PlusX => !map.IsBlock(globalX + 1, globalY, globalZ, ignoreTransparent),

                Face.MinusX => !map.IsBlock(globalX - 1, globalY, globalZ, ignoreTransparent),

                Face.PlusY => !map.IsBlock(globalX, globalY + 1, globalZ, ignoreTransparent),

                Face.MinusY => !map.IsBlock(globalX, globalY - 1, globalZ, ignoreTransparent),

                Face.PlusZ => !map.IsBlock(globalX, globalY, globalZ + 1, ignoreTransparent),

                Face.MinusZ => !map.IsBlock(globalX, globalY, globalZ - 1, ignoreTransparent),

                _ => false
            };
        }

        private static void AddQuad(
            Chunk chunk,
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
                        float y = v;
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
                        float y = v;
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
                        float y = slice + 1;
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
                        float y = slice;
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
                        float y = v;
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
                        float y = v;
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

            Vector2 uv;

            if (face == Face.PlusY)
                uv = Utils.GetUV(ids.Item1);
            else
                uv = Utils.GetUV(ids.Item2);

            int vertexOffset = mesh.Vertices.Count;

            mesh.Vertices.Add(
                new VoxelVertex
                {
                    Position = a,
                    AtlasUV = uv,
                    TileUV = new Vector2(width, height)
                }
            );

            mesh.Vertices.Add(
                new VoxelVertex
                {
                    Position = b,
                    AtlasUV = uv,
                    TileUV = new Vector2(0, height)
                }
            );

            mesh.Vertices.Add(
                new VoxelVertex
                {
                    Position = c,
                    AtlasUV = uv,
                    TileUV = new Vector2(0, 0)
                }
            );

            mesh.Vertices.Add(
                new VoxelVertex
                {
                    Position = d,
                    AtlasUV = uv,
                    TileUV = new Vector2(width, 0)
                }
            );

            mesh.Indices.Add(vertexOffset);
            mesh.Indices.Add(vertexOffset + 2);
            mesh.Indices.Add(vertexOffset + 1);

            mesh.Indices.Add(vertexOffset);
            mesh.Indices.Add(vertexOffset + 3);
            mesh.Indices.Add(vertexOffset + 2);
        }
    }
}
