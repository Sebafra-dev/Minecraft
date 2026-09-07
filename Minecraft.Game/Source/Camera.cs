using Microsoft.Xna.Framework;

namespace Minecraft.Source
{
    public class Camera
    {
        private Vector3 _cameraPosition;
        private Matrix _matrix;

        public Camera()
        {

        }

        public void SetCameraPos(Vector3 pos)
        {
            _cameraPosition = pos;
        }

        public void Update(GameTime gameTime)
        {
            var controlling = Globals.GetControlling();

            Vector3 forward = Vector3.Transform(
                Vector3.Forward,
                Matrix.CreateFromYawPitchRoll(controlling.GetYaw(), controlling.GetPitch(), 0)
            );

            _matrix = Matrix.CreateLookAt(
                _cameraPosition,
                _cameraPosition + forward,
                Vector3.Up
            );

            Globals.GetHud().OnPos(_cameraPosition);
        }

        public Vector3 GetCameraPos() => _cameraPosition;

        public Matrix GetView() => _matrix;
    }
}
