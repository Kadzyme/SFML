using SFML.Graphics;

namespace Game
{
    public class Collisions
    {
        public static bool Collide(CircleShape firstCircle, CircleShape secondCircle)
            => CollideX(firstCircle, secondCircle) && CollideY(firstCircle, secondCircle);

        public static bool CollideX(CircleShape firstCircle, CircleShape secondCircle)
            => Math.Abs(secondCircle.Position.X - firstCircle.Position.X) <= secondCircle.Radius + firstCircle.Radius;

        public static bool CollideY(CircleShape firstCircle, CircleShape secondCircle)
            => Math.Abs(secondCircle.Position.Y - firstCircle.Position.Y) <= secondCircle.Radius + firstCircle.Radius;
    }
}