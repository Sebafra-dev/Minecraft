using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Minecraft.Source.Objects;
using System.Collections.Generic;
using System.Linq;
using static Minecraft.Source.Objects.Block;

namespace Minecraft.Source
{
    public sealed class Map
    {
        private readonly BasicEffect _effect;

        private const int WIDTH = 32;
        private const int HEIGHT = 32;
        private const int DEPTH = 32;


        private bool[,,] _blockMap;
        private readonly List<Block> _blocks;
        private readonly List<VertexPositionTexture> _vertices;
        private readonly List<int> _indices;

        private bool _initialized = false;

        public Map()
        {
            var graphicsDevice = Globals.GetGraphics().GraphicsDevice;

            _blockMap = new bool[WIDTH, HEIGHT, DEPTH];

            _effect = new BasicEffect(graphicsDevice)
            {
                TextureEnabled = true,
                World = Matrix.Identity,
                Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), graphicsDevice.Viewport.AspectRatio, 0.1f, 100f)
            };

            _blocks = [];
            for (int x = -1; x < 2; x++)
                for (int z = -1; z < 2; z++)
                {
                    AddBlockOnMap(BlockType.Grass, x, 0, z);
                }

            _vertices = [];
            _indices = [];
        }

        ~Map()
        {
            _blocks.Clear();
            _blockMap = null;
            _vertices.Clear();
            _indices.Clear();
        }

        public void AddBlockOnMap(BlockType blockType, int x, int y, int z)
        {
            if (_blockMap[x + WIDTH / 2, y + HEIGHT / 2, z + DEPTH / 2])
                return;

            _blocks.Add(new Block(blockType, x, y, z));
            _blockMap[x + WIDTH / 2, y + HEIGHT / 2, z + DEPTH / 2] = true;
        }

        public bool IsBlock(int x, int y, int z) => _blockMap[x + WIDTH / 2, y + HEIGHT / 2, z + DEPTH / 2];

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

            var sortedBlocks = _blocks.OrderByDescending(_ => _.GetDist(camera.GetCameraPos()));

            _vertices.Clear();
            _indices.Clear();
            var ids = 0;

            foreach (var block in sortedBlocks)
            {
                var vertices = block.GetVertices();
                _vertices.AddRange(vertices);

                for (int i = 0; i < vertices.Length / 4; i++)
                {
                    int offset = ids + i * 4;

                    _indices.AddRange([
                        offset, offset + 2, offset + 1,
                        offset, offset + 3, offset + 2
                    ]);
                }

                ids += vertices.Length;
            }

            Globals.GetHud().OnBlockData(sortedBlocks.Count(), _vertices.Count);

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Globals.GetGraphics().GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _vertices.ToArray(),
                    0,
                    _vertices.Count,
                    _indices.ToArray(),
                    0,
                    _indices.Count / 3
                );
            }
        }
    }
}
