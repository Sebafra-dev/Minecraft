using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Minecraft.Source.Objects
{
    public class Entity
    {
        protected Vector3 _pos;

        private float _velocityY = 0f;

        private Vector3 _sizes;

        public Entity(Vector3 sizes)
        {
            _sizes = sizes;
        }

        public Vector3 GetPosition() => _pos;

        public void SetPosition(Vector3 pos)
        {
            _pos = pos;
        }

        public void SetPosition(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
        }

        public void Move(Vector3 dPos) //TODO collision
        {
            _pos.X += dPos.X;
            _pos.Y += dPos.Y;
            _pos.Z += dPos.Z;
        }

        public void Move(float dx, float dy, float dz)
        {
            Move(new(dx, dy, dz));
        }

        protected bool IsClientPlayer() => this == Globals.GetPlayer();

        public virtual void Update(GameTime gameTime)
        {
            var map = Globals.GetMap();
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var block = map.GetBlock((int)_pos.X, (int)(_pos.Y - _sizes.Y), (int)_pos.Z);
            var cameraBlock = map.GetBlock((int)_pos.X, (int)_pos.Y, (int)_pos.Z);

            bool grounded = !block.GetProperties().NoCollision;
            bool inLiquid = cameraBlock.GetProperties().Liquid;

            //jump
            if (IsClientPlayer() && (grounded || inLiquid) && 
                Globals.GetControlling().GetKeyboardState().IsKeyDown(Keys.Space))
            {
                const float jumpVelocity = 8f;
                const float liquidJumpVelocity = 5f;
                _velocityY = inLiquid ? liquidJumpVelocity : jumpVelocity;
            }

            //gravity
            if (!grounded)
            {
                const float gravity = 20f;
                const float liquidGravity = 1f;
                const float maxVelocity = 50f;
                const float maxLiquidVelocity = 3f;

                _velocityY -= (inLiquid ? liquidGravity : gravity) * deltaTime;
                _velocityY = MathF.Max(_velocityY, -(inLiquid ? maxLiquidVelocity : maxVelocity));
            }
            else if (_velocityY < 0)
            {
                _velocityY = 0f;
            }

            Move(0, _velocityY * deltaTime, 0);
        }
    }
}
