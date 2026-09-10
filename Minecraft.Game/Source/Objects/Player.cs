using Microsoft.Xna.Framework;

namespace Minecraft.Source.Objects
{
    public class Player : Entity
    {
        public override void Update(GameTime gameTime)
        {
            if (Globals.GetPlayer() == this)
            {
                Globals.GetCamera().SetCameraPos(_pos);
            }

            base.Update(gameTime);
        }
    }
}
