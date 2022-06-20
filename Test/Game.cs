using SFML.Window;
using SFML.Graphics;
using SFML.System;
using System.Xml;

namespace Game
{
    public class Game
    {
        public static RenderWindow window;

        private List<CircleShape> foodList = new();
        private List<Bullet> bulletList = new();
        private List<Player> playerList = new();

        private void Init()
        {
            XmlDocument settingsXml = new();
            settingsXml.Load("C:\\Users\\Vano\\source\\repos\\Test\\Test\\Settings.xml");
            Vector2u size = new(0, 0);
            foreach (XmlElement element in settingsXml.DocumentElement)
            {
                foreach (XmlNode node in element.ChildNodes)
                {
                    switch(node.Name)
                    {
                        case "windowSizeX":
                            size.X = uint.Parse(node.InnerText);
                            break;
                        case "windowSizeY":
                            size.Y = uint.Parse(node.InnerText);
                            break;
                        case "minSizeOfPlayer":
                            GameParametres.minSizeOfPlayer = float.Parse(node.InnerText);
                            break;
                        case "startSizeOfPlayer":
                            GameParametres.startSizeOfPlayer = float.Parse(node.InnerText);
                            break;
                        case "multiplierOfEatenFood":
                            GameParametres.multiplierOfEatenFood = float.Parse(node.InnerText) /*/ 100*/;
                            break;
                        case "multiplierOfEatenPlayers":
                            GameParametres.multiplierOfEatenPlayers = float.Parse(node.InnerText) /*/ 100*/;
                            break;
                        case "sizeOfTheBullets":
                            GameParametres.sizeOfTheBullets = float.Parse(node.InnerText) /*/ 100*/;
                            break;
                        case "bulletDamageMultiplier":
                            GameParametres.bulletDamageMultiplier = float.Parse(node.InnerText) /*/ 100*/;
                            break;
                        case "timeForRevivePlayer":
                            GameParametres.timeForRevivePlayer = float.Parse(node.InnerText);
                            break;
                    }
                }
            }
            window = new(new VideoMode(size.X, size.Y), "Game");
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

        private void SetStartPlayerSettings(KeysForMoving? keys, Keyboard.Key? keyForSwap, Color playerShapeColor, bool isBot)
        {
            Player player = new();
            playerList.Add(player.SetStartPlayerSettings(keys, keyForSwap, playerShapeColor, isBot));
            SetOrigin(player.playerShape);
        }

        public void GameLoop()
        {
            float foodSpawnRate = 0.4f;
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
                BulletUpdate();
                currentTimeToSpawnFood -= time.ElapsedTime.AsSeconds();
                time.Restart();
                Draw();
            }
        }

        private void PlayerUpdate(float time)
        {
            foreach (Player player in playerList)
            {
                player.UpdateCooldowns(time);
                if (player.isAlive)
                {
                    player.Moving();
                    player.SwapAbility(playerList, (Vector2f)window.Size);
                    player.ShootAbility(bulletList);
                    TryEatAnything(player);
                    AntiStack(player.playerShape);
                }
                else
                {
                    player.TryRespawnPlayer(time);
                }
            }
        }

        private void BulletUpdate()
        {
            List<Bullet> curBulletList = new(bulletList);
            foreach (Bullet bullet in curBulletList)
            {
                bullet.MoveBullet(bullet);
                if (!bullet.BulletInScreen(window.Size))
                {
                    DeleteBullet(bullet);
                }
                foreach (Player player in playerList)
                {
                    if (Collisions.Collide(bullet.bulletShape, player.playerShape) && player.playerShape != bullet.ownerShape)
                    {
                        player.playerShape.Radius -= bullet.bulletShape.Radius * GameParametres.bulletDamageMultiplier;
                        SetOrigin(player.playerShape);
                        if (player.playerShape.Radius < GameParametres.minSizeOfPlayer)
                            player.Die();
                        DeleteBullet(bullet);
                    }
                }
            }
        }

        private void DeleteBullet(Bullet bullet)
        {
            bulletList.Remove(bullet);
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

        private void TryEatAnything(Player player)
        {
            List<CircleShape> listFood = new(foodList);
            foreach (CircleShape food in listFood)
            {
                if (Collisions.Collide(player.playerShape, food))
                {
                    foodList.Remove(food);
                    food.Dispose();
                    player.playerShape.Radius += food.Radius * GameParametres.multiplierOfEatenFood;
                    SetOrigin(player.playerShape);
                }
            }
            List<Player> listPlayer = new(playerList);
            foreach (Player otherPlayer in listPlayer)
            {
                if (Collisions.Collide(player.playerShape, otherPlayer.playerShape) && otherPlayer.isAlive)
                {
                    if (player.playerShape.Radius > otherPlayer.playerShape.Radius)
                        EatingPlayer(otherPlayer, player);
                    else if (player.playerShape.Radius < otherPlayer.playerShape.Radius)
                        EatingPlayer(player, otherPlayer);
                }
            }
        }

        private void EatingPlayer(Player playerForDestroy, Player playerForReward)
        {
            playerForDestroy.Die();
            playerForReward.playerShape.Radius += playerForDestroy.playerShape.Radius * GameParametres.multiplierOfEatenPlayers;
            SetOrigin(playerForReward.playerShape);
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