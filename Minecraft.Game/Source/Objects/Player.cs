using Microsoft.Xna.Framework;
using Minecraft.Source.Structures;
using System;

namespace Minecraft.Source.Objects
{
    public class Player : Entity
    {
        public Player(Vector3 sizes) : base(sizes)
        {

        }

        public override void Update(GameTime gameTime)
        {
            if (IsClientPlayer()) //TODO new class?
            {
                var controlling = Globals.GetControlling();
                var map = Globals.GetMap();

                Globals.GetCamera().SetCameraPos(_pos + new Vector3(0, 1, 0));

                Vector3 origin = _pos;
                Vector3 direction = Vector3.Normalize(controlling.Forward);

                const float maxDistance = 5.0f;

                Globals.SetSelectedBlock(null);

                int x = (int)MathF.Floor(origin.X);
                int y = (int)MathF.Floor(origin.Y + 1);
                int z = (int)MathF.Floor(origin.Z);

                int stepX = Math.Sign(direction.X);
                int stepY = Math.Sign(direction.Y);
                int stepZ = Math.Sign(direction.Z);

                float deltaX = direction.X == 0 ? float.PositiveInfinity
                    : MathF.Abs(1.0f / direction.X);

                float deltaY = direction.Y == 0 ? float.PositiveInfinity
                    : MathF.Abs(1.0f / direction.Y);

                float deltaZ = direction.Z == 0 ? float.PositiveInfinity
                    : MathF.Abs(1.0f / direction.Z);

                float maxX = direction.X > 0 ? (x + 1 - origin.X) * deltaX
                    : direction.X < 0 ? (origin.X - x) * deltaX
                    : float.PositiveInfinity;

                float maxY = direction.Y > 0 ? (y + 1 - origin.Y) * deltaY
                    : direction.Y < 0 ? (origin.Y - y) * deltaY
                    : float.PositiveInfinity;

                float maxZ = direction.Z > 0 ? (z + 1 - origin.Z) * deltaZ
                    : direction.Z < 0 ? (origin.Z - z) * deltaZ
                    : float.PositiveInfinity;

                IntPosition normal = new(0, 0, 0);
                float distance = 0;
                Block block;

                while (distance <= maxDistance)
                {
                    block = map.GetBlock(new(x, y, z));
                    if (block.GetBlockType() != BlockProperties.BlockType.Air && !block.GetProperties().Liquid)
                    {
                        Globals.SetSelectedBlock(new IntPosition(x, y, z));

                        Globals.SetSelectedBlockNormal(normal);

                        break;
                    }

                    if (maxX <= maxY && maxX <= maxZ)
                    {
                        distance = maxX;
                        maxX += deltaX;
                        x += stepX;

                        normal = new IntPosition(-stepX, 0, 0);
                    }
                    else if (maxY <= maxZ)
                    {
                        distance = maxY;
                        maxY += deltaY;
                        y += stepY;

                        normal = new IntPosition(0, -stepY, 0);
                    }
                    else
                    {
                        distance = maxZ;
                        maxZ += deltaZ;
                        z += stepZ;

                        normal = new IntPosition(0, 0, -stepZ);
                    }
                }

            }

            base.Update(gameTime);
        }
    }
}
