using SFML.Window;
using SFML.Graphics;
using SFML.System;
//using System.Configuration;

namespace Game
{
    public class Game
    {
        private RenderWindow window;

        private List<CircleShape> foodList = new();
        private List<Bullet> bulletList = new();
        private List<Player> playerList = new();

        private void Init()
        {
            //var x = Convert.ToUInt32(ConfigurationManager.AppSettings.Get("Window_X"));
            //var y = Convert.ToUInt32(ConfigurationManager.AppSettings.Get("Window_Y"));
            window = new(new VideoMode(1600, 900), "Game");
            window.SetFramerateLimit(60);
            window.Closed += WindowClosed;

            InitPlayers();
        }

        private void InitPlayers()
        {
            SetStartPlayerSettings(null, null, Color.Blue, true);
            KeysForMoving keysForMoving = new(Keyboard.Key.Up, Keyboard.Key.Down, Keyboard.Key.Left, Keyboard.Key.Right);
            SetStartPlayerSettings(keysForMoving, Keyboard.Key.R, Color.Red, false);
            SetStartPlayerSettings(null, null, Color.Black, true);
        }

        private Player SetStartPlayerSettings(KeysForMoving? keys, Keyboard.Key? keyForSwap, Color playerShapeColor, bool isBot)
        {
            Player player = new();
            player = player.SetStartPlayerSettings(keys, keyForSwap, playerShapeColor, isBot);
            SetOrigin(player.playerShape);
            player.ChangePos(player.playerShape, window);
            player.ChangePos(player.pointToGo, window);
            playerList.Add(player);
            return player;
        }

        public void GameLoop()
        {
            float foodSpawnRate = 0.5f;
            float currentTimeToSpawnFood = foodSpawnRate;
            Clock time = new();
            Init();
            while (window.IsOpen)
            {
                if (currentTimeToSpawnFood <= 0)
                {
                    currentTimeToSpawnFood = foodSpawnRate;
                    foodList.Add(SpawnFood());
                }
                PlayerUpdate(time.ElapsedTime.AsSeconds());
                BulletMoving();
                currentTimeToSpawnFood -= time.ElapsedTime.AsSeconds();
                time.Restart();
                Draw();
            }
        }

        private void PlayerUpdate(float time)
        {
            foreach (Player player in playerList)
            {
                player.shootAbility.currentCooldown -= time;
                player.swapAbility.currentCooldown -= time;
                if (player.isAlive)
                {
                    Moving(player);
                    if (Keyboard.IsKeyPressed(player.swapAbility.key))
                        SwapAbility(player);
                    ShootAbility(player);
                    CheckEating(player);
                    AntiStack(player.playerShape);
                }
                else
                {
                    TryRespawnPlayer(time, player);
                }
            }
        }

        private void BulletMoving()
        {
            List<Bullet> curBulletList = new(bulletList);
            foreach (Bullet bullet in curBulletList)
            {
                bullet.MoveBullet(bullet);
                if (!BulletInScreen(bullet.bulletShape.Position, bullet.bulletShape.Radius))
                {
                    DeleteBullet(bullet);
                }
                foreach (Player player in playerList)
                {
                    if (Collide(bullet.bulletShape, player.playerShape) && player.playerShape != bullet.owner)
                    {
                        player.playerShape.Radius -= bullet.bulletShape.Radius / 2;
                        SetOrigin(player.playerShape);
                        if (player.playerShape.Radius < 10)
                            DiePlayer(player);
                        DeleteBullet(bullet);
                    }
                }
            }
        }

        private void DeleteBullet(Bullet bullet)
        {
            bulletList.Remove(bullet);
        }

        private void ShootAbility(Player player)
        {
            if (player.shootAbility.currentCooldown > 0)
                return;
            Vector2f bulletSpeed = CustomRandom.Vector(new Vector2f(-5, -5), new Vector2f(5, 5));
            if (bulletSpeed.X == 0 && bulletSpeed.Y == 0)
            {
                bulletSpeed = CustomRandom.Vector(new Vector2f(1, 1), new Vector2f(6, 6));
            }
            bulletList.Add(player.shootAbility.SpawnBullet(player, bulletSpeed));
            player.shootAbility.ResetCooldownOfAbility();
        }

        private void SwapAbility(Player player)
        {
            float minDistance = window.Size.X + window.Size.Y;
            Player? p = null;
            foreach (Player otherPlayer in playerList)
            {
                float curDistance = player.swapAbility.CalculateDistance(player.playerShape.Position, otherPlayer.playerShape.Position);
                if (curDistance < minDistance && otherPlayer != player && otherPlayer.isAlive
                    && player.swapAbility.currentCooldown < 0)
                {
                    minDistance = curDistance;
                    p = otherPlayer;
                }
            }
            if (p != null)
            {
                player.swapAbility.SwapPlayers(p, player);
                player.swapAbility.ResetCooldownOfAbility();
            }
        }

        private void TryRespawnPlayer(float time, Player player)
        {
            player.currentTimeForRevive -= time;
            if (player.currentTimeForRevive <= 0 && !player.isAlive)
            {
                player.ChangePos(player.playerShape, window);
                player.playerShape.Radius = 25;
                player.isAlive = true;
            }
        }

