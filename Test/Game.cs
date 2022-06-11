using SFML.Window;
using SFML.Graphics;
using SFML.System;

namespace Game
{
    public class Game
    {
        private RenderWindow window = new(new VideoMode(1600, 900), "Game");

        private List<CircleShape> foodList = new();
        private List<Player> playerList = new();

        private void Init()
        {
            window.SetFramerateLimit(60);
            window.Closed += WindowClosed;

            SetStartPlayerSettings(null, null, Color.Blue, true);
            SetStartPlayerSettings(new KeysForMoving(Keyboard.Key.Up, Keyboard.Key.Down, Keyboard.Key.Left, Keyboard.Key.Right), Keyboard.Key.R, Color.Red, false);
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
            float playerSpeed = 7;
            Clock time = new();
            Init();
            while (window.IsOpen)
            {
                foreach (Player player in playerList)
                {
                    player.swapAbility.currentCooldown -= time.ElapsedTime.AsSeconds();
                    if (player.isAlive)
                    {
                        if (!player.isBot)
                            PlayerMove(player, playerSpeed);
                        else
                            BotMove(player, playerSpeed);
                        if (Keyboard.IsKeyPressed(player.swapAbility.key))
                            SwapAbility(player);
                        AntiStack(player);
                    }
                    else
                    {
                        TryRespawnPlayer(time.ElapsedTime.AsSeconds(), player);
                    }
                }
                if (currentTimeToSpawnFood <= 0)
                {
                    currentTimeToSpawnFood = foodSpawnRate;
                    foodList.Add(SpawnFood());
                }
                currentTimeToSpawnFood -= time.ElapsedTime.AsSeconds();
                time.Restart();
                Draw();
            }
        }

        public void SwapAbility(Player player)
        {
            float minDistance = window.Size.X + window.Size.Y;
            Player? p = null;
            foreach (Player otherPlayer in playerList)
            {
                float curDistance = player.swapAbility.Distance(player.playerShape.Position, otherPlayer.playerShape.Position);
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

        private bool Collide(CircleShape firstCircle, CircleShape secondCircle)
            => CollideX(firstCircle, secondCircle) && CollideY(firstCircle, secondCircle);

        private bool CollideX(CircleShape firstCircle, CircleShape secondCircle)
            => Math.Abs(secondCircle.Position.X - firstCircle.Position.X) <= secondCircle.Radius + firstCircle.Radius;

        private bool CollideY(CircleShape firstCircle, CircleShape secondCircle)
            => Math.Abs(secondCircle.Position.Y - firstCircle.Position.Y) <= secondCircle.Radius + firstCircle.Radius;

        private void BotMove(Player player, float playerSpeed)
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
            if (playerOnBounds(player.playerShape.Position + movePlayer, player.playerShape.Radius))
            {
                player.playerShape.Position += movePlayer;
            }
            movePlayer = new(0, 0);
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
            if (playerOnBounds(player.playerShape.Position + movePlayer, player.playerShape.Radius))
            {
                player.playerShape.Position += movePlayer;
            }
            if (Collide(player.playerShape, player.pointToGo))
            {
                player.ChangePos(player.pointToGo, window);
            }
            CheckEating(player);
        }

        private void PlayerMove(Player player, float playerSpeed)
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
            if (playerOnBounds(player.playerShape.Position + movePlayer, player.playerShape.Radius))
            {
                player.playerShape.Position += movePlayer;
            }
            movePlayer = new(0, 0);
            if (Keyboard.IsKeyPressed(player.keys.RightKey))
            {
                movePlayer.X += playerSpeed;
            }
            else if (Keyboard.IsKeyPressed(player.keys.LeftKey))
            {
                movePlayer.X -= playerSpeed;
            }
            if (playerOnBounds(player.playerShape.Position + movePlayer, player.playerShape.Radius))
            {
                player.playerShape.Position += movePlayer;
            }
            CheckEating(player);
        }

        private bool playerOnBounds(Vector2f playerPos, float radius)
            => playerPos.Y + radius < window.Size.Y && playerPos.Y - radius > 0
            && playerPos.X + radius < window.Size.X && playerPos.X - radius > 0;

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
            int timeForRevivePlayer = 5;
            playerForDestroy.isAlive = false;
            playerForDestroy.currentTimeForRevive = timeForRevivePlayer;
            playerForReward.playerShape.Radius += playerForDestroy.playerShape.Radius / 4;
            SetOrigin(playerForReward.playerShape);
        }

        private void AntiStack(Player player)
        {
            Vector2f PlayerPos = player.playerShape.Position;
            float radius = player.playerShape.Radius;
            if (PlayerPos.Y + radius > window.Size.Y)
            {
                PlayerPos.Y = window.Size.Y - radius - 1;
            }
            if (PlayerPos.Y - radius < 0)
            {
                PlayerPos.Y = radius + 1;
            }
            if (PlayerPos.X + radius > window.Size.X)
            {
                PlayerPos.X = window.Size.X - radius - 1;
            }
            if (PlayerPos.X - radius < 0)
            {
                PlayerPos.X = radius + 1;
            }
            player.playerShape.Position = PlayerPos;
        }

        private void Draw()
        {
            window.DispatchEvents();
            window.Clear(Color.Cyan);
            foreach (CircleShape food in foodList)
            {
                window.Draw(food);
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