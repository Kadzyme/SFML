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

        public bool BulletInScreen(Vector2u windowSize)
        {
            var radius = bulletShape.Radius;
            var bulletPos = bulletShape.Position;
            return bulletPos.X + radius < windowSize.X && bulletPos.Y + radius < windowSize.Y
            && bulletPos.X - radius > 0 && bulletPos.Y - radius > 0;
        }
    }
}