#region File Description
//-----------------------------------------------------------------------------
// Level.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;

namespace TexasJames
{
    /// <summary>
    /// A uniform grid of tiles with collections of gems and enemies.
    /// The level owns the player and controls the game's win and lose
    /// conditions as well as scoring.
    /// </summary>
    class Level : IDisposable
    {
        // Physical structure of the level.
        private Tile[,] tiles;
        private Texture2D[] layers;
        // The layer which entities are drawn on top of.
        private const int EntityLayer = 2;

        // Entities in the level.
        public Player Player
        {
            get { return player; }
        }
        Player player;

        private List<Gem> gems = new List<Gem>();
        private List<Enemy> enemies = new List<Enemy>();
        private List<EnemyB> enemiesB = new List<EnemyB>();
        private List<Bullet> bullets = new List<Bullet>();
        private List<Ammo> ammos = new List<Ammo>();

        // Key locations in the level.        
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);

        private CollisionManager collisionManager;
        private SoundManager soundManager;

        private int ammoCount = 10;

        // Level game state.
        private Random random = new Random(354668); // Arbitrary, but constant seed

        public int Score
        {
            get { return score; }
        }
        int score;

        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        public bool CollidingExit
        {
            get { return collidingExit; }
        }
        bool collidingExit;

        public bool HasGameEnded
        {
            get { return hasGameEnded; }
        }
        bool hasGameEnded;

        public TimeSpan TimeRemaining
        {
            get { return timeRemaining; }
        }
        TimeSpan timeRemaining;

        private const int PointsPerSecond = 5;

        // Level content.        
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        public Loader Loader
        {
            get { return loader; }
        }
        Loader loader;

        #region Loading

        /// <summary>
        /// Constructs a new level.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider that will be used to construct a ContentManager.
        /// </param>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex)
        {
            hasGameEnded = false;

            loader = new Loader(fileStream);
            loader.ReadXML("Content/Info.xml");

            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");
            collisionManager = new CollisionManager();

            timeRemaining = TimeSpan.FromMinutes(2.0);

            LoadTiles(fileStream);

            // Load background layer textures. For now, all levels must
            // use the same backgrounds and only use the left-most part of them.
            //layers = new Texture2D[3];
            //for (int i = 0; i < layers.Length; ++i)
            //{
            //    // Choose a random segment if each background layer for level variety.
            //    int segmentIndex = levelIndex;
            //    layers[i] = Content.Load<Texture2D>("Backgrounds/Layer" + i + "_" + segmentIndex);
            //}
            layers = new Texture2D[1];
            layers[0] = Content.Load<Texture2D>("Backgrounds/CaveBackground");

            // Load sounds.
            soundManager = new SoundManager(content);
        }

        /// <summary>
        /// Iterates over every tile in the structure file and loads its
        /// appearance and behavior. This method also validates that the
        /// file is well-formed with a player start point, exit, etc.
        /// </summary>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        private void LoadTiles(Stream fileStream)
        {
            // Load the level and ensure all of the lines are the same length.
            int width;
            List<string> lines;// = new List<string>();

            lines = loader.ReadLinesFromTextFile();
            width = lines[0].Length;

            // Allocate the tile grid.
            tiles = new Tile[width, lines.Count];

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // to load each tile.
                    char tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }

