using SFML.System;

namespace Game
{
    public class ShootAbility
    {
        public Ability ability = new();

        public Bullet SpawnBullet(Player owner, Vector2f bulletSpeed)
        {
            Bullet bullet = new();
            bullet.bulletShape = new(owner.playerShape.Radius / 4);
            bullet.bulletShape.FillColor = owner.playerShape.FillColor;
            bullet.bulletShape.Position = owner.playerShape.Position;
            bullet.bulletSpeed = bulletSpeed;
            bullet.ownerShape = owner.playerShape;
            return bullet;
        }
    }
}
