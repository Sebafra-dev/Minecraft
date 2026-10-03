using Microsoft.Xna.Framework.Graphics;
using Minecraft.Source.Objects;
using Minecraft.Source.Structures;
using System.Diagnostics;
using static Minecraft.Source.GreedyMesher;

namespace Minecraft.Source
{
    public class ChunkSection
    {
        public const int WIDTH = 16;
        public const int DEPTH = 16;
        public const int HEIGHT = 16;

        private Block[] _blocks;

        public VertexBuffer VertexBuffer { get; private set; }
        public IndexBuffer IndexBuffer { get; private set; }
        public int IndexCount { get; private set; }

        public ChunkSection()
        {
            ResetBlocks();
        }

        ~ChunkSection()
        {
            _blocks = null;
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }

        public void ResetBlocks()
        {
            _blocks = new Block[WIDTH * HEIGHT * DEPTH];
        }

        private int vertexCount;
        private int indexCount;

        public void UpdateMeshData(MeshData mesh)
        {
            var graphicsDevice = Globals.GetGraphics().GraphicsDevice;

            IndexCount = mesh.Indices.Count;

            if (IndexCount == 0)
                return;

            if (VertexBuffer == null || vertexCount < mesh.Vertices.Count)
            {
                vertexCount = mesh.Vertices.Count;

                VertexBuffer?.Dispose();

                VertexBuffer = new VertexBuffer(graphicsDevice, VoxelVertex.VertexDeclaration, vertexCount, BufferUsage.WriteOnly);
            }

            if (IndexBuffer == null || indexCount < mesh.Indices.Count)
            {
                indexCount = mesh.Indices.Count;
                IndexBuffer?.Dispose();

                IndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indexCount, BufferUsage.WriteOnly);
            }

            VertexBuffer.SetData(mesh.Vertices.ToArray());
            IndexBuffer.SetData(mesh.Indices.ToArray());
        }

        private static int GetBlockIndex(int x, int y, int z) => x + (z << 4) + (y << 8);

        public Block GetBlock(int x, int y, int z)
        {
            return _blocks[GetBlockIndex(x, y, z)];
        }

        public void SetBlock(int x, int y, int z, Block block)
        {
            _blocks[GetBlockIndex(x, y, z)] = block;
        }
    }
}
