#region File Description
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
    class Gem : Powerup
    {
        public const int PointValue = 30;
        public readonly Color Color = Color.Yellow;
        public bool wasCollected = false;

        // The gem is animated from a base position along the Y axis.
        private Vector2 basePosition;
        private float bounce;
        
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
        /// Constructs a new gem.
        /// </summary>
        public Gem(Level level, Vector2 position):base(level, position, "Sprites/Gem")
        {
            this.basePosition = position;
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
        /// Draws a gem in the appropriate color.
        /// </summary>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, GameInfo.Instance.GemInfo.Color, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }

        public override void OnCollected()
        {
            Console.WriteLine(this + " collected by player");
            level.OnGemCollected(this);
            this.FlaggedForRemoval = true;
        }
    }
}
