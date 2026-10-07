using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Minecraft.Source
{
    public class Controlling
    {
        private MouseState _mouseState;
        private KeyboardState _keyboardState;

        private float _yaw = 0f;
        private float _pitch = 0f;
        private readonly float _moveSpeed = 8f;
        private readonly float _moveSpeedNoClip = 30f;
        private readonly float _mouseSensitivity = 0.004f;
        public static bool NoClip = false;

        public Controlling()
        {

        }

        public MouseState GetMouseState() => _mouseState;
        public KeyboardState GetKeyboardState() => _keyboardState;

        public float GetYaw() => _yaw;
        public float GetPitch() => _pitch;

        public void Update(GameTime gameTime)
        {
            if (!GameMain.Instance.IsActive)
                return;

            _mouseState = Mouse.GetState();
            _keyboardState = Keyboard.GetState();

            var graphicsDevice = Globals.GetGraphics().GraphicsDevice;
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector3 forward = Vector3.Transform(
                Vector3.Forward,
                Matrix.CreateFromYawPitchRoll(_yaw, _pitch, 0)
            );

            Vector3 right = Vector3.Cross(forward, Vector3.Up);
            right.Normalize();

            var player = Globals.GetPlayer();

            var ctrl = _keyboardState.IsKeyDown(Keys.LeftControl);
            var shift = _keyboardState.IsKeyDown(Keys.LeftShift);
            var speedPerc = ctrl ? 0.75f : (shift ? 1.5f : 1f);

            if (NoClip)
            {
                var f = forward * _moveSpeedNoClip * deltaTime * speedPerc;
                var r = right * _moveSpeedNoClip * deltaTime * speedPerc;

                if (_keyboardState.IsKeyDown(Keys.W))
                    player.Move(f);

                if (_keyboardState.IsKeyDown(Keys.S))
                    player.Move(-f);

                if (_keyboardState.IsKeyDown(Keys.A))
                    player.Move(-r);

                if (_keyboardState.IsKeyDown(Keys.D))
                    player.Move(r);

                if (_keyboardState.IsKeyDown(Keys.Space))
                    player.Move(new(0, _moveSpeed * deltaTime, 0));

                if (ctrl)
                    player.Move(new(0, -_moveSpeed * deltaTime, 0));
            }
            else
            {
                Vector3 f = Vector3.Transform(
                    Vector3.Forward,
                    Matrix.CreateRotationY(_yaw)
                );

                Vector3 r = Vector3.Transform(
                    Vector3.Right,
                    Matrix.CreateRotationY(_yaw)
                );

                f.Normalize();
                r.Normalize();

                f *= _moveSpeed * deltaTime * speedPerc;
                r *= _moveSpeed * deltaTime * speedPerc;

                if (_keyboardState.IsKeyDown(Keys.W))
                    player.Move(f);

                if (_keyboardState.IsKeyDown(Keys.S))
                    player.Move(-f);

                if (_keyboardState.IsKeyDown(Keys.A))
                    player.Move(-r);

                if (_keyboardState.IsKeyDown(Keys.D))
                    player.Move(r);
            }

            if (_keyboardState.IsKeyDown(Keys.Escape))
                GameMain.Instance.Exit();

            int centerX = graphicsDevice.Viewport.Width / 2;
            int centerY = graphicsDevice.Viewport.Height / 2;

            float deltaX = _mouseState.X - centerX;
            float deltaY = _mouseState.Y - centerY;

            if (deltaX != 0 || deltaY != 0)
            {
                _yaw -= deltaX * _mouseSensitivity;
                _pitch -= deltaY * _mouseSensitivity;

                _pitch = MathHelper.Clamp(
                    _pitch,
                    -MathHelper.PiOver2 + 0.01f,
                    MathHelper.PiOver2 - 0.01f
                );

                Mouse.SetPosition(centerX, centerY);
            }
        }
    }
}
