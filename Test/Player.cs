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
            playerShape = new(GameParametres.startSizeOfPlayer);
            playerShape.FillColor = playerShapeColor;
            isAlive = true;
            this.isBot = isBot;
            pointToGo = new(5);
            swapAbility.ability.SetNormalCooldown(3);
            shootAbility.ability.SetNormalCooldown(1);
            swapAbility.ability.ResetCooldownOfAbility();
            shootAbility.ability.ResetCooldownOfAbility();
            SetRandomPos(playerShape);
            SetRandomPos(pointToGo);
            if (keys != null && keyForSwap != null)
            {
                SetKeys((KeysForMoving)keys, (Keyboard.Key)keyForSwap);
            }
            return this;
        }

        private void SetKeys(KeysForMoving keys, Keyboard.Key keyForSwap)
        {
            this.keys = keys;
            swapAbility.ability.key = keyForSwap;
        }

        public Bullet Shoot(Vector2f bulletSpeed)
            => shootAbility.SpawnBullet(this, bulletSpeed);

        public void ShootAbility(List<Bullet> bulletList)
        {
            if (shootAbility.ability.CanCastAbility())
                return;
            Vector2f bulletSpeed = CustomRandom.Vector(new Vector2f(-5, -5), new Vector2f(5, 5));
            if (bulletSpeed.X == 0 && bulletSpeed.Y == 0)
            {
                bulletSpeed = CustomRandom.Vector(new Vector2f(1, 1), new Vector2f(6, 6));
            }
            bulletList.Add(Shoot(bulletSpeed));
            shootAbility.ability.ResetCooldownOfAbility();
        }

        public void SwapAbility(List<Player> playerList, Vector2f windowSize)
        {
            if (!Keyboard.IsKeyPressed(swapAbility.ability.key))
                return;
            float minDistance = windowSize.X + windowSize.Y;
            Player? p = null;
            foreach (Player otherPlayer in playerList)
            {
                float curDistance = swapAbility.CalculateDistance(playerShape.Position, otherPlayer.playerShape.Position);
                if (curDistance < minDistance && otherPlayer != this && otherPlayer.isAlive
                    && !swapAbility.ability.CanCastAbility())
                {
                    minDistance = curDistance;
                    p = otherPlayer;
                }
            }
            if (p != null)
            {
                swapAbility.SwapPlayers(p, this);
                swapAbility.ability.ResetCooldownOfAbility();
            }
        }

        public void UpdateCooldowns(float time)
        {
            shootAbility.ability.UpdateCooldown(time);
            swapAbility.ability.UpdateCooldown(time);
        }

        public void Moving()
        {
            float playerSpeed = 7f;
            Vector2f movePlayer;
            if (!isBot)
                movePlayer = PlayerMove(playerSpeed);
            else
                movePlayer = BotMove(playerSpeed);
            playerShape.Position += movePlayer;
        }

        private Vector2f BotMove(float playerSpeed)
        {
            Vector2f playerPos = playerShape.Position;
            Vector2f pointToGoPos = pointToGo.Position;
            Vector2f movePlayer = new(0, 0);
            if (!Collisions.CollideY(playerShape, pointToGo))
            {
                if (playerPos.Y < pointToGoPos.Y)
                {
                    movePlayer.Y += playerSpeed;
                }
                else
                {
                    movePlayer.Y -= playerSpeed;
                }
            }
            if (!Collisions.CollideX(playerShape, pointToGo))
            {
                if (playerPos.X < pointToGoPos.X)
                {
                    movePlayer.X += playerSpeed;
                }
                else
                {
                    movePlayer.X -= playerSpeed;
                }
            }
            if (Collisions.Collide(playerShape, pointToGo))
            {
                SetRandomPos(pointToGo);
            }
            return movePlayer;
        }

        public void TryRespawnPlayer(float time)
        {
            currentTimeForRevive -= time;
            if (currentTimeForRevive <= 0 && !isAlive)
            {
                SetRandomPos(playerShape);
                playerShape.Radius = GameParametres.startSizeOfPlayer;
                isAlive = true;
            }
        }

        private Vector2f PlayerMove(float playerSpeed)
        {
            Vector2f movePlayer = new(0, 0);
            if (Keyboard.IsKeyPressed(keys.DownKey))
            {
                movePlayer.Y += playerSpeed;
            }
            else if (Keyboard.IsKeyPressed(keys.UpKey))
            {
                movePlayer.Y -= playerSpeed;
            }
            if (Keyboard.IsKeyPressed(keys.RightKey))
            {
                movePlayer.X += playerSpeed;
            }
            else if (Keyboard.IsKeyPressed(keys.LeftKey))
            {
                movePlayer.X -= playerSpeed;
            }
            return movePlayer;
        }

        public void Die()
        {
            isAlive = false;
            currentTimeForRevive = GameParametres.timeForRevivePlayer;
        }

        public void SetRandomPos(CircleShape shape)
        {
            Vector2f min = new(shape.Radius, shape.Radius);
            Vector2f max = new(Game.window.Size.X - shape.Radius, Game.window.Size.Y - shape.Radius);
            shape.Position = CustomRandom.Vector(min, max);
        }
    }
}
