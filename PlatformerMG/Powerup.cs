using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TexasJames
{
    class Powerup:Collidable
    {
        protected Texture2D texture;
        protected Vector2 origin;
        protected Vector2 Position;

        public Level level;

        public virtual void OnCollected() { }

        /// <summary>
        /// Loads the gem texture and collected sound.
        /// </summary>
        public void LoadContent(string texPath)
        {
            texture = level.Content.Load<Texture2D>(texPath);
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
        }

        public Powerup() { }

        public Powerup(Level level, Vector2 pos, string texPath) {
            this.level = level;
            this.Position = pos;

            this.boundingRectangle.Width = Tile.Width;
            this.boundingRectangle.Height = Tile.Height;
            this.boundingRectangle.X = (int)(pos.X - Tile.Width / 2);
            this.boundingRectangle.Y = (int)(pos.Y - Tile.Height / 2);

            LoadContent(texPath);
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }
    }
}
