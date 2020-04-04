#region File Description
//-----------------------------------------------------------------------------
// Enemy.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TexasJames
{
    /// <summary>
    /// Facing direction along the X axis.
    /// </summary>
    enum FaceDirection
    {
        Left = -1,
        Right = 1,
    }

    /// <summary>
    /// A monster who is impeding the progress of our fearless adventurer.
    /// </summary>
    class Enemy:Collidable
    {

        public Enemy() { }

        public Level Level
        {
            get { return level; }
        }
        Level level;

        /// <summary>
        /// Position in world space of the bottom center of this enemy.
        /// </summary>
        public Vector2 Position
        {
            set { position = value; }
            get { return position; }
        }
        protected Vector2 position;
        
        protected Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this enemy in world space.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        // Animations
        protected Animation runAnimation;
        protected Animation idleAnimation;
        protected AnimationPlayer sprite;

        /// <summary>
        /// The direction this enemy is facing and moving along the X axis.
        /// </summary>
        protected FaceDirection direction = FaceDirection.Left;

        /// <summary>
        /// How long this enemy has been waiting before turning around.
        /// </summary>
        protected float waitTime;

        /// <summary>
        /// How long to wait before turning around.
        /// </summary>
        protected const float MaxWaitTime = 0.5f;

        /// <summary>
        /// The speed at which this enemy moves along the X axis.
        /// </summary>
        protected const float MoveSpeed = 64.0f;
        
        protected float width = 64;
        protected float height = 64;

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public Enemy(Level level, Vector2 position, string spriteSet)
        {
            this.level = level;
            this.position = position;

            LoadContent(spriteSet);

            this.boundingRectangle.X = (int)(position.X - width / 2);
            this.boundingRectangle.Y = (int)(position.Y - height / 2);
        }

        /// <summary>
        /// Loads a particular enemy sprite sheet and sounds.
        /// </summary>
        public void LoadContent(string spriteSet)
        {
            this.boundingRectangle.Width = (int)width;
            this.boundingRectangle.Height = (int)height;

            // Load animations.
            spriteSet = "Sprites/" + spriteSet + "/";
            runAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Run"), 0.1f, true);
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Idle"), 0.15f, true);
            sprite.PlayAnimation(idleAnimation);

            // Calculate bounds within texture size.
            int newWidth = (int)(idleAnimation.FrameWidth);
            int newHeight = (int)(idleAnimation.FrameHeight);
            int left = (idleAnimation.FrameWidth - newWidth) / 2;
            int top = idleAnimation.FrameHeight - newHeight;
            localBounds = new Rectangle(left, top, newWidth, newHeight);

        }
        
        /// <summary>
        /// Paces back and forth along a platform, waiting at either end.
        /// </summary>
        public virtual void Update(GameTime gameTime)
        {
            if (this.FlaggedForRemoval) return;

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate tile position based on the side we are walking towards.
            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / Tile.Height);

            if (waitTime > 0)
            {
                // Wait for some amount of time.
                waitTime = Math.Max(0.0f, waitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                if (waitTime <= 0.0f)
                {
                    // Then turn around.
                    direction = (FaceDirection)(-(int)direction);
                }
            }
            else
            {
                // If we are about to run into a wall or off a cliff, start waiting.
                if (Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable ||
                    Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Passable)
                {
                    waitTime = MaxWaitTime;
                }
                else
                {
                    // Move in the current direction.
                    Vector2 velocity = new Vector2((int)direction * MoveSpeed * elapsed, 0.0f);
                    position = position + velocity;
                    this.boundingRectangle.X = (int)(position.X - width/2);
                    this.boundingRectangle.Y = (int)(position.Y - height/2);
                }
            }
        }

        /// <summary>
        /// Draws the animated enemy.
        /// </summary>
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (this.FlaggedForRemoval) return;

            // Stop running when the game is paused or before turning around.
            if (!Level.Player.IsAlive ||
                Level.ReachedExit ||
                Level.TimeRemaining == TimeSpan.Zero ||
                waitTime > 0)
            {
                sprite.PlayAnimation(idleAnimation);
            }
            else
            {
                sprite.PlayAnimation(runAnimation);
            }


            // Draw facing the way the enemy is moving.
            SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sprite.Draw(gameTime, spriteBatch, Position, flip);
        }

        public override bool CollisionTest(Collidable obj)
        {
            if (obj != null)
            {
                return boundingRectangle.Intersects(obj.BoundingRectangle);
            }

            return false;
        }

        public override void OnCollision(Collidable obj)
        {
            if (obj == null) return;
            if (obj.FlaggedForRemoval || this.FlaggedForRemoval) return;

            if (obj as Gem != null) return;

            Console.WriteLine("Enemy collided with "+obj);
            Bullet bullet = obj as Bullet;

            if(bullet != null)
            {
                bullet.FlaggedForRemoval = true;
                this.FlaggedForRemoval = true;
                level.RemoveBullet(bullet);
                level.RemoveEnemy(this);
                Console.WriteLine(bullet + " and " + this + " collided");
                //level.remo
            }
            
        }
    }
}
