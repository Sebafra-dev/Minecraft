using Microsoft.Xna.Framework;

namespace Minecraft.Source.UI
{
    public class HUD : UIElement
    {
        private readonly DebugInfo _debugInfo;
        public HUD(FrameCounter frameCounter) : base(new(0, 0))
        {
            AddChild(_debugInfo = new DebugInfo(frameCounter));
        }

        public void OnRenderData(int chunks, int blocks, int vertices)
        {
            _debugInfo.OnRenderData(chunks, blocks, vertices);
        }

        public void OnPos(Vector3 pos)
        {
            _debugInfo.OnPos(pos);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Draw()
        {
            var spriteBatch = Globals.GetSpriteBatch();

            spriteBatch.Begin();

            base.Draw();

            spriteBatch.End();
        }
    }
}
