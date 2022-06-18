using SFML.Graphics;
using SFML.System;

namespace Game
{
    public class Bullet
    {
        public Vector2f bulletSpeed;
        public CircleShape bulletShape;
        public CircleShape ownerShape;

        public void MoveBullet(Bullet bullet)
            => bullet.bulletShape.Position += bullet.bulletSpeed;
    }
}