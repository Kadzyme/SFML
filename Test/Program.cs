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

        public float currentTimeForRevive = 0;
    }

    public class Game
    {
        private RenderWindow window = new RenderWindow(new VideoMode(1600, 900), "Game");

        private Player rightPlayer = new();
        private Player leftPlayer = new();

        private List<CircleShape> foodList = new();
        private List<Player> playerList = new();

        //private List<CircleShape> listToDraw = new();

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
            player.playerShape = new(30);
            player.DownKey = DownKey;
            player.UpKey = UpKey;
            player.LeftKey = LeftKey;
            player.RightKey = RightKey;
            player.playerShape.FillColor = playerShapeColor;
            Random rand = new Random();
            player.playerShape.Origin = new Vector2f(player.playerShape.Radius, player.playerShape.Radius);
            player.playerShape.Position = new Vector2f(rand.Next((int)player.playerShape.Radius, (int)window.Size.X - (int)player.playerShape.Radius), rand.Next((int)player.playerShape.Radius, (int)window.Size.Y - (int)player.playerShape.Radius));
            playerList.Add(player);
            return player;
        }

        public void GameLoop()
        {
            float foodSpawnRate = 1f;
            Clock time = new();
            Init();
            while (window.IsOpen)
            {
                PlayerMove(leftPlayer);
                PlayerMove(rightPlayer);
                if (time.ElapsedTime.AsSeconds() >= foodSpawnRate)
                {
                    foodList.Add(SpawnFood());
                    time.Restart();
                }
                foreach (Player player in playerList)
                {
                    player.currentTimeForRevive -= time.ElapsedTime.AsSeconds();
                }
                Draw();
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
                if (Collide(player.playerShape, otherPlayer.playerShape))
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
            playerList.Remove(playerForDestroy);
            playerForDestroy.playerShape.Dispose();
            playerForDestroy.currentTimeForRevive = timeForRevivePlayer;
            //playerForReward.playerShape.Radius += playerForDestroy.playerShape.Radius;
            playerForReward.playerShape.Origin = new Vector2f(playerForReward.playerShape.Radius, playerForReward.playerShape.Radius);
        }

        private CircleShape SpawnFood()
        {
            Random rand = new Random();
            CircleShape food = new(15);
            food.FillColor = new Color((byte)rand.Next(0, 255), (byte)rand.Next(0, 255), (byte)rand.Next(0, 255));
            food.Position = new Vector2f(rand.Next((int)food.Radius, (int)window.Size.X - (int)food.Radius), rand.Next((int)food.Radius, (int)window.Size.Y - (int)food.Radius));
            return food;
        }

        private bool Collide(CircleShape firstCircle, CircleShape secondCircle)
            => secondCircle.Position.X + secondCircle.Radius >= firstCircle.Position.X - firstCircle.Radius
            && secondCircle.Position.X - secondCircle.Radius <= firstCircle.Position.X + firstCircle.Radius
            && secondCircle.Position.Y - secondCircle.Radius <= firstCircle.Position.Y + firstCircle.Radius
            && secondCircle.Position.Y + secondCircle.Radius >= firstCircle.Position.Y - firstCircle.Radius;

        private void PlayerMove(Player player)
        {
            float playerSpeed = 7;
            Vector2f movePlayer = new(0,0);
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
