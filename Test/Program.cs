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


    }

    public class Game
    {
        public RenderWindow window = new RenderWindow(new VideoMode(1600, 900), "Game");

        public Player rightPlayer;
        public Player leftPlayer;

        private Ball ball;

        private void Init()
        {
            //window
            window.SetFramerateLimit(60);
            window.Closed += WindowClosed;
            //left player
            leftPlayer.playerShape = new RectangleShape(new Vector2f(120,25));
            leftPlayer.playerShape.FillColor = Color.Blue;
            leftPlayer.playerShape.Origin = new Vector2f(leftPlayer.playerShape.Size.X / 2, leftPlayer.playerShape.Size.Y / 2);
            leftPlayer.playerShape.Position = new Vector2f(leftPlayer.playerShape.Size.X / 2, leftPlayer.playerShape.Size.Y / 2);
            //right player
            rightPlayer.playerShape = new RectangleShape(new Vector2f(120, 25));
            rightPlayer.playerShape.FillColor = Color.Red;
            rightPlayer.playerShape.Origin = new Vector2f(rightPlayer.playerShape.Size.X / 2, rightPlayer.playerShape.Size.Y / 2);
            rightPlayer.playerShape.Position = new Vector2f(window.Size.X - rightPlayer.playerShape.Size.X / 2, rightPlayer.playerShape.Size.Y / 2);
            //ball
            ball.ballShape = new CircleShape(25);
            ball.ballShape.FillColor = Color.Green;
            ball.ballShape.Position = new Vector2f(window.Size.X / 2, window.Size.Y / 2);
            ball.ballShape.Origin = new Vector2f(ball.ballShape.Radius, ball.ballShape.Radius);
        }

        public void GameLoop()
        {
            Init();
            while (window.IsOpen)
            {
                PlayerMove(leftPlayer.UpKey, leftPlayer.DownKey, leftPlayer);
                PlayerMove(rightPlayer.UpKey, rightPlayer.DownKey, rightPlayer);
                BallMove(ball);
                Draw();
            }
        }

        private void BallMove(Ball ball)
        {
            var ballShape = ball.ballShape;
            var normalBallSpeed = ball.normalBallSpeed;
            var ballSpeedX = ball.ballSpeedX;
            var ballSpeedY = ball.ballSpeedY;

            if (ballShape.Position.X + ballShape.Radius >= window.Size.X || ballShape.Position.X - ballShape.Radius <= 0)
            {
                if (ballSpeedX > 0)
                    ballSpeedX = normalBallSpeed;
                else
                    ballSpeedX = -normalBallSpeed;
                if (ballSpeedY > 0)
                    ballSpeedY = normalBallSpeed;
                else
                    ballSpeedY = -normalBallSpeed;
                normalBallSpeed = ball.startBallSpeed;
                ballShape.Position = new Vector2f(window.Size.X / 2, window.Size.Y / 2);
            }
            if (ballShape.Position.X + ballShape.Radius >= rightPlayer.playerShape.Position.X - rightPlayer.playerShape.Size.X / 2 && BallTouchPlayer(rightPlayer, ball))
            {
                ballSpeedX = -normalBallSpeed;
            }
            else if (ballShape.Position.X - ballShape.Radius <= leftPlayer.playerShape.Position.X + leftPlayer.playerShape.Size.X / 2 && BallTouchPlayer(leftPlayer, ball))
            {
                ballSpeedX = normalBallSpeed;
            }
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
            => ball.ballShape.Position.Y - ball.ballShape.Radius <= player.playerShape.Position.Y + player.playerShape.Size.Y / 2
            && ball.ballShape.Position.Y + ball.ballShape.Radius >= player.playerShape.Position.Y - player.playerShape.Size.Y / 2;

        private float BallSpeedIncrease(float speed)
        {
            float speedIncrease = 0.005f;
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
