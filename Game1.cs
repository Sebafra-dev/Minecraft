using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Minecraft.Source;

namespace Minecraft
{
    public class Game1 : Game
    {
        private readonly FrameCounter _frameCounter;
        public Game1()
        {
            Globals.SetGraphics(new GraphicsDeviceManager(this)
            {
                IsFullScreen = false,
                PreferredBackBufferWidth = 1920,
                PreferredBackBufferHeight = 1080,
                SynchronizeWithVerticalRetrace = false,
                PreferMultiSampling = false,
                GraphicsProfile = GraphicsProfile.HiDef
            });
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;

            _frameCounter = new FrameCounter();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Globals.SetSpriteBatch(new SpriteBatch(GraphicsDevice));
            Globals.SetFont(Content.Load<SpriteFont>("Fonts/Arial"));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _frameCounter.OnUpdate(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            var spriteBatch = Globals.GetSpriteBatch();
            var font = Globals.GetFont();

            spriteBatch.Begin();
            spriteBatch.DrawString(font, $"Draw FPS: {_frameCounter.GetDrawFps()}", new Vector2(0, 0), Color.White);
            spriteBatch.DrawString(font, $"Update FPS: {_frameCounter.GetUpdateFps()}", new Vector2(0, 20), Color.White);
            spriteBatch.End();

            _frameCounter.OnDraw(gameTime);
            base.Draw(gameTime);
        }
    }
}
