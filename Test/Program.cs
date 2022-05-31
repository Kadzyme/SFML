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

    public class Player
    {
        public CircleShape playerShape;

        public Keyboard.Key DownKey;
        public Keyboard.Key UpKey;
        public Keyboard.Key LeftKey;
        public Keyboard.Key RightKey;

        public bool isAlive;
        public float currentTimeForRevive = 0;
    }

    public class Game
    {
        private RenderWindow window = new RenderWindow(new VideoMode(1600, 900), "Game");

        private Player rightPlayer = new();
        private Player leftPlayer = new();

        private List<CircleShape> foodList = new();
        private List<Player> playerList = new();

        private int timeForRevivePlayer = 5;

        private void Init()
        {
            window.SetFramerateLimit(60);
            window.Closed += WindowClosed;

            leftPlayer = SetStartPlayerSettings(leftPlayer, Keyboard.Key.W, Keyboard.Key.S, Keyboard.Key.A, Keyboard.Key.D, Color.Blue);

            rightPlayer = SetStartPlayerSettings(rightPlayer, Keyboard.Key.Up, Keyboard.Key.Down, Keyboard.Key.Left, Keyboard.Key.Right, Color.Red);
        }

        private Player SetStartPlayerSettings(Player player, Keyboard.Key UpKey, Keyboard.Key DownKey, Keyboard.Key LeftKey, Keyboard.Key RightKey, Color playerShapeColor)
        {
            player.playerShape = new(25);
            player.DownKey = DownKey;
            player.UpKey = UpKey;
            player.LeftKey = LeftKey;
            player.RightKey = RightKey;
            player.playerShape.FillColor = playerShapeColor;
            Random rand = new Random();
            player.playerShape.Origin = new Vector2f(player.playerShape.Radius, player.playerShape.Radius);
            player.playerShape.Position = new Vector2f(rand.Next((int)player.playerShape.Radius, (int)window.Size.X - (int)player.playerShape.Radius), rand.Next((int)player.playerShape.Radius, (int)window.Size.Y - (int)player.playerShape.Radius));
            playerList.Add(player);
            player.isAlive = true;
            return player;
        }

        public void GameLoop()
        {
            float foodSpawnRate = 1f;
            float currentTimeToSpawnFood = foodSpawnRate;
            Clock time = new();
            Init();
            while (window.IsOpen)
            {
                PlayerMove(leftPlayer);
                PlayerMove(rightPlayer);
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
                    Random rand = new();
                    player.playerShape.Position = new Vector2f(rand.Next((int)player.playerShape.Radius, (int)window.Size.X - (int)player.playerShape.Radius), rand.Next((int)player.playerShape.Radius, (int)window.Size.Y - (int)player.playerShape.Radius));
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

        private void PlayerMove(Player player)
        {
            if (player.isAlive)
            {
                float playerSpeed = 7;
                Vector2f movePlayer = new(0, 0);
                if (Keyboard.IsKeyPressed(player.DownKey) && player.playerShape.Position.Y + player.playerShape.Radius < window.Size.Y)
                {
                    movePlayer.Y += playerSpeed;
                }
                else if (Keyboard.IsKeyPressed(player.UpKey) && player.playerShape.Position.Y - player.playerShape.Radius > 0)
                {
                    movePlayer.Y -= playerSpeed;
                }
                if (Keyboard.IsKeyPressed(player.RightKey) && player.playerShape.Position.X + player.playerShape.Radius < window.Size.X)
                {
                    movePlayer.X += playerSpeed;
                }
                else if (Keyboard.IsKeyPressed(player.LeftKey) && player.playerShape.Position.X - player.playerShape.Radius > 0)
                {
                    movePlayer.X -= playerSpeed;
                }
                player.playerShape.Position += movePlayer;
                CheckCollisions(player);
            }
        }

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
            playerForDestroy.isAlive = false;
            playerForDestroy.currentTimeForRevive = timeForRevivePlayer;
            playerForReward.playerShape.Radius += playerForDestroy.playerShape.Radius;
            playerForReward.playerShape.Origin = new Vector2f(playerForReward.playerShape.Radius, playerForReward.playerShape.Radius);
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
