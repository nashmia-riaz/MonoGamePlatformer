#region File Description
//-----------------------------------------------------------------------------
// PlatformerGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;


namespace TexasJames
{
    enum GameState
    {
        TitleScreen = 1,
        InGame = 2,
    }
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PlatformerGame : Microsoft.Xna.Framework.Game
    {
        // Resources for drawing.
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // Global content.
        private SpriteFont hudFont;

        private Texture2D titleScreen;
        private Texture2D winOverlay;
        private Texture2D loseOverlay;
        private Texture2D diedOverlay;
        private Texture2D GameOverScreen;

        // Meta-level game state.
        private int levelIndex = -1;
        private Level level;
        private bool wasContinuePressed;

        private GameState currentState = GameState.TitleScreen;

        private CommandManager commandManager;

        // When the time remaining is less than the warning time, it blinks on the hud
        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

        // The number of levels in the Levels directory of our content. We assume that
        // levels in our content are 0-based and that all numbers under this constant
        // have a level file present. This allows us to not need to check for the file
        // or handle exceptions, both of which can add unnecessary time to level loading.
        private const int numberOfLevels = 3;

        private bool hasGameStarted = false;

        public PlatformerGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

                #if WINDOWS_PHONE
                graphics.IsFullScreen = true;
                TargetElapsedTime = TimeSpan.FromTicks(333333);
                #endif

            currentState = GameState.TitleScreen;
            commandManager = new CommandManager();
            Accelerometer.Initialize();
            InitializeBindings();
        }

        #region keybindings

        private void InitializeBindings()
        {
            commandManager.AddKeyboardBinding(Keys.Enter, StartGame);
            commandManager.AddKeyboardBinding(Keys.Escape, ExitGame);
            commandManager.AddKeyboardBinding(Keys.Left, MovePlayerToLeft);
            commandManager.AddKeyboardBinding(Keys.Right, MovePlayerToRight);
            commandManager.AddKeyboardBinding(Keys.Up, MakePlayerJump);
            commandManager.AddKeyboardBinding(Keys.Space, MakePlayerAttack);
            commandManager.AddKeyboardBinding(Keys.Z, ProceedToNextArea);
            commandManager.AddKeyboardBinding(Keys.X, MakePlayerShoot);
            commandManager.AddKeyboardBinding(Keys.R, ResetGameStats);
        }

        /// <summary>
        /// Reset all info.xml stats so we can start from beginning
        /// </summary>
        /// <param name="buttonState"></param>
        /// <param name="amount"></param>
        private void ResetGameStats(eButtonState buttonState, Vector2 amount)
        {
            GameInfo.Instance.Highscore = 0;
            level.ResetStats();
        }

        private void MakePlayerShoot(eButtonState buttonState, Vector2 amount)
        {
            if(buttonState == eButtonState.PRESSED)
            {
                level.MakePlayerShoot();
            }
        }

        private void ExitGame(eButtonState buttonState, Vector2 amount)
        {
            if (buttonState == eButtonState.DOWN)
            {
                level.SaveGame();
                Exit();
            }
        }

        private void StartGame(eButtonState buttonState, Vector2 amount)
        {
            if (!hasGameStarted && buttonState == eButtonState.DOWN)
            {
                currentState = GameState.InGame;
            }

            LoadNextLevel(0);

            commandManager.UpdateKeyboardBinding(Keys.Enter, ContinueGame);
        }

        private void MovePlayerToLeft(eButtonState buttonState, Vector2 amount) {
            if (buttonState == eButtonState.DOWN)
            {
                level.MovePlayerLeft();
            }
        }

        private void MovePlayerToRight(eButtonState buttonState, Vector2 amount) {
            if(buttonState == eButtonState.DOWN) {
                level.MovePlayerRight();
            }
        }
        
        private void MakePlayerJump(eButtonState buttonState, Vector2 amount) {
            if(buttonState == eButtonState.DOWN)
            {
                level.PlayerJumps();
            }
        }

        private void MakePlayerAttack(eButtonState buttonState, Vector2 amount)
        {
            if (buttonState == eButtonState.DOWN)
            {
                level.PlayerAttack();
            }
        }

        private void ProceedToNextArea(eButtonState buttonState, Vector2 amount)
        {
            if (buttonState == eButtonState.DOWN)
            {
                levelIndex = level.LevelToLoad;
                if (levelIndex < 0) return;

                if (level.CollidingExit && GameInfo.Instance.WasKeyCollected)
                    level.OnExitReached();
                else if (!level.CollidingExit)
                    LoadNextLevel(levelIndex);
            }
        }

        private void ContinueGame(eButtonState buttonState, Vector2 amount)
        {
            // Perform the appropriate action to advance the game and
            // to get the player back to playing.
            if (buttonState == eButtonState.DOWN)
            {
                if (!level.Player.IsAlive)
                {
                    level.StartNewLife();
                }
                else if (level.TimeRemaining == TimeSpan.Zero)
                {
                    if (level.ReachedExit)
                    {
                        levelIndex = level.LevelToLoad;
                        LoadNextLevel(levelIndex);
                    }
                    else
                        ReloadCurrentLevel();
                }
            }

            wasContinuePressed = buttonState == eButtonState.DOWN;
        }

        #endregion
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load fonts
            hudFont = Content.Load<SpriteFont>("Fonts/gameFont");

