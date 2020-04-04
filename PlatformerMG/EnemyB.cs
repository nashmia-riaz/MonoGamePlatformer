using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TexasJames
{
    enum EnemyState
    {
        Idle = 0,
        Patrolling = 1,
        Chasing = 2,
    }

    class EnemyB : Enemy
    {
        EnemyState enemyState;

        public EnemyB(Level level, Vector2 position, string spriteSet):base(level, position, spriteSet)
        {
            enemyState = EnemyState.Idle;        
        }
        
        /// <summary>
        /// Paces back and forth along a platform, waiting at either end.
        /// </summary>
        public virtual void Update(GameTime gameTime, Vector2 playerPos)
        {
            if (this.FlaggedForRemoval) return;

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate tile position based on the side we are walking towards.
            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / Tile.Height);

            //if player's y position is on the same level as the enemy, set state to chase
            if (playerPos.Y < position.Y+20 && playerPos.Y > position.Y - 20)
            {
                Console.WriteLine("Player is in sight!");
                enemyState = EnemyState.Chasing;
                if (playerPos.X < this.position.X)
                    this.direction = FaceDirection.Left;
                else
                    this.direction = FaceDirection.Right;
            }

            //if in idle state, just stay there
            if(enemyState == EnemyState.Idle)
            {
                waitTime = Math.Max(0.0f, waitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            //if in any other state than idle
            else
            {
                // If we are about to run into a wall or off a cliff, start waiting.
                if (Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable ||
                    Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Passable)
                {
                    waitTime = MaxWaitTime;
                    enemyState = EnemyState.Idle;
                }
                else
                {
                    // Move in the current direction.
                    Vector2 velocity = new Vector2((int)direction * GameInfo.Instance.EnemyInfo.ChaseSpeed * elapsed, 0.0f);
                    position = position + velocity;
                    this.boundingRectangle.X = (int)(position.X);
                    this.boundingRectangle.Y = (int)(position.Y - height );
                }
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
            else if(enemyState == EnemyState.Chasing)
            {
                sprite.PlayAnimation(runAnimation);
            }

            // Draw facing the way the enemy is moving.
            SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sprite.Draw(gameTime, spriteBatch, Position, flip);
        }
    }

}
