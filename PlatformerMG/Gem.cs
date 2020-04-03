﻿#region File Description
//-----------------------------------------------------------------------------
// Gem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace TexasJames
{
    /// <summary>
    /// A valuable item the player can collect.
    /// </summary>
    class Gem : Collidable
    {
        private Texture2D texture;
        private Vector2 origin;
        private SoundEffect collectedSound;

        public const int PointValue = 30;
        public readonly Color Color = Color.Yellow;
        public bool wasCollected = false;

        private Rectangle localBounds;

        // The gem is animated from a base position along the Y axis.
        private Vector2 basePosition;
        private float bounce;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        /// <summary>
        /// Gets the current position of this gem in world space.
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return basePosition + new Vector2(0.0f, bounce);
            }
        }

        /// <summary>
        /// Gets a circle which bounds this gem in world space.
        /// </summary>
        //public Circle BoundingCircle
        //{
        //    get
        //    {
        //        return new Circle(Position, Tile.Width / 3.0f);
        //    }
        //}

        /// <summary>
        /// Constructs a new gem.
        /// </summary>
        public Gem(Level level, Vector2 position)
        {
            this.level = level;
            this.basePosition = position;

            int left = (int)Math.Round(Position.X - localBounds.Width/2);
            int top = (int)Math.Round(Position.Y - localBounds.Height/2);

            this.boundingRectangle = new Rectangle(left, top, localBounds.Width, localBounds.Height);
            
            //this.boundingRectangle.Center = position;
            //this.boundingRectangle.Radius = Tile.Width / 3.0f;
            //Console.WriteLine("Gem's radius is " + this.boundingRectangle.Radius);

            LoadContent();
        }

        /// <summary>
        /// Loads the gem texture and collected sound.
        /// </summary>
        public void LoadContent()
        {
            texture = Level.Content.Load<Texture2D>("Sprites/Gem");
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
            collectedSound = Level.Content.Load<SoundEffect>("Sounds/GemCollected");
            
            int width = (int) texture.Width;
            int left = (int) Position.X - width / 2;
            int height = (int) texture.Height;
            int top = (int) Position.Y - width /2;
            localBounds = new Rectangle(left, top, width, height);
        }

        /// <summary>
        /// Bounces up and down in the air to entice players to collect them.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Bounce control constants
            const float BounceHeight = 0.18f;
            const float BounceRate = 3.0f;
            const float BounceSync = -0.75f;

            // Bounce along a sine curve over time.
            // Include the X coordinate so that neighboring gems bounce in a nice wave pattern.            
            double t = gameTime.TotalGameTime.TotalSeconds * BounceRate + Position.X * BounceSync;
            bounce = (float)Math.Sin(t) * BounceHeight * texture.Height;
        }

        /// <summary>
        /// Called when this gem has been collected by a player and removed from the level.
        /// </summary>
        /// <param name="collectedBy">
        /// The player who collected this gem. Although currently not used, this parameter would be
        /// useful for creating special powerup gems. For example, a gem could make the player invincible.
        /// </param>
        public void OnCollected(Player collectedBy)
        {
            //collectedSound.Play();
        }

        /// <summary>
        /// Draws a gem in the appropriate color.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (wasCollected) return;
            spriteBatch.Draw(texture, Position, null, Color, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
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
            //if (wasCollected) return;

            //Player player = obj as Player;
            //if (player != null)
            //{
            //    Console.WriteLine("Gem collided with " + obj);
            //    collectedSound.Play();
            //    wasCollected = true;
            //}
        }
    }
}
