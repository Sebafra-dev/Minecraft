using Microsoft.Xna.Framework;

namespace Minecraft.Source
{
    public class FrameCounter
    {
        private readonly FpsCounter drawFps;
        private readonly FpsCounter updateFps;

        public FrameCounter()
        {
            drawFps = new FpsCounter();
            updateFps = new FpsCounter();
        }

        private class FpsCounter
        {
            public int RetCount;
            private int Count;
            private double LastMS;

            public void OnUpdateCounter(GameTime gameTime)
            {
                var totalMS = gameTime.TotalGameTime.TotalMilliseconds;
                if (totalMS >= LastMS + Globals.SEC_TO_MS)
                {
                    RetCount = Count;
                    Count = 0;
                    LastMS = totalMS;
                }

                Count++;
            }
        }

        public void OnUpdate(GameTime gameTime)
        {
            updateFps.OnUpdateCounter(gameTime);
        }

        public void OnDraw(GameTime gameTime)
        {
            drawFps.OnUpdateCounter(gameTime);
        }

        public int GetUpdateFps() => updateFps.RetCount;

        public int GetDrawFps() => drawFps.RetCount;
    }
}