            // Load overlay textures
            winOverlay = Content.Load<Texture2D>("Overlays/you_win");
            loseOverlay = Content.Load<Texture2D>("Overlays/you_lose");
            diedOverlay = Content.Load<Texture2D>("Overlays/you_died");
            titleScreen = Content.Load<Texture2D>("Overlays/Title Screen");
            GameOverScreen = Content.Load<Texture2D>("Overlays/Game Over-01");

            //Known issue that you get exceptions if you use Media PLayer while connected to your PC
            //See http://social.msdn.microsoft.com/Forums/en/windowsphone7series/thread/c8a243d2-d360-46b1-96bd-62b1ef268c66
            //Which means its impossible to test this from VS.
            //So we have to catch the exception and throw it away
            try
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(Content.Load<Song>("Sounds/Music"));
            }
            catch { }

            levelIndex = 0;
            LoadNextLevel(levelIndex);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            commandManager.Update();

            // update our level, passing down the GameTime along with all of our input states
            level.Update(gameTime, Window.CurrentOrientation);

            base.Update(gameTime);
        }

        private void LoadNextLevel(int newLevelIndex)
        {
            // move to the next level
            //levelIndex = (levelIndex + 1) % numberOfLevels;
            levelIndex = newLevelIndex;

            // Unloads the content for the current level before loading the next one.
            if (level != null)
                level.Dispose();

            // Load the level.
            string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                level = new Level(Services, fileStream, levelIndex);
        }

        private void ReloadCurrentLevel()
        {
            //--levelIndex;
            LoadNextLevel(levelIndex);
        }

        /// <summary>
        /// Draws the game from background to foreground.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            level.Draw(gameTime, spriteBatch);

            DrawHud();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawHud()
        {
            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                                         titleSafeArea.Y + titleSafeArea.Height / 2.0f);

            if (currentState == GameState.TitleScreen)
            {
                // Draw status message.
                Rectangle destinationRectangle = new Rectangle(0, 0, titleSafeArea.Width, titleSafeArea.Height);
                Vector2 statusSize = new Vector2(titleSafeArea.Width, titleSafeArea.Height);
                spriteBatch.Draw(titleScreen, destinationRectangle, Color.White);
                return;
            }

            // Draw time remaining. Uses modulo division to cause blinking when the
            // player is running out of time.
            string timeString = "TIME: " + level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00");
            Color timeColor;
            if (level.TimeRemaining > WarningTime ||
                level.ReachedExit ||
                (int)level.TimeRemaining.TotalSeconds % 2 == 0)
            {
                timeColor = Color.Yellow;
            }
            else
            {
                timeColor = Color.Red;
            }
            DrawShadowedString(hudFont, timeString, hudLocation, timeColor);

            // Draw score
            float timeHeight = hudFont.MeasureString(timeString).Y;
            DrawShadowedString(hudFont, "SCORE: " + level.Score.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Yellow);

            DrawShadowedString(hudFont, "BULLETS: " + GameInfo.Instance.NumberOfBullets.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 2.4f), Color.Yellow);

            DrawShadowedString(hudFont, "HIGHSCORE: " + GameInfo.Instance.Highscore.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 3.6f), Color.Yellow);
            // Determine the status overlay message to show.
            Texture2D status = null;
            if (level.TimeRemaining == TimeSpan.Zero)
            {
                if (level.ReachedExit)
                {
                    status = GameOverScreen;
                }
                else
                {
                    status = loseOverlay;
                }
            }
            else if (!level.Player.IsAlive)
            {
                status = diedOverlay;
            }

            if (status != null)
            {
                if(status == GameOverScreen)
                {
                    Rectangle destinationRectangle = new Rectangle(0, 0, titleSafeArea.Width, titleSafeArea.Height);
                    Vector2 statusSize = new Vector2(titleSafeArea.Width, titleSafeArea.Height);
                    spriteBatch.Draw(GameOverScreen, destinationRectangle, Color.White);
                    //DrawShadowedString(hudFont, "YOU DID IT!", hudLocation + new Vector2(0.0f, timeHeight * 2.4f), Color.Yellow);
                    //spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
                    spriteBatch.DrawString(hudFont, "YOU DID IT! YOU REACHED THE TREASURE!", hudLocation + new Vector2(10, 10), Color.Yellow, 0, new Vector2(0, 0), 2, SpriteEffects.None, 0);
                    spriteBatch.DrawString(hudFont, "Your score is: "+level.Score.ToString(), hudLocation + new Vector2(10, 60), Color.Yellow, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
                    spriteBatch.DrawString(hudFont, "Previous Highscores:", hudLocation + new Vector2(10, 100), Color.Yellow, 0, new Vector2(0, 0), 1.5f, SpriteEffects.None, 0);

                    for (int i = 0; i < level.highscoresInt.Count; i++)
                    {
                        spriteBatch.DrawString(hudFont, "Player "+(i+1)+": "+ level.highscoresInt[i], 
                            hudLocation + new Vector2(10, 120 + (i+1) * 20), Color.Yellow, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                    }
                    return;
                }
                else
                {
                    // Draw status message.
                    Vector2 statusSize = new Vector2(status.Width, status.Height);
                    spriteBatch.Draw(status, center - statusSize / 2, Color.White);
                }
            }
        }

        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }
    }
}
