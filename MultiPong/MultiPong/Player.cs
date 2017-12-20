using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MultiPong
{
    class Player
    {
        public bool Playing = false;
        public int score = 0;
        public float location;
        private float width;
        private float edge;
        private float rotation;
        private Vector2 vecLocation;

        public Player (int edge, float location, float score)
        {
            this.edge = edge;
            this.location = location;

        }
        public void Update(KeyboardState newK, Camera camera, float playerNumber, float edgeLength)
        {
            if (Playing)
            {
                if (newK.IsKeyDown(Keys.Left))  location += 0.01f;
                if (newK.IsKeyDown(Keys.Right)) location -= 0.01f;
                if (playerNumber%2==1)
                     camera.Rotation += ((float)Math.PI - (edge + 1) * 2 * (float)Math.PI / (playerNumber + 1) - camera.Rotation) / 5;
                else camera.Rotation += ((float)Math.PI - (edge + 0.5f) * 2 * (float)Math.PI / (playerNumber + 1) - camera.Rotation) / 5;
            }
            width = edgeLength / 5;
            if (location > 0.91f) location = 0.91f;
            if (location < 0.09f) location = 0.09f;
        }
        public Vector2 CollisionDetect(Vector2 pukLoc, Vector2 pukVel)
        {
            Vector2 paral = Vector2.Transform(Vector2.UnitY, Matrix.CreateRotationZ(rotation));
            Vector2 perpa = Vector2.Transform( Vector2.UnitX, Matrix.CreateRotationZ(rotation));
            Vector2 top   = vecLocation + paral * 5;
            Vector2 bot   = vecLocation - paral * 5;
            Vector2 left  = vecLocation - perpa * width/2;
            Vector2 right = vecLocation + perpa * width/2;

            if (paral.X * pukLoc.X + paral.Y * pukLoc.Y < paral.X * top.X + paral.Y * top.Y &&
                paral.X * pukLoc.X + paral.Y * pukLoc.Y > paral.X * bot.X + paral.Y * bot.Y &&
                perpa.X * pukLoc.X + perpa.Y * pukLoc.Y > perpa.X * left.X + perpa.Y * left.Y &&
                perpa.X * pukLoc.X + perpa.Y * pukLoc.Y < perpa.X * right.X + perpa.Y * right.Y)
            {
                return (perpa.X * pukVel.X + perpa.Y * pukVel.Y) * perpa - (paral.X * pukVel.X + paral.Y * pukVel.Y) * paral;
            }
            return pukVel;
        }
        public void Draw(SpriteBatch spriteBatch, Texture2D texture,SpriteFont spriteFont, int screenWidth, int screenHeight, int playerNumber, int radius)
        {
            Color textCol = Color.White;
            if (Playing) textCol = Color.Red;
            Vector2 vertex = new Vector2();
            Vector2 newvertex = new Vector2();
            if (playerNumber % 2 == 0)
            {
                vertex = new Vector2(screenWidth, screenHeight) / 2 + Vector2.Transform(-radius * Vector2.UnitY, Matrix.CreateRotationZ(MathHelper.ToRadians(edge * (360 / (float)(playerNumber + 1)))));
                newvertex = new Vector2(screenWidth, screenHeight) / 2 + Vector2.Transform(-radius * Vector2.UnitY, Matrix.CreateRotationZ(MathHelper.ToRadians((edge + 1) * (360 / (float)(playerNumber + 1)))));
            }
            else
            {
                vertex = new Vector2(screenWidth, screenHeight) / 2 + Vector2.Transform(-radius * Vector2.UnitY, Matrix.CreateRotationZ(MathHelper.ToRadians((edge + 0.5f) * (360 / (float)(playerNumber + 1)))));
                newvertex = new Vector2(screenWidth, screenHeight) / 2 + Vector2.Transform(-radius * Vector2.UnitY, Matrix.CreateRotationZ(MathHelper.ToRadians((edge + 1.5f) * (360 / (float)(playerNumber + 1)))));
            }
            rotation = (float)Math.Atan2((newvertex - vertex).Y, (newvertex - vertex).X);
            vecLocation = vertex + location * (newvertex - vertex);
            vecLocation += (vecLocation - new Vector2(screenWidth/2,screenHeight/2))/4;
            spriteBatch.Draw(texture, new Rectangle((int)vecLocation.X,(int)vecLocation.Y,(int)width,5), null, Color.Gray,rotation, Vector2.One, SpriteEffects.None, 0);
            spriteBatch.DrawString(spriteFont, (edge + 1).ToString(), vecLocation, textCol, rotation, spriteFont.MeasureString((edge + 1).ToString()) / 2, 1, SpriteEffects.None, 0);
            spriteBatch.DrawString(spriteFont, score.ToString(), (vertex + newvertex) / 2 + 0.4f * ((vertex + newvertex) / 2 - new Vector2(screenWidth, screenHeight) / 2),
                Color.White, (float)Math.PI + rotation, spriteFont.MeasureString(score.ToString()) / 2, 1f, SpriteEffects.None, 0);
        }
    }
}
