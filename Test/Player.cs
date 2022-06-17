using SFML.System;
using SFML.Graphics;
using SFML.Window;

namespace Game
{
    public class Player
    {
        public CircleShape playerShape;

        public KeysForMoving keys;

        public PlayerSwapAbility swapAbility = new();
        public ShootAbility shootAbility = new();

        public CircleShape pointToGo;
        public bool isBot;

        public bool isAlive;
        public float currentTimeForRevive = 0;

        public Player SetStartPlayerSettings(KeysForMoving? keys, Keyboard.Key? keyForSwap, Color playerShapeColor, bool isBot)
        {
            Player player = new();
            player.playerShape = new(25);
            player.playerShape.FillColor = playerShapeColor;
            player.isAlive = true;
            player.isBot = isBot;
            player.pointToGo = new(5);
            player.swapAbility.ResetCooldownOfAbility();
            player.shootAbility.ResetCooldownOfAbility();
            if (keys != null && keyForSwap != null)
            {
                SetKeys(player, (KeysForMoving)keys, (Keyboard.Key)keyForSwap);
            }
            return player;
        }

        private void SetKeys(Player player, KeysForMoving keys, Keyboard.Key keyForSwap)
        {
            player.keys = keys;
            player.swapAbility.key = keyForSwap;
            player.swapAbility.key = keyForSwap;
        }

        public void ChangePos(CircleShape playerShape, RenderWindow window)
        {
            Vector2f min = new(playerShape.Radius, playerShape.Radius);
            Vector2f max = new(window.Size.X - playerShape.Radius, window.Size.Y - playerShape.Radius);
            playerShape.Position = CustomRandom.Vector(min, max);
        }
    }
}
