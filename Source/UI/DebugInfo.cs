using Microsoft.Xna.Framework;
using System;

namespace Minecraft.Source.UI
{
    public class DebugInfo : UIElement
    {
        private readonly FrameCounter _frameCounter;
        private int _blocks;
        private int _vertices;

        private readonly Text _textDrawFps;
        private readonly Text _textUpdateFps;
        private readonly Text _textBlocks;

        public DebugInfo(FrameCounter frameCounter) : base(new Vector2(0, 0))
        {
            _frameCounter = frameCounter;
            AddChild(_textDrawFps = new Text(new Vector2(0, 0), ""));
            AddChild(_textUpdateFps = new Text(new Vector2(0, 15), ""));
            AddChild(_textBlocks = new Text(new Vector2(0, 30), ""));
        }

        public void OnBlockData(int blocks, int vertices)
        {
            _blocks = blocks;
            _vertices = vertices;
        }

        public override void Update()
        {
            var drawFps = _frameCounter.GetDrawFps();
            var updateFps = _frameCounter.GetUpdateFps();
                
            _textDrawFps.SetText($"Draw FPS: {drawFps} (avg: {Math.Round(1000f / drawFps, 5)} ms)");
            _textUpdateFps.SetText($"Update FPS: {updateFps} (avg: {Math.Round(1000f / updateFps, 5)} ms)");
            _textBlocks.SetText($"Blocks: {_blocks} Vertices: {_vertices}");

            base.Update();
        }
    }
}
