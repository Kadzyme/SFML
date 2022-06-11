using SFML.System;
using SFML.Graphics;

namespace Game
{
    public class CustomRandom
    {
        private static Random rand = new();

        public static Vector2f Vector(Vector2f min, Vector2f max)
        {
            float x = rand.Next((int)min.X, (int)max.X);
            float y = rand.Next((int)min.Y, (int)max.Y);
            Vector2f result = new(x, y);
            return result;
        }

        public static Color Color()
            => new Color((byte)rand.Next(0, 256), (byte)rand.Next(0, 256), (byte)rand.Next(0, 256));
    }
}
