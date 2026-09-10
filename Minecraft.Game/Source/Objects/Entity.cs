using Microsoft.Xna.Framework;

namespace Minecraft.Source.Objects
{
    public class Entity
    {
        protected Vector3 _pos;

        public Entity()
        {

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

        public void Move(Vector3 dPos)
        {
            _pos.X += dPos.X;
            _pos.Y += dPos.Y;
            _pos.Z += dPos.Z;
        }

        public void Move(float dx, float dy, float dz)
        {
            Move(new(dx, dy, dz));
        }

        public virtual void Update(GameTime gameTime)
        {
            var map = Globals.GetMap();
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!map.IsBlock((int)_pos.X, (int)_pos.Y - 2, (int)_pos.Z))
            {
                Move(0, -10f * deltaTime, 0);
            }
        }
    }
}
