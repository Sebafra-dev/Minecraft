namespace Minecraft.Source.UI
{
    public class HUD : UIElement
    {
        public HUD(FrameCounter frameCounter) : base(new(0, 0))
        {
            AddChild(new DebugInfo(frameCounter));
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
