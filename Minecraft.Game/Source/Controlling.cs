using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Minecraft.Source.Structures;
using System.Diagnostics;
using static Minecraft.Source.BlockProperties;

namespace Minecraft.Source
{
    public class Controlling
    {
        private MouseState _previousMouseState;
        private MouseState _mouseState;
        private KeyboardState _previousKeyboardState;
        private KeyboardState _keyboardState;

        private float _yaw = 0f;
        private float _pitch = 0f;
        private readonly float _moveSpeed = 8f;
        private readonly float _moveSpeedNoClip = 30f;
        private readonly float _mouseSensitivity = 0.004f;
        public static bool NoClip = false;

        public Vector3 Forward { get; private set; }

        public Controlling()
        {

        }

        public MouseState GetMouseState() => _mouseState;
        public KeyboardState GetKeyboardState() => _keyboardState;

        public float GetYaw() => _yaw;
        public float GetPitch() => _pitch;

        public bool IsMouseRightClick() => _previousMouseState.RightButton == ButtonState.Released && _mouseState.RightButton == ButtonState.Pressed;
        public bool IsMouseLeftClick() => _previousMouseState.LeftButton == ButtonState.Released && _mouseState.LeftButton == ButtonState.Pressed;

        private void PlayerMoveControlling(GameTime gameTime)
        {
            var player = Globals.GetPlayer();
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector3 right = Vector3.Cross(Forward, Vector3.Up);
            right.Normalize();

            var ctrl = _keyboardState.IsKeyDown(Keys.LeftControl);
            var shift = _keyboardState.IsKeyDown(Keys.LeftShift);
            var speedPerc = ctrl ? 0.75f : (shift ? 1.5f : 1f);

            if (NoClip)
            {
                var f = Forward * _moveSpeedNoClip * deltaTime * speedPerc;
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
        }

        private void OnMouseLeftClick()
        {
            var selectedBlock = Globals.GetSelectedBlock();
            if (selectedBlock == null)
                return;

            var map = Globals.GetMap();

            var block = map.GetBlock(selectedBlock.Value);
            if (block.GetProperties().Liquid)
                return;

            map.RemoveBlockOnMap(selectedBlock.Value);
        }


        private void OnMouseRightClick()
        {
            var selectedBlock = Globals.GetSelectedBlock();

            if (selectedBlock == null)
                return;

            var map = Globals.GetMap();

            var block = selectedBlock.Value;
            var normal = Globals.GetSelectedBlockNormal();

            var target = new IntPosition(
                block.X + normal.X,
                block.Y + normal.Y,
                block.Z + normal.Z
            );

            if (map.IsBlock(target))
                return;

            map.AddBlockOnMap(BlockType.Dirt, target);
        }


        public void Update(GameTime gameTime)
        {
            if (!GameMain.Instance.IsActive)
                return;

            _previousMouseState = _mouseState;
            _mouseState = Mouse.GetState();

            _previousKeyboardState = _keyboardState;
            _keyboardState = Keyboard.GetState();

            var graphicsDevice = Globals.GetGraphics().GraphicsDevice;

            Forward = Vector3.Transform(
                Vector3.Forward,
                Matrix.CreateFromYawPitchRoll(_yaw, _pitch, 0)
            );

            if (_keyboardState.IsKeyDown(Keys.Escape))
                GameMain.Instance.Exit();

            PlayerMoveControlling(gameTime);

            if (IsMouseLeftClick())
                OnMouseLeftClick();

            if (IsMouseRightClick())
                OnMouseRightClick();

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
