using Microsoft.Xna.Framework;

namespace Minecraft.Source
{
    public class FrameCounter
    {
        private readonly FpsCounter _drawFps;
        private readonly FpsCounter _updateFps;

        public FrameCounter()
        {
            _drawFps = new FpsCounter();
            _updateFps = new FpsCounter();
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
            _updateFps.OnUpdateCounter(gameTime);
        }

        public void OnDraw(GameTime gameTime)
        {
            _drawFps.OnUpdateCounter(gameTime);
        }

        public int GetUpdateFps() => _updateFps.RetCount;

        public int GetDrawFps() => _drawFps.RetCount;
    }
}
