using SFML.System;
using SFML.Graphics;
using SFML.Window;

namespace Game
{
    public class ShootAbility
    {
        private float normalCooldown = 1f;
        public float currentCooldown;

        public Bullet bullet;

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

        public void ResetCooldownOfAbility()
            => currentCooldown = normalCooldown;
    }
}
