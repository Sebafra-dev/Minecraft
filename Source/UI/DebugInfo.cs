using Microsoft.Xna.Framework;
using System;

namespace Minecraft.Source.UI
{
    public class DebugInfo : UIElement
    {
        private readonly FrameCounter _frameCounter;
        private int _chunks;
        private int _blocks;
        private int _vertices;

        private Vector3 _pos;

        private readonly Text _textDrawFps;
        private readonly Text _textUpdateFps;
        private readonly Text _textBlocks;
        private readonly Text _textPos;

        public DebugInfo(FrameCounter frameCounter) : base(new Vector2(0, 0))
        {
            _frameCounter = frameCounter;
            AddChild(_textDrawFps = new Text(new Vector2(0, 0), ""));
            AddChild(_textUpdateFps = new Text(new Vector2(0, 15), ""));
            AddChild(_textBlocks = new Text(new Vector2(0, 30), ""));
            AddChild(_textPos = new Text(new Vector2(0, 45), ""));
        }

        public void OnRenderData(int chunks, int blocks, int vertices)
        {
            _chunks = chunks;
            _blocks = blocks;
            _vertices = vertices;
        }

        public void OnPos(Vector3 pos)
        {
            _pos = pos;
        }

        public override void Update()
        {
            var drawFps = _frameCounter.GetDrawFps();
            var updateFps = _frameCounter.GetUpdateFps();
                
            _textDrawFps.SetText($"Draw FPS: {drawFps} (avg: {Math.Round(1000f / drawFps, 5)} ms)");
            _textUpdateFps.SetText($"Update FPS: {updateFps} (avg: {Math.Round(1000f / updateFps, 5)} ms)");
            _textBlocks.SetText($"chunks: {_chunks} blocks: {_blocks} vertices: {_vertices}");
            _textPos.SetText($"x: {_pos.X} y: {_pos.Y} z: {_pos.Z}");

            base.Update();
        }
    }
}
