using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TexasJames
{
    /// <summary>
    /// This type of enemy patrols the platform and will chase player if in sight
    /// </summary>
    class EnemyC : EnemyB
    {
        EnemyState enemyState;

        public EnemyC(Level level, Vector2 position, string spriteSet) : base(level, position, spriteSet)
        {
            enemyState = EnemyState.Patrolling;
        }

        /// <summary>
        /// Paces back and forth along a platform, waiting at either end.
        /// </summary>
        public override void Update(GameTime gameTime, Vector2 playerPos)
        {
            if (this.FlaggedForRemoval) return;

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate tile position based on the side we are walking towards.
            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / Tile.Height);

            //if player's y position is on the same level as the enemy, set state to chase
            if (playerPos.Y < position.Y + 20 && playerPos.Y > position.Y - 20)
            {
                Console.WriteLine("Player is in sight!");
                enemyState = EnemyState.Chasing;
                if (playerPos.X < this.position.X)
                    this.direction = FaceDirection.Left;
                else
                    this.direction = FaceDirection.Right;
            }

            //if in idle state, just stay there
            if (enemyState == EnemyState.Idle)
            {
                waitTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                Console.WriteLine("Enemy has been waiting " + waitTime+" while max wait time is "+MaxWaitTime);
                //after waiting, resume movement
                if (waitTime >= MaxWaitTime)
                {
                    waitTime = Math.Max(0.0f, waitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                    enemyState = EnemyState.Patrolling;
                    direction = (FaceDirection)(-(int)direction);
                }
            }
            
            // If we are about to run into a wall or off a cliff, start waiting.
            if ((Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable ||
                Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Passable)
                && enemyState != EnemyState.Idle)
            {
                waitTime = 0;
                enemyState = EnemyState.Idle;
            }
            //if enemy is patrolling, move into the direction at normal speed
            else if (enemyState == EnemyState.Patrolling)
            {
                // Move in the current direction.
                Vector2 velocity = new Vector2((int)direction * GameInfo.Instance.EnemyInfo.Speed * elapsed, 0.0f);
                position = position + velocity;
                this.boundingRectangle.X = (int)(position.X);
                this.boundingRectangle.Y = (int)(position.Y - height);
            }
            //if enemy is chasing, move towards player (direction set after seeing player) at chase speed
            else if (enemyState == EnemyState.Chasing)
            {
                // Move in the current direction.
                Vector2 velocity = new Vector2((int)direction * GameInfo.Instance.EnemyInfo.ChaseSpeed * elapsed, 0.0f);
                position = position + velocity;
                this.boundingRectangle.X = (int)(position.X);
                this.boundingRectangle.Y = (int)(position.Y - height);
            }
        }

        /// <summary>
        /// Draws the animated enemy.
        /// </summary>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (this.FlaggedForRemoval) return;

            // Stop running when the game is paused or before turning around.
            if (enemyState == EnemyState.Idle)
            {
                sprite.PlayAnimation(idleAnimation);
            }
            else if (enemyState == EnemyState.Chasing || enemyState == EnemyState.Patrolling)
            {
                sprite.PlayAnimation(runAnimation);
            }

            // Draw facing the way the enemy is moving.
            SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sprite.Draw(gameTime, spriteBatch, Position, flip);
        }
    }
}
