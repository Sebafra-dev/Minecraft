using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace Minecraft.Source
{
    public class Camera
    {
        private Vector3 _cameraPosition = new(0, 75, 0);
        private float _yaw = 0f;
        private float _pitch = 0f;

        private readonly float _moveSpeed = 5f;
        private readonly float _mouseSensitivity = 0.005f;

        private Matrix _matrix;

        public Camera()
        {
        }

        public void Update(GameTime gameTime)
        {
            if (!GameMain.Instance.IsActive)
                return;

            var graphicsDevice = Globals.GetGraphics().GraphicsDevice;
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            Vector3 forward = Vector3.Transform(
                Vector3.Forward,
                Matrix.CreateFromYawPitchRoll(_yaw, _pitch, 0)
            );

            Vector3 right = Vector3.Cross(forward, Vector3.Up);
            right.Normalize();

            if (keyboard.IsKeyDown(Keys.W))
                _cameraPosition += forward * _moveSpeed * deltaTime;

            if (keyboard.IsKeyDown(Keys.S))
                _cameraPosition -= forward * _moveSpeed * deltaTime;

            if (keyboard.IsKeyDown(Keys.A))
                _cameraPosition -= right * _moveSpeed * deltaTime;

            if (keyboard.IsKeyDown(Keys.D))
                _cameraPosition += right * _moveSpeed * deltaTime;

            if (keyboard.IsKeyDown(Keys.Space))
                _cameraPosition.Y += _moveSpeed * deltaTime;

            if (keyboard.IsKeyDown(Keys.LeftControl))
                _cameraPosition.Y -= _moveSpeed * deltaTime;

            Globals.GetHud().OnPos(_cameraPosition);

            int mouseX = mouse.X - graphicsDevice.Viewport.Width / 2;
            int mouseY = mouse.Y - graphicsDevice.Viewport.Height / 2;

            _yaw -= mouseX * _mouseSensitivity;
            _pitch -= mouseY * _mouseSensitivity;

            Mouse.SetPosition(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);

            _pitch = MathHelper.Clamp(
                _pitch,
                -MathHelper.PiOver2 + 0.01f,
                MathHelper.PiOver2 - 0.01f
            );

            forward = Vector3.Transform(
                Vector3.Forward,
                Matrix.CreateFromYawPitchRoll(_yaw, _pitch, 0)
            );

            _matrix = Matrix.CreateLookAt(
                _cameraPosition,
                _cameraPosition + forward,
                Vector3.Up
            );
        }

        public Vector3 GetCameraPos() => _cameraPosition;

        public Matrix GetView()
        {
            return _matrix;
        }
    }
}
