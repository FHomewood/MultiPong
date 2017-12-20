using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace MultiPong
{
    public class Main : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Camera camera;
        int Width = 1000, Height = 1000;
        MouseState oldM, newM;
        KeyboardState oldK, newK;
        Texture2D pixel;
        SpriteFont spriteFont;

        // Client variables //
        private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private const int dedicatedVarbytes = 7;
        private static byte clientID;
        private static Player[] playerList = new Player[10];
        private static Vector2 pukLoc;
        // Client variables //

        private static int playerNumber = 2, theta, edgeLength, radius = 400;
        List<Vector2> circlePoints = new List<Vector2>();
        Vector2 pukVel;
        Player lastHit;

        public Main()
        {
            IsMouseVisible = true;
            Window.Title = "MultiPong_V0.0.1";
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = Height;
            graphics.PreferredBackBufferWidth  = Width;
            Content.RootDirectory = "Content";
        }
        protected override void Initialize()
        {
            base.Initialize();
            LoopConnect();
            oldK = Keyboard.GetState();
            oldM = Mouse   .GetState();
            camera = new Camera(GraphicsDevice.Viewport);
            for (int i = 0; i < 3600; i++)
            { circlePoints.Add(new Vector2(Width,Height)/2 + Vector2.Transform(-radius * Vector2.UnitY, Matrix.CreateRotationZ(MathHelper.ToRadians((float)i/10)))); }
            for (int i = 0; i <= playerNumber; i++)
            {
                playerList[i] = new Player(i, 0.5f, 0f);
            }
            pukLoc = new Vector2(Width, Height) / 2;
            Random rand = new Random();
            if (clientID == 0) pukVel = Vector2.Transform(2* Vector2.UnitY, Matrix.CreateRotationZ((float)(2 * Math.PI * rand.NextDouble())));
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            pixel = Content.Load<Texture2D>("Pixel2");
            spriteFont = Content.Load<SpriteFont>("spriteFont");
        }
        protected override void UnloadContent()
        {
        }
        protected override void Update(GameTime gameTime)
        {
            newK = Keyboard.GetState();
            newM = Mouse   .GetState();
            playerList[clientID].Playing = true;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (newK.IsKeyDown(Keys.Up) && oldK.IsKeyUp(Keys.Up) && playerNumber < 9) { ; playerNumber++;  playerList[playerNumber] = new Player(playerNumber, 0.5f, 0f); }
            if (newK.IsKeyDown(Keys.Down) && oldK.IsKeyUp(Keys.Down) && playerNumber > 2) { playerNumber--; playerList[playerNumber+1] = null; }

            foreach (Player player in playerList) if (player != null)
                {
                    player.Update(newK, camera, playerNumber, edgeLength);
                    if (clientID == 0 && player.CollisionDetect(pukLoc, pukVel) != pukVel)
                    {
                        pukVel = player.CollisionDetect(pukLoc, pukVel);
                        lastHit = player;
                    }
                }
            camera.Update(new Vector2(Width/2, Height/2));

            //puk logic//
            if (clientID == 0)
            {
                pukLoc += pukVel;
                if ((pukLoc - new Vector2(Width, Height) / 2).Length() > 1.4f * radius)
                {
                    pukLoc = new Vector2(Width, Height) / 2;
                    if (lastHit != null) lastHit.score++;
                    Random rand = new Random();
                    pukVel = Vector2.Transform(2 * Vector2.UnitY, Matrix.CreateRotationZ((float)(2 * Math.PI * rand.NextDouble())));
                    lastHit = null;
                }
            }
            oldK = newK;
            oldM = newM;
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Deferred,BlendState.AlphaBlend,
                null, null, null, null, camera.Transform);
            foreach (Vector2 vector in circlePoints) spriteBatch.Draw(pixel, vector, null, Color.Blue, 0, Vector2.One, 1f, SpriteEffects.None, 0);
            if (playerNumber > 1)
                for (float i = 0; i <= playerNumber; i++)
                {
                    Color color = new Color();
                    Vector2 vertex = new Vector2();
                    Vector2 newvertex = new Vector2();
                    color = Color.FromNonPremultiplied((int)(255 * (1 + Math.Cos(2 * Math.PI * (i / (float)playerNumber)))), (int)(255 * (1 + Math.Cos(Math.PI / 2 + 2 * Math.PI * (i / (float)playerNumber)))), 128, 255);
                    if (playerNumber % 2 == 0)
                    {
                        vertex = new Vector2(Width, Height) / 2 + Vector2.Transform(-radius * Vector2.UnitY, Matrix.CreateRotationZ(MathHelper.ToRadians(i * (360 / (float)(playerNumber + 1)))));
                        newvertex = new Vector2(Width, Height) / 2 + Vector2.Transform(-radius * Vector2.UnitY, Matrix.CreateRotationZ(MathHelper.ToRadians((i + 1) * (360 / (float)(playerNumber+1)))));
                    }
                    else
                    {
                        vertex = new Vector2(Width, Height) / 2 + Vector2.Transform(-radius * Vector2.UnitY, Matrix.CreateRotationZ(MathHelper.ToRadians((i + 0.5f) * (360 / (float)(playerNumber + 1)))));
                        newvertex = new Vector2(Width, Height) / 2 + Vector2.Transform(-radius * Vector2.UnitY, Matrix.CreateRotationZ(MathHelper.ToRadians((i + 1.5f) * (360 / (float)(playerNumber + 1)))));
                    }
                    theta = (int)(180 - 360 / (float)playerNumber);
                    edgeLength = (int)(vertex - newvertex).Length();

                    spriteBatch.Draw(pixel, vertex, null, Color.Red, 0, Vector2.One, 3f, SpriteEffects.None, 0);
                    for (float j = 0; j < 300; j++)
                    {
                        spriteBatch.Draw(pixel, vertex + (j / 300) * (newvertex - vertex), null, color, 0, Vector2.One, 1f, SpriteEffects.None, 0);
                    }
                }
            foreach (Player player in playerList) if(player != null) player.Draw(spriteBatch, pixel,spriteFont, Width, Height, playerNumber, radius);
            spriteBatch.Draw(pixel, pukLoc, null, Color.Gray, 0, Vector2.One, 7f, SpriteEffects.None, 0);
            spriteBatch.End();
            base.Draw(gameTime);
        }
        private static void LoopConnect()
        {
            while (!_clientSocket.Connected)
            {
                try
                {
                    _clientSocket.Connect(IPAddress.Loopback, 100);
                }
                catch (SocketException)
                { }
            }
            byte[] receivedBuf = new byte[1024];
            int rec = _clientSocket.Receive(receivedBuf);
            byte[] data = new byte[rec];
            Array.Copy(receivedBuf, data, rec);
            clientID = data[0];
        }
        private static void SendLoop()
        {
            byte[] buffer = new byte[dedicatedVarbytes] { clientID, (byte)((int)pukLoc.X / 256), (byte)(pukLoc.X % 256), (byte)((int)pukLoc.Y / 256), (byte)(pukLoc.Y % 256), (byte)(255 * playerList[clientID].location), (byte)(playerList[clientID].score) };
            _clientSocket.Send(buffer);
            byte[] receivedBuf = new byte[1024];
            int rec = _clientSocket.Receive(receivedBuf);
            byte[] data = new byte[rec];
            Array.Copy(receivedBuf, data, rec);
            playerNumber = data[0];
            pukLoc = new Vector2(256 * data[1] + data[2], 256 * data[3] + data[4]);
            for (int i = 5; i < (rec-5) / 3; i++)
            {
                playerList[i] = new Player(data[3 * i], data[3 * i + 1], 1/data[3 * i + 2]);
            }
        }
    }
}