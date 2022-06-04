using SFML.Window;
using SFML.Graphics;
using SFML.System;

namespace Game
{
    class Program
    {
        static void Main(string[] args)
        {
            Game start = new Game();
            start.GameLoop();
        }
    }

    public class CustomRandom
    {
        public static Vector2f Next(Vector2f min, Vector2f max)
        {
            Random rand = new();
            float x = rand.Next((int)min.X, (int)max.X);
            float y = rand.Next((int)min.Y, (int)max.Y);
            Vector2f result = new(x, y);
            return result;
        }
    }

    public class Player
    {
        public CircleShape playerShape;

        public Keys keys;

        public CircleShape pointToGo;
        public bool isBot;

        public bool isAlive;
        public float currentTimeForRevive = 0;

        public Player SetStartPlayerSettings(Keys keys, Color playerShapeColor, bool isBot)
        {
            Player player = new();
            player.playerShape = new(25);
            player.keys = keys;
            player.playerShape.FillColor = playerShapeColor;
            player.playerShape.Origin = new Vector2f(player.playerShape.Radius, player.playerShape.Radius);
            player.isAlive = true;
            player.isBot = isBot;
            return player;
        }

        public void ChangePlayerPos(CircleShape playerShape, RenderWindow window)
        {
            Vector2f min = new(playerShape.Radius, playerShape.Radius);
            Vector2f max = new(window.Size.X - playerShape.Radius, window.Size.Y - playerShape.Radius);
            playerShape.Position = CustomRandom.Next(min, max);
        }
    }

    public struct Keys
    {
        public Keyboard.Key UpKey;
        public Keyboard.Key DownKey;
        public Keyboard.Key LeftKey;
        public Keyboard.Key RightKey;

        public Keys(Keyboard.Key UpKey, Keyboard.Key DownKey, Keyboard.Key LeftKey, Keyboard.Key RightKey)
        {
            this.UpKey = UpKey;
            this.DownKey = DownKey;
            this.LeftKey = LeftKey;
            this.RightKey = RightKey;
        }
    }

    public class Game
    {
        private RenderWindow window = new(new VideoMode(1600, 900), "Game");

        private List<CircleShape> foodList = new();
        private List<Player> playerList = new();

        private void Init()
        {
            window.SetFramerateLimit(60);
            window.Closed += WindowClosed;

            SetStartPlayerSettings(new Keys(Keyboard.Key.W, Keyboard.Key.S, Keyboard.Key.A, Keyboard.Key.D), Color.Blue, true);
            SetStartPlayerSettings(new Keys(Keyboard.Key.Up, Keyboard.Key.Down, Keyboard.Key.Left, Keyboard.Key.Right), Color.Red, true);
            SetStartPlayerSettings(new Keys(Keyboard.Key.I, Keyboard.Key.K, Keyboard.Key.J, Keyboard.Key.L), Color.Green, true);
        }

        private Player SetStartPlayerSettings(Keys keys, Color playerShapeColor, bool isBot)
        {
            Player player = new();
            player = player.SetStartPlayerSettings(keys, playerShapeColor, isBot);
            player.ChangePlayerPos(player.playerShape, window);
            if (player.isBot)
            {
                player.pointToGo = new(5);
                player.ChangePlayerPos(player.pointToGo, window);
            }
            playerList.Add(player);
            return player;
        }

        public void GameLoop()
        {
            float foodSpawnRate = 1f;
            float currentTimeToSpawnFood = foodSpawnRate;
            float playerSpeed = 7;
            Clock time = new();
            Init();
            while (window.IsOpen)
            {
                foreach (Player player in playerList)
                {
                    if (player.isAlive)
                    {
                        if (!player.isBot)
                            PlayerMove(player, playerSpeed);
                        else
                            BotMove(player, playerSpeed);
                    }
                }
                CheckTimeForPlayerRespawn(time.ElapsedTime.AsSeconds());
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

        private void CheckTimeForPlayerRespawn(float time)
        {
            foreach (Player player in playerList)
            {
                player.currentTimeForRevive -= time;
                if (player.currentTimeForRevive <= 0 && !player.isAlive)
                {
                    player.ChangePlayerPos(player.playerShape, window);
                    player.isAlive = true;
                }
            }
        }

        private CircleShape SpawnFood()
        {
            Random rand = new Random();
            CircleShape food = new(15);
            food.Origin = new Vector2f(food.Radius, food.Radius);
            food.FillColor = new Color((byte)rand.Next(0, 255), (byte)rand.Next(0, 255), (byte)rand.Next(0, 255));
            food.Position = new Vector2f(rand.Next((int)food.Radius, (int)window.Size.X - (int)food.Radius), rand.Next((int)food.Radius, (int)window.Size.Y - (int)food.Radius));
            return food;
        }

        private bool Collide(CircleShape firstCircle, CircleShape secondCircle)
            => Math.Abs(secondCircle.Position.X - firstCircle.Position.X) <= secondCircle.Radius + firstCircle.Radius 
            && Math.Abs(secondCircle.Position.Y - firstCircle.Position.Y) <= secondCircle.Radius + firstCircle.Radius;

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
                player.ChangePlayerPos(player.pointToGo, window);
            }
            CheckCollisions(player);
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
            CheckCollisions(player);
        }

        private bool playerOnBounds(Vector2f PlayerPos, float radius)
            => PlayerPos.Y + radius < window.Size.Y && PlayerPos.Y - radius > 0
            && PlayerPos.X + radius < window.Size.X && PlayerPos.X - radius > 0;

        private void CheckCollisions(Player player)
        {
            foreach (CircleShape food in foodList)
            {
                if (Collide(player.playerShape, food))
                {
                    foodList.Remove(food);
                    food.Dispose();
                    player.playerShape.Radius += 0.5f;
                    player.playerShape.Origin = new Vector2f(player.playerShape.Radius, player.playerShape.Radius);
                    break;
                }
            }
            foreach (Player otherPlayer in playerList)
            {
                if (Collide(player.playerShape, otherPlayer.playerShape) && otherPlayer.isAlive
                    && player.playerShape.Radius != otherPlayer.playerShape.Radius)
                {
                    if (player.playerShape.Radius > otherPlayer.playerShape.Radius)
                        EatingPlayer(otherPlayer, player);
                    else
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
            playerForReward.playerShape.Radius += playerForDestroy.playerShape.Radius / 2;
            playerForReward.playerShape.Origin = new Vector2f(playerForReward.playerShape.Radius, playerForReward.playerShape.Radius);
            BugFix(playerForReward, playerForDestroy.playerShape.Radius / 2);
        }

        private void BugFix(Player player, float addedRadius)
        {
            Vector2f PlayerPos = player.playerShape.Position;
            float radius = player.playerShape.Radius;
            if (PlayerPos.Y + radius > window.Size.Y)
            {
                PlayerPos.Y -= addedRadius;
            }
            else if (PlayerPos.Y - radius < 0)
            {
                PlayerPos.Y += addedRadius;
            }
            if (PlayerPos.X + radius > window.Size.X)
            {
                PlayerPos.X -= addedRadius;
            }
            else if (PlayerPos.X - radius < 0)
            {
                PlayerPos.X += addedRadius;
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
    
    }
}