            // Verify that the level has a beginning and an end.
            if (Player == null)
                throw new NotSupportedException("A level must have a starting point.");
            if (exit == InvalidPosition)
                throw new NotSupportedException("A level must have an exit.");
        }

        /// <summary>
        /// Loads an individual tile's appearance and behavior.
        /// </summary>
        /// <param name="tileType">
        /// The character loaded from the structure file which
        /// indicates what should be loaded.
        /// </param>
        /// <param name="x">
        /// The X location of this tile in tile space.
        /// </param>
        /// <param name="y">
        /// The Y location of this tile in tile space.
        /// </param>
        /// <returns>The loaded tile.</returns>
        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                // Blank space
                case '.':
                    return new Tile(null, TileCollision.Passable);

                // Exit
                case 'X':
                    return LoadExitTile(x, y);

                // Gem
                case 'G':
                    return LoadGemTile(x, y);

                case 'Q':
                    return LoadAmmoTile(x, y);

                // Floating platform
                case '-':
                    return LoadTile("Platform", TileCollision.Platform);

                // Various enemies
                case 'A':
                    return LoadEnemyTile(x, y, "MonsterA");
                case 'B':
                    return LoadEnemyTile(x, y, "MonsterB");
                case 'C':
                    return LoadEnemyTile(x, y, "MonsterC");
                case 'D':
                    return LoadEnemyTile(x, y, "MonsterD");

                // Platform block
                case '~':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Platform);

                // Passable block
                case ':':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Passable);

                // Player 1 start point
                case '1':
                    return LoadStartTile(x, y);

                // Impassable block
                case '#':
                    return LoadVarietyTile("BlockA", 7, TileCollision.Impassable);

                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        /// <summary>
        /// Creates a new tile. The other tile loading methods typically chain to this
        /// method after performing their special logic.
        /// </summary>
        /// <param name="name">
        /// Path to a tile texture relative to the Content/Tiles directory.
        /// </param>
        /// <param name="collision">
        /// The tile collision type for the new tile.
        /// </param>
        /// <returns>The new tile.</returns>
        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }


        /// <summary>
        /// Loads a tile with a random appearance.
        /// </summary>
        /// <param name="baseName">
        /// The content name prefix for this group of tile variations. Tile groups are
        /// name LikeThis0.png and LikeThis1.png and LikeThis2.png.
        /// </param>
        /// <param name="variationCount">
        /// The number of variations in this group.
        /// </param>
        private Tile LoadVarietyTile(string baseName, int variationCount, TileCollision collision)
        {
            int index = random.Next(variationCount);
            return LoadTile(baseName + index, collision);
        }


        /// <summary>
        /// Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private Tile LoadStartTile(int x, int y)
        {
            if (Player != null)
                throw new NotSupportedException("A level may only have one starting point.");

            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            player = new Player(this, start);
            collisionManager.AddCollidable(player);

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Remembers the location of the level's exit.
        /// </summary>
        private Tile LoadExitTile(int x, int y)
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            exit = GetBounds(x, y).Center;
            Tile exitTile = LoadTile("Exit", TileCollision.Exit);
            exitTile.boundingRectangle.X = (int)exit.X;
            exitTile.boundingRectangle.Y = (int)exit.Y;
            exitTile.boundingRectangle.Width = Tile.Width;
            exitTile.boundingRectangle.Height = Tile.Height;
            
            collisionManager.AddCollidable(exitTile);
            return exitTile;
        }

        /// <summary>
        /// Instantiates an enemy and puts him in the level.
        /// </summary>
        private Tile LoadEnemyTile(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            Console.WriteLine("Spawned enemy at " + position);
            Enemy enemy;
            if (spriteSet == "MonsterB")
            {
                enemy = new EnemyB(this, position, spriteSet);
                enemiesB.Add(enemy as EnemyB);
            }
            else
            {
                enemy = new Enemy(this, position, spriteSet);
                enemies.Add(enemy);
            }


            collisionManager.AddCollidable(enemy);

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Instantiates a gem and puts it in the level.
        /// </summary>
        private Tile LoadGemTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            Gem newGem = new Gem(this, new Vector2(position.X, position.Y));
            gems.Add(newGem);
            collisionManager.AddCollidable(newGem);

            return new Tile(null, TileCollision.Passable);
        }

        public Tile LoadAmmoTile(int x, int y)
        {
            Console.WriteLine("Loaded ammo");
            Point point = GetBounds(x, y).Center;
            Ammo newAmmo = new Ammo(this, new Vector2(point.X, point.Y), "Sprites/Ammo");
            ammos.Add(newAmmo);
            collisionManager.AddCollidable(newAmmo);

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Unloads the level content.
        /// </summary>
        public void Dispose()
        {
            Content.Unload();
        }

        #endregion

        #region Bounds and collision

        /// <summary>
        /// Gets the collision mode of the tile at a particular location.
        /// This method handles tiles outside of the levels boundries by making it
        /// impossible to escape past the left or right edges, but allowing things
        /// to jump beyond the top of the level and fall off the bottom.
        /// </summary>
        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        /// <summary>
        /// Gets the bounding rectangle of a tile in world space.
        /// </summary>        
        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        /// <summary>
        /// Width of level measured in tiles.
        /// </summary>
        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        /// <summary>
        /// Height of the level measured in tiles.
        /// </summary>
        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates all objects in the world, performs collision between them,
        /// and handles the time limit with scoring.
        /// </summary>
        public void Update(
            GameTime gameTime, 
            DisplayOrientation orientation)
        {
            collisionManager.Update();

            // Pause while the player is dead or time is expired.
            if (!Player.IsAlive || TimeRemaining == TimeSpan.Zero)
            {
                // Still want to perform physics on the player.
                Player.ApplyPhysics(gameTime);
            }
            else if (ReachedExit)
            {
                // Animate the time being converted into points.
                int seconds = (int)Math.Round(gameTime.ElapsedGameTime.TotalSeconds * 100.0f);
                seconds = Math.Min(seconds, (int)Math.Ceiling(TimeRemaining.TotalSeconds));
                timeRemaining -= TimeSpan.FromSeconds(seconds);
                score += seconds * PointsPerSecond;
            }
            else
            {
                timeRemaining -= gameTime.ElapsedGameTime;
                player.Update(gameTime);
                UpdateGems(gameTime);

                // Falling off the bottom of the level kills the player.
                if (Player.oldBoundingRectangle.Top >= Height * Tile.Height)
                    //OnPlayerKilled(null);
                    player.OnKilled(null);

                UpdateEnemies(gameTime);

                UpdateBullets(gameTime);

                // The player has reached the exit if they are standing on the ground and
                // his bounding rectangle contains the center of the exit tile. They can only
                // exit when they have collected all of the gems.
                if (Player.IsAlive &&
                    Player.IsOnGround &&
                    Player.oldBoundingRectangle.Contains(exit))
                {
                    //OnExitReached();
                }
            }

            // Clamp the time remaining at zero.
            if (timeRemaining < TimeSpan.Zero)
                timeRemaining = TimeSpan.Zero;
        }

        public void MovePlayerLeft()
        {
            player.MovePlayerLeft();
        }

        public void MovePlayerRight()
        {
            player.MovePlayerRight();
        }

        public void PlayerJumps()
        {
            player.PlayerJumps();
            if (player.didPlayerJustJump) 
                soundManager.PlaySound("PlayerJump");
        }

        public void MakePlayerShoot()
        {
            if (ammoCount <= 0) return;
            ammoCount--;
            Bullet bullet = new Bullet(this, player.Position, player.direction);
            bullets.Add(bullet);
            collisionManager.AddCollidable(bullet);
        }

        public void AmmoCollected(Ammo ammo)
        {
            ammoCount+=5;
            soundManager.PlaySound("GemCollected");

            RemoveAmmo(ammo);
        }

        public void PlayerAttack()
        {
            player.PlayerAttack();
        }

        public void RemoveBullet(Bullet bullet)
        {
            bullets.Remove(bullet);
        }

        public void RemoveEnemy(Enemy enemy)
        {
            enemies.Remove(enemy);
            soundManager.PlaySound("MonsterKilled");
        }

        public void RemoveGem(Gem gem)
        {
            gems.Remove(gem);
        }

        public void RemoveAmmo(Ammo ammo)
        {
            ammos.Remove(ammo);
        }

        private void UpdateBullets(GameTime gameTime)
        {
            foreach(Bullet bullet in bullets)
            {
                bullet.Update(gameTime);
            }
        }
        /// <summary>
        /// Animates each gem and checks to allows the player to collect them.
        /// </summary>
        private void UpdateGems(GameTime gameTime)
        {
            for (int i = 0; i < gems.Count; ++i)
            {
                Gem gem = gems[i];

                gem.Update(gameTime);

                //if (gem.BoundingCircle.Intersects(Player.BoundingRectangle))
                //{
                //    gems.RemoveAt(i--);
                //    OnGemCollected(gem, Player);
                //}
            }
        }

        /// <summary>
        /// Animates each enemy and allow them to kill the player.
        /// </summary>
        private void UpdateEnemies(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies)
            {
                enemy.Update(gameTime);
            }

            foreach (EnemyB enemy in enemiesB)
            {
                if(enemy!=null)
                    enemy.Update(gameTime, player.Position);
            }
        }

        /// <summary>
        /// Called when a gem is collected.
        /// </summary>
        public void OnGemCollected(Gem gem)
        {
            score += Gem.PointValue;
            soundManager.PlaySound("GemCollected");

            RemoveGem(gem);
        }
        
        /// <summary>
        /// Called when the player is killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This is null if the player was not killed by an
        /// enemy, such as when a player falls into a hole.
        /// </param>
        public void OnPlayerKilled(Enemy killedBy)
        {
            if (killedBy != null)
                soundManager.PlaySound("PlayerFall");
            else
                soundManager.PlaySound("PlayerKilled");

            hasGameEnded = true;
        }
        
        /// <summary>
        /// Called when the player reaches the level's exit.
        /// </summary>
        public void OnExitReached()
        {
            if (!collidingExit || ReachedExit) return;

            Player.OnReachedExit();
            soundManager.PlaySound("ExitReached");
            reachedExit = true;
            hasGameEnded = true;

            if (score > GameInfo.Instance.Highscore)
            {
                GameInfo.Instance.Highscore = score;
                loader.WriteXML("Content/Info.xml");
            }
        }

        public void OnExitColliding()
        {
            if (collidingExit) return;

            collidingExit = true;
            Console.WriteLine("Walked into exit tile");
        }

        public void OnOutOfExit()
        {
            if (!collidingExit) return;

            Console.WriteLine("Walked out of exit tile");
            collidingExit = false;
        }
        
        /// <summary>
        /// Restores the player to the starting point to try the level again.
        /// </summary>
        public void StartNewLife()
        {
            hasGameEnded = false;
            Player.Reset(start);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw everything in the level from background to foreground.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            for (int i = 0; i <= 0; ++i)
                spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);

            DrawTiles(spriteBatch);

            foreach (Gem gem in gems)
                gem.Draw(gameTime, spriteBatch);

            Player.Draw(gameTime, spriteBatch);

            foreach (Enemy enemy in enemies)
                enemy.Draw(gameTime, spriteBatch);

            foreach (EnemyB enemy in enemiesB)
                enemy.Draw(gameTime, spriteBatch);

            //for (int i = EntityLayer + 1; i < layers.Length; ++i)
            //    spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);

            foreach (Bullet bullet in bullets)
                bullet.Draw(gameTime, spriteBatch);

            foreach (Ammo ammo in ammos)
                ammo.Draw(gameTime, spriteBatch);
        }

        /// <summary>
        /// Draws each tile in the level.
        /// </summary>
        private void DrawTiles(SpriteBatch spriteBatch)
        {
            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // If there is a visible tile in that position
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }

        #endregion
    }
}
