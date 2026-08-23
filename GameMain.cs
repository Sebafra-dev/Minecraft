using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Minecraft.Source;
using Minecraft.Source.UI;
using System;

namespace Minecraft
{
    public class GameMain : Game
    {
        private FrameCounter _frameCounter;
        private HUD _hud;
        private Map _map;

        private Camera _camera;

        public static GameMain Instance { get; private set; }
        
        public GameMain()
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

            Instance = this;
        }

        protected override void Initialize()
        {
            var graphicsDevice = Globals.GetGraphics().GraphicsDevice;

            _frameCounter = new FrameCounter();
            _hud = new HUD(_frameCounter);
            _map = new Map();

            Globals.SetCamera(_camera = new Camera());
            Mouse.SetPosition(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
            IsMouseVisible = false;

            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            Globals.SetSpriteBatch(new SpriteBatch(Globals.GetGraphics().GraphicsDevice));
            Globals.SetFont(Content.Load<SpriteFont>("Fonts/Arial"));
            Globals.SetTexture(Content.Load<Texture2D>("Images/Blocks"));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _camera.Update(gameTime);
            _hud.Update();

            _frameCounter.OnUpdate(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Globals.GetGraphics().GraphicsDevice.Clear(Color.Black);

            _map.Draw();
            _hud.Draw();

            _frameCounter.OnDraw(gameTime);
            base.Draw(gameTime);
        }
    }
}
