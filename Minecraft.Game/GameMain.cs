using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Minecraft.Source;
using Minecraft.Source.UI;

namespace Minecraft
{
    public class GameMain : Game
    {
        private FrameCounter _frameCounter;
        private HUD _hud;
        private Map _map;

        private Controlling _controlling;
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
            IsMouseVisible = false;
            IsFixedTimeStep = false;

            Window.Position = new(100, 100);

            Instance = this;
        }

        protected override void Initialize()
        {
            var graphics = Globals.GetGraphics();
            var graphicsDevice = graphics.GraphicsDevice;

            Globals.SetHud(_hud = new HUD(_frameCounter = new FrameCounter()));
            Globals.SetMap(_map = new Map());
            Globals.SetControlling(_controlling = new Controlling());
            Globals.SetCamera(_camera = new Camera());

            Mouse.SetPosition(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);

            graphicsDevice.RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.CullCounterClockwiseFace,
                MultiSampleAntiAlias = true,
                FillMode = FillMode.Solid
            };

            base.Initialize();
        }

        protected override void LoadContent()
        {
            var graphicsDevice = Globals.GetGraphics().GraphicsDevice;
            Globals.SetSpriteBatch(new SpriteBatch(graphicsDevice));
            Globals.SetFont(Content.Load<SpriteFont>("Fonts/Arial"));
            Globals.SetTexture(Content.Load<Texture2D>("Images/Blocks"));
            Globals.SetEffect(Content.Load<Effect>("Shaders/Voxel"));

            Globals.PROJECTION = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), graphicsDevice.Viewport.AspectRatio, 0.1f, 1000f);

            var eff = Globals.GetEffect();
            eff.Parameters["Texture"].SetValue(Globals.GetTexture());
            eff.Parameters["World"].SetValue(Matrix.Identity);
            eff.Parameters["Projection"].SetValue(Globals.PROJECTION);
        }

        protected override void Update(GameTime gameTime)
        {
            _controlling.Update(gameTime);
            _camera.Update(gameTime);
            _map.Update(gameTime);
            _hud.Update();

            _frameCounter.OnUpdate(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            var graphics = Globals.GetGraphics();
            var graphicsDevice = graphics.GraphicsDevice;

            graphicsDevice.Clear(Color.CornflowerBlue);
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;

            _map.Draw();
            _hud.Draw();

            _frameCounter.OnDraw(gameTime);
            base.Draw(gameTime);
        }
    }
}
