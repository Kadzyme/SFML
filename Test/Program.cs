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

    public struct Player
    {
        public RectangleShape playerShape;
        public Keyboard.Key DownKey;
        public Keyboard.Key UpKey;
        public int score;

        public Player(RectangleShape playerShape, Keyboard.Key DownKey, Keyboard.Key UpKey, int score)
        {
            this.playerShape = playerShape;
            this.DownKey = DownKey;
            this.UpKey = UpKey;
            this.score = score;
        }
    }

    public class Ball
    {
        public CircleShape ballShape;
        public Vector2f startBallPosition;
        public Player owner;

        public float startBallSpeed;
        public float ballSpeedX;
        public float ballSpeedY;
        public float normalBallSpeed;

        public void SetNormalSpeed()
        {
            normalBallSpeed = startBallSpeed;
            ballSpeedX = startBallSpeed;
            ballSpeedY = startBallSpeed;
        }

        public void SetStartSettings(float startSpeed, float ballRadius, Color ballShapeColor, Vector2f ballPosition)
        {
            startBallSpeed = startSpeed;
            ballShape = new(ballRadius);
            ballShape.FillColor = ballShapeColor;
            ballShape.Position = ballPosition;
            startBallPosition = ballPosition;
            ballShape.Origin = new Vector2f(ballShape.Radius, ballShape.Radius);
        }
    }

    public class Game
    {
        public RenderWindow window = new RenderWindow(new VideoMode(1600, 900), "Game");

        public Player rightPlayer;
        public Player leftPlayer;

        private CircleShape coin = new CircleShape(30);

        private Text scoreText = new();

        private Ball ball = new();

        private void Init()
        {
            window.SetFramerateLimit(60);
            window.Closed += WindowClosed;

            scoreText.Style = Text.Styles.Bold; 
            scoreText.Font = new Font("C:\\Windows\\Fonts\\Arial.ttf");
            scoreText.CharacterSize = 120;
            scoreText.Position = new Vector2f(window.Size.X / 2 - (scoreText.Scale.X * scoreText.CharacterSize), 0);

            leftPlayer.playerShape = new RectangleShape(new Vector2f(25, 120));
            leftPlayer = SetPlayerSettings(leftPlayer.playerShape.Size.X / 2, leftPlayer, Keyboard.Key.W, Keyboard.Key.S, Color.Blue);

            rightPlayer.playerShape = new RectangleShape(new Vector2f(25, 120));
            rightPlayer = SetPlayerSettings(window.Size.X - rightPlayer.playerShape.Size.X / 2, rightPlayer, Keyboard.Key.Up, Keyboard.Key.Down, Color.Red);

            coin.FillColor = Color.Yellow;
            coin.Origin = new Vector2f(coin.Radius, coin.Radius);
            CoinMove();

            SetNeutralOwnerForBall();
            ball.SetStartSettings(4f, 25f, Color.Green, new Vector2f(window.Size.X / 2, window.Size.Y / 2));
            ball.SetNormalSpeed();
        }

        private void SetNeutralOwnerForBall()
        {
            Player neutralPlayer = new();
            neutralPlayer.playerShape = new();
            neutralPlayer.score = 0;
            neutralPlayer.playerShape.FillColor = Color.Green;
            ball.owner = neutralPlayer;
        }

        private Player SetPlayerSettings(float playerPosX, Player player, Keyboard.Key UpKey, Keyboard.Key DownKey, Color playerShapeColor)
        {
            player.DownKey = DownKey;
            player.UpKey = UpKey;
            player.playerShape.FillColor = playerShapeColor;
            player.playerShape.Origin = new Vector2f(player.playerShape.Size.X / 2, player.playerShape.Size.Y / 2);
            player.playerShape.Position = new Vector2f(playerPosX, player.playerShape.Size.Y / 2);
            player.score = 0;
            return player;
        }

        public void GameLoop()
        {
            Init();
            while (window.IsOpen)
            {
                PlayerMove(leftPlayer.UpKey, leftPlayer.DownKey, leftPlayer);
                PlayerMove(rightPlayer.UpKey, rightPlayer.DownKey, rightPlayer);
                BallMove(ball);
                SetTextForScore();
                Draw();
            }
        }

        private void CoinMove()
        {
            Random rand = new Random();
            int radius = Convert.ToInt32(coin.Radius);
            int windowSizeX = Convert.ToInt32(window.Size.X);
            int windowSizeY = Convert.ToInt32(window.Size.Y);
            coin.Position = new Vector2f(rand.Next(radius * 2, windowSizeX - (radius * 2)), rand.Next(radius, windowSizeY - radius));
        }
        
        private void SetTextForScore()
        {
            scoreText.FillColor = Color.Green;
            scoreText.DisplayedString = $"{leftPlayer.score}:{rightPlayer.score}";
        }

        private void BallMove(Ball ball)
        {
            var ballShape = ball.ballShape;
            var normalBallSpeed = ball.normalBallSpeed;
            var ballSpeedX = ball.ballSpeedX;
            var ballSpeedY = ball.ballSpeedY;

            if (ballShape.Position.X + ballShape.Radius >= window.Size.X || ballShape.Position.X - ballShape.Radius <= 0)
            {
                normalBallSpeed = ball.startBallSpeed;
                if (ballSpeedX > 0)
                {
                    ballSpeedX = normalBallSpeed;
                }
                else
                {
                    ballSpeedX = -normalBallSpeed;
                }
                if (ballSpeedY > 0)
                    ballSpeedY = normalBallSpeed;
                else
                    ballSpeedY = -normalBallSpeed;
                if (ball.owner.playerShape == leftPlayer.playerShape)
                    leftPlayer.score++;
                else if (ball.owner.playerShape == rightPlayer.playerShape)
                    rightPlayer.score++;
                SetNeutralOwnerForBall();
                ballShape.Position = ball.startBallPosition;
            }
            if (BallTouchPlayer(rightPlayer, ball))
            {
                ballSpeedX = -normalBallSpeed;
                ball.owner = rightPlayer;
            }
            else if (BallTouchPlayer(leftPlayer, ball))
            {
                ballSpeedX = normalBallSpeed;
                ball.owner = leftPlayer;
            }
            if (BallTouchCoin(coin, ball))
            {
                if (ball.owner.playerShape == leftPlayer.playerShape)
                    leftPlayer.score++;                
                else if (ball.owner.playerShape == rightPlayer.playerShape)
                    rightPlayer.score++;
                CoinMove();
            }
            ball.ballShape.FillColor = ball.owner.playerShape.FillColor;
            if (ballShape.Position.Y - ballShape.Radius <= 0)
            {
                ballSpeedY = normalBallSpeed;
            }
            else if (ballShape.Position.Y + ballShape.Radius >= window.Size.Y)
            {
                ballSpeedY = -normalBallSpeed;
            }
            ballShape.Position += new Vector2f(ballSpeedX, ballSpeedY);

            ball.normalBallSpeed = BallSpeedIncrease(normalBallSpeed);
            ball.ballSpeedX = BallSpeedIncrease(ballSpeedX);
            ball.ballSpeedY = BallSpeedIncrease(ballSpeedY);
        }

        private bool BallTouchPlayer(Player player, Ball ball)
            => ball.ballShape.Position.X + ball.ballShape.Radius >= player.playerShape.Position.X - player.playerShape.Size.X / 2
            && ball.ballShape.Position.X - ball.ballShape.Radius <= player.playerShape.Position.X + player.playerShape.Size.X / 2
            && ball.ballShape.Position.Y - ball.ballShape.Radius <= player.playerShape.Position.Y + player.playerShape.Size.Y / 2
            && ball.ballShape.Position.Y + ball.ballShape.Radius >= player.playerShape.Position.Y - player.playerShape.Size.Y / 2;

        private bool BallTouchCoin(CircleShape coin, Ball ball)
            => ball.ballShape.Position.X + ball.ballShape.Radius >= coin.Position.X - coin.Radius
            && ball.ballShape.Position.X - ball.ballShape.Radius <= coin.Position.X + coin.Radius
            && ball.ballShape.Position.Y - ball.ballShape.Radius <= coin.Position.Y + coin.Radius
            && ball.ballShape.Position.Y + ball.ballShape.Radius >= coin.Position.Y - coin.Radius;

        private float BallSpeedIncrease(float speed)
        {
            float speedIncrease = 0.004f;
            Random rand = new Random();
            if (speed == 0)
                speed += speedIncrease * rand.Next(-1, 2);
            else if (speed > 0)
                speed += speedIncrease;
            else
                speed -= speedIncrease;
            return speed;
        }

        private void PlayerMove(Keyboard.Key UpKey, Keyboard.Key DownKey, Player player)
        {
            float playerSpeed = 7;
            float movePlayer = 0;
            if (Keyboard.IsKeyPressed(DownKey) && player.playerShape.Position.Y + player.playerShape.Size.Y / 2 < window.Size.Y)
            {
                movePlayer += playerSpeed;
            }
            else if (Keyboard.IsKeyPressed(UpKey) && player.playerShape.Position.Y - player.playerShape.Size.Y / 2 > 0)
            {
                movePlayer -= playerSpeed;
            }
            player.playerShape.Position += new Vector2f(0, movePlayer);
        }

        private void Draw()
        {
            window.DispatchEvents();
            window.Clear();
            window.Draw(scoreText);
            window.Draw(coin);
            window.Draw(ball.ballShape);
            window.Draw(leftPlayer.playerShape);
            window.Draw(rightPlayer.playerShape);
            window.Display();
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            RenderWindow w = (RenderWindow)sender;
            w.Close();
        }
    
    }
}