        private CircleShape SpawnFood()
        {
            CircleShape food = new(15);
            SetOrigin(food);
            food.FillColor = CustomRandom.Color();
            food.Position = CustomRandom.Vector(new Vector2f(food.Radius, food.Radius), 
                new Vector2f(window.Size.X - food.Radius, window.Size.Y - food.Radius));
            return food;
        }

        private bool BulletInScreen(Vector2f bulletPos, float radius)
            => bulletPos.X + radius < window.Size.X && bulletPos.Y + radius < window.Size.Y
            && bulletPos.X - radius > 0 && bulletPos.Y - radius > 0;

        private bool Collide(CircleShape firstCircle, CircleShape secondCircle)
            => CollideX(firstCircle, secondCircle) && CollideY(firstCircle, secondCircle);

        private bool CollideX(CircleShape firstCircle, CircleShape secondCircle)
            => Math.Abs(secondCircle.Position.X - firstCircle.Position.X) <= secondCircle.Radius + firstCircle.Radius;

        private bool CollideY(CircleShape firstCircle, CircleShape secondCircle)
            => Math.Abs(secondCircle.Position.Y - firstCircle.Position.Y) <= secondCircle.Radius + firstCircle.Radius;

        private void Moving(Player player)
        {
            float playerSpeed = 7f;
            Vector2f movePlayer;
            if (!player.isBot)
                movePlayer = PlayerMove(player, playerSpeed);
            else
                movePlayer = BotMove(player, playerSpeed);
            player.playerShape.Position += movePlayer;
        }

        private Vector2f BotMove(Player player, float playerSpeed)
        {
            Vector2f playerPos = player.playerShape.Position;
            Vector2f pointToGoPos = player.pointToGo.Position;
            Vector2f movePlayer = new(0, 0);
            if (!CollideY(player.playerShape, player.pointToGo))
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
            if (!CollideX(player.playerShape, player.pointToGo))
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
            if (Collide(player.playerShape, player.pointToGo))
            {
                player.ChangePos(player.pointToGo, window);
            }
            return movePlayer;
        }

        private Vector2f PlayerMove(Player player, float playerSpeed)
        {            
            Vector2f movePlayer = new(0, 0);
            if (Keyboard.IsKeyPressed(player.keys.DownKey))
            {
                movePlayer.Y += playerSpeed;
            }
            else if (Keyboard.IsKeyPressed(player.keys.UpKey))
            {
                movePlayer.Y -= playerSpeed;
            }
            if (Keyboard.IsKeyPressed(player.keys.RightKey))
            {
                movePlayer.X += playerSpeed;
            }
            else if (Keyboard.IsKeyPressed(player.keys.LeftKey))
            {
                movePlayer.X -= playerSpeed;
            }
            return movePlayer;
        }

        private void CheckEating(Player player)
        {
            foreach (CircleShape food in foodList)
            {
                if (Collide(player.playerShape, food))
                {
                    foodList.Remove(food);
                    food.Dispose();
                    player.playerShape.Radius += 0.5f;
                    SetOrigin(player.playerShape);
                    break;
                }
            }
            foreach (Player otherPlayer in playerList)
            {
                if (Collide(player.playerShape, otherPlayer.playerShape) && otherPlayer.isAlive)
                {
                    if (player.playerShape.Radius > otherPlayer.playerShape.Radius)
                        EatingPlayer(otherPlayer, player);
                    else if (player.playerShape.Radius < otherPlayer.playerShape.Radius)
                        EatingPlayer(player, otherPlayer);
                    break;
                }
            }
        }

        private void EatingPlayer(Player playerForDestroy, Player playerForReward)
        {
            DiePlayer(playerForDestroy);
            playerForReward.playerShape.Radius += playerForDestroy.playerShape.Radius / 4;
            SetOrigin(playerForReward.playerShape);
        }

        private void DiePlayer(Player player)
        {
            int timeForRevivePlayer = 5;
            player.isAlive = false;
            player.currentTimeForRevive = timeForRevivePlayer;
        }

        private void AntiStack(CircleShape playerShape)
        {
            Vector2f PlayerPos = playerShape.Position;
            float radius = playerShape.Radius;
            if (PlayerPos.Y + radius > window.Size.Y)
            {
                PlayerPos.Y = window.Size.Y - radius;
            }
            if (PlayerPos.Y - radius < 0)
            {
                PlayerPos.Y = radius;
            }
            if (PlayerPos.X + radius > window.Size.X)
            {
                PlayerPos.X = window.Size.X - radius;
            }
            if (PlayerPos.X - radius < 0)
            {
                PlayerPos.X = radius;
            }
            playerShape.Position = PlayerPos;
        }

        private void Draw()
        {
            window.DispatchEvents();
            window.Clear(Color.Cyan);
            foreach (Shape food in foodList)
            {
                window.Draw(food);
            }
            foreach (Bullet bullet in bulletList)
            {
                window.Draw(bullet.bulletShape);
            }
            foreach (Player player in playerList)
            {
                if (player.isAlive)
                    window.Draw(player.playerShape);
            }
            window.Display();
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            RenderWindow w = (RenderWindow)sender;
            w.Close();
        }

        private void SetOrigin(CircleShape circle)
            => circle.Origin = new Vector2f(circle.Radius, circle.Radius);
    }
}