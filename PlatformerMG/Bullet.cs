using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TexasJames
{
    class Bullet:Collidable
    {
        float width = 10;
        float height = 10;
        float speed = 500.0f;

        public Vector2 position = new Vector2(0.0f, 0.0f);

        float direction = 0.0f;
        
        public Level Level
        {
            get { return level; }
        }
        Level level;

        private Texture2D texture;
        private Vector2 origin;

        public void LoadContent()
        {
            texture = Level.Content.Load<Texture2D>("Sprites/Bullet");
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
        }

        public Bullet(Level level, Vector2 pos, float dir)
        {
            this.level = level;
            position = pos;
            position.Y -= 25;
            direction = dir;

            this.boundingRectangle.Width = (int)width;
            this.boundingRectangle.Height = (int)height;
            this.boundingRectangle.X = (int)(position.X - width / 2);
            this.boundingRectangle.Y = (int)(position.Y - height / 2);

            LoadContent();
        }
        public void Update(GameTime gameTime)
        {
            if (this.FlaggedForRemoval) return;
            this.position.X += direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            this.boundingRectangle.X = (int)(position.X - width / 2);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (this.FlaggedForRemoval) return;
            spriteBatch.Draw(texture, position, null, GameInfo.Instance.GemInfo.Color, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }
    }
}
