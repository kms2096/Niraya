using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;


namespace Niraya
{
    /// <summary>
    /// Manages all game processes and draws output to screen
    /// </summary>
    public class GameManager
    {
        /// <summary>
        /// The current dimension the player exists in, to use for drawing tileset and background
        /// </summary>
        public enum DimensionState
        {
            Overworld = 0,
            Underworld = 1
        }

        /// <summary>
        /// The state of the game
        /// </summary>
        private enum GameState
        {
            Game,
            Credits,
            Options,
            PauseOptions,
            GameOver,
            Title,
            Victory,
            WarningScreen
        }

        /// <summary>
        /// The game camera
        /// </summary>
        public static Camera camera;

        private List<Level> levels;
        int currentLevelIndex;

        private GUIMeter healthbar;
        private GUIFadeFx screenFadeFx;
        private GUISprite deathSprite;
        private List<GUIElement> ingameGuiElementList;

        /// <summary>
        /// Collision manager instance
        /// </summary>
        public static CollisionManager _collisionManager;

        /// <summary>
        /// The tilesets the level uses
        /// </summary>
        private Tileset[] tilesets;

        /// <summary>
        /// The parallaxing backgrounds used behind the level
        /// </summary>
        private ParallaxingBackground[] backgrounds;

        /// <summary>
        /// Current dimension, whether we are in Overworld or Underworld
        /// </summary>
        public static DimensionState dimensionState;

        /// <summary>
        /// The current state of the game
        /// </summary>
        private GameState currentGameState;

        /// <summary>
        /// If we are in debugging mode
        /// </summary>
        public static bool debugModeEnabled;
        /// <summary>
        /// If background music is enabled
        /// </summary>
        public static bool musicEnabled;
        /// <summary>
        /// If the game is paused
        /// </summary>
        private bool isPaused;
        /// <summary>
        /// If the player is invulnerable
        /// </summary>
        public static bool isGodmodeEnabled;


        // Input states
        private MouseState mouseState;
        private KeyboardState kbState;
        private KeyboardState previousKbState;

        // Menu Screens
        private MenuScreen mainMenu;
        private MenuScreen optionsMenu;
        private MenuScreen creditsMenu;
        private MenuScreen pauseMenu;

        #region ### WARNING SCREEN FADE ###

        /// <summary>
        /// Warning screen before game starts
        /// </summary>
        private GUIFadeFx warningScreenFadeFx;

        /// <summary>
        /// Time at which the Warning Screen begins to fade in
        /// </summary>
        private readonly TimeSpan WarningScreenFadeInTime = TimeSpan.FromSeconds(1f);

        /// <summary>
        /// Time at which the Warning Screen begins to fade out
        /// </summary>
        private readonly TimeSpan WarningScreenFadeOutTime = TimeSpan.FromSeconds(5f);

        /// <summary>
        /// Time at which the Warning Screen goes to Title
        /// </summary>
        private readonly TimeSpan WarningScreenEndTime = TimeSpan.FromSeconds(6f);

        #endregion

        // WILL MOVE THESE TO A DIFFERENT CLASS LATER
        private Texture2D victoryScreen;
        private Texture2D gameOverScreen;

        /// <summary>
        /// The onscreen dimensions of a single tile
        /// </summary>
        public const int TileSize = 48;

        /// <summary>
        /// Constant Y-axis acceleration to simulate gravity
        /// </summary>
        public const float DefaultGravity = 1.49f;

        /// <summary>
        /// Cap for how fast an entity can fall
        /// </summary>
        public const float MaxFallSpeed = 50f;

        /// <summary>
        /// Large font used for debug text
        /// </summary>
        private SpriteFont arial30font;
        /// <summary>
        /// Small font used for debug text
        /// </summary>
        private SpriteFont consolas12font;

        /// <summary>
        /// Creates a GameManager
        /// </summary>
        public GameManager()
        {
            // Create collision manager
            _collisionManager = new CollisionManager();

            // Create camera
            camera = new Camera();

            // Initialize list of levels
            levels = new List<Level>();

            // Start on the first level
            currentLevelIndex = 0;

            // Start on the overworld tileset
            dimensionState = DimensionState.Overworld;

            currentGameState = GameState.WarningScreen;

            // Unpause
            isPaused = false;

            debugModeEnabled = false;

            musicEnabled = true;

            isGodmodeEnabled = false;
        }

        /// <summary>
        /// Update method, called each frame
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Get mouse and keyboard states
            mouseState = Mouse.GetState();
            kbState = Keyboard.GetState();

            //FSM for Game States
            switch (currentGameState)
            {
                #region ### case WARNING SCREEN: ###

                // Warning Screen
                case GameState.WarningScreen:

                    // Update warning screen fade
                    warningScreenFadeFx.Update(gameTime);

                    // If it's fully transparent and time to fade in
                    if (warningScreenFadeFx.CurrentFadeState == GUIFadeFx.FadeState.Inactive &&
                        gameTime.TotalGameTime > WarningScreenFadeInTime)
                    {
                        // Fade in
                        warningScreenFadeFx.StartFadeIn();
                    }

                    // If it's fully opaque and it's time to fade out
                    if (gameTime.TotalGameTime > WarningScreenFadeOutTime &&
                        warningScreenFadeFx.CurrentFadeState == GUIFadeFx.FadeState.FullOpaque)
                    {
                        // Fade out
                        warningScreenFadeFx.StartFadeOut();
                    }

                    // Once fade is fully transparent (ended), go to title
                    // Or, if user presses Enter
                    if (SingleKeyPress(kbState, previousKbState, Keys.Enter) ||
                        gameTime.TotalGameTime > WarningScreenEndTime)
                    {
                        // Go to title
                        currentGameState = GameState.Title;
                    }

                    break;

                #endregion

                #region ### case TITLE SCREEN: ###
                // Title Screen
                case GameState.Title:

                    // Update main menu
                    mainMenu.UpdateMainMenu(kbState, previousKbState);

                    // Check if background music is enabled
                    if (musicEnabled)
                    {
                        AudioManager.soundEffects["BGM\\xDeviruchi - Title Theme"].Play();
                    }
                    else
                    {
                        AudioManager.soundEffects["BGM\\xDeviruchi - Title Theme"].Pause();
                    }

                    // On user selection
                    if (SingleKeyPress(kbState, previousKbState, Keys.Enter))
                    {
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu confirm"]);

                        // Start game button is selected
                        if (mainMenu.MainButtonSelected == MainMenuState.startSelected)
                        {
                            // Switch to Game state
                            currentGameState = GameState.Game;

                            dimensionState = DimensionState.Overworld;

                            // Stop title music
                            AudioManager.soundEffects["BGM\\xDeviruchi - Title Theme"].Stop();

                            // Load the first level
                            StartLevel(0);
                        }

                        // Options button is selected
                        else if (mainMenu.MainButtonSelected == MainMenuState.optionsSelected)
                        {
                            currentGameState = GameState.Options;
                        }

                        // Credits button is selected
                        else
                        {
                            currentGameState = GameState.Credits;
                        }
                    }
                    break;

                #endregion

                #region ### case GAME: ###

                // Main Game Loop
                case GameState.Game:

                    Player player = levels[currentLevelIndex].player;

                    // if UNPAUSED:
                    if (!isPaused)
                    {
                        // --- CAMERA TARGET --- //
                        camera.TargetPosition =
                            new Vector2(
                                // Player position
                                player.Position.X - (Game1.ScreenWidth * .5f) + player.Hitbox.Width / 2,
                                player.Position.Y - (Game1.ScreenHeight * .55f) + player.Hitbox.Height / 2
                            );

                        // Update level
                        levels[currentLevelIndex].Update(gameTime);


                        // --- UPDATE CAMERA --- //

                        camera.Update();

                        // --- UPDATE HEALTH BAR --- //
                        healthbar.CurrentValue = player.Health;

                        foreach (GUIElement g in ingameGuiElementList)
                        {
                            g.Update(gameTime);
                        }

                        // If player is dead
                        if (player.CurrentPlayerState == Player.PlayerState.Dead)
                        {
                            deathSprite.IsActive = true;

                            // If they are in the overworld, they can respawn in the underworld
                            if (dimensionState == DimensionState.Overworld)
                            {
                                // On pressing enter (as long as the fade has yet to start)
                                if (kbState.IsKeyDown(Keys.Enter) &&
                                    screenFadeFx.CurrentFadeState == GUIFadeFx.FadeState.FullTransparent)
                                {
                                    // Start fade in/out effect
                                    screenFadeFx.StartFadeIn();

                                    AudioManager.soundEffects["effects\\super crash"].Play();
                                }

                                // Once the screen is completely white, switch to underworld
                                if (screenFadeFx.CurrentFadeState == GUIFadeFx.FadeState.FullOpaque)
                                {
                                    dimensionState = DimensionState.Underworld;
                                    levels[currentLevelIndex].CurrentTileset = tilesets[(int)dimensionState];
                                    levels[currentLevelIndex].TransitionToUnderworld();

                                    player.ReviveAfterDeath();

                                    screenFadeFx.StartFadeOut();

                                    deathSprite.IsActive = false;
                                }
                            }
                            
                            // If they are in the underworld
                            else
                            {
                                // On pressing enter (as long as the fade has yet to start)
                                if (kbState.IsKeyDown(Keys.Enter) &&
                                    screenFadeFx.CurrentFadeState == GUIFadeFx.FadeState.FullTransparent)
                                {
                                    // Start fade effect
                                    screenFadeFx.StartFadeIn();
                                }

                                // Once the screen is completely white, switch to game over
                                if (screenFadeFx.CurrentFadeState == GUIFadeFx.FadeState.FullOpaque)
                                {
                                    AudioManager.StopAllEffects();
                                    currentGameState = GameState.GameOver;
                                    deathSprite.IsActive = false;

                                    screenFadeFx.Reset();
                                }
                            }
                        }

                        // If the player collides with end of level
                        if (_collisionManager.CheckEntityCollision(levels[currentLevelIndex].levelGoal, player))
                        {
                            // If it's the final level (goal type == dog), then win
                            if (levels[currentLevelIndex].levelGoal.Type == LevelGoal.GoalType.Dog ||
                                // (Also check if its the last level in the list so we don't go out of range)
                                currentLevelIndex + 1  == levels.Count)
                            {
                                AudioManager.StopAllEffects();
                                currentGameState = GameState.Victory;
                            }
                            // Otherwise, go to the next level
                            else
                            {
                                //AudioManager.StopAllEffects();
                                StartLevel(currentLevelIndex + 1);
                            }
                        }

                        // If music is enabled, play the appropriate music for the dimension
                        if (musicEnabled)
                        {
                            if (dimensionState == DimensionState.Overworld)
                            {
                                AudioManager.soundEffects["BGM\\xDeviruchi - And The Journey Begins"].Play();
                                AudioManager.soundEffects["BGM\\xDeviruchi - The Icy Cave "].Stop();
                                // Stop underworld music
                            }
                            else
                            {
                                // Start underworld music
                                AudioManager.soundEffects["BGM\\xDeviruchi - The Icy Cave "].Play();
                                AudioManager.soundEffects["BGM\\xDeviruchi - And The Journey Begins"].Stop();
                            }
                        }
                        else
                        {
                            AudioManager.soundEffects["BGM\\xDeviruchi - And The Journey Begins"].Pause();
                            AudioManager.soundEffects["BGM\\xDeviruchi - The Icy Cave "].Pause();
                        }

                        // DEBUGGING KEYS
                        if (debugModeEnabled)
                        {
                            if (SingleKeyPress(kbState, previousKbState, Keys.D1))
                            {
                                dimensionState = DimensionState.Overworld;
                                levels[currentLevelIndex].CurrentTileset = tilesets[(int)dimensionState];
                            }
                            if (SingleKeyPress(kbState, previousKbState, Keys.D2))
                            {
                                dimensionState = DimensionState.Underworld;
                                levels[currentLevelIndex].CurrentTileset = tilesets[(int)dimensionState];
                                levels[currentLevelIndex].TransitionToUnderworld();                                
                            }

                            if (SingleKeyPress(kbState, previousKbState, Keys.L))
                            {
                                StartLevel((currentLevelIndex + 1) % levels.Count);
                            }

                            if (SingleKeyPress(kbState, previousKbState, Keys.G))
                            {
                                isGodmodeEnabled = !isGodmodeEnabled;
                            }

                            // Player Damage Test
                            if (SingleKeyPress(kbState, previousKbState, Keys.R))
                            {
                                player.TakeDamage(gameTime, 35,
                                    // Source of the damage is from 1 pixel in front of them
                                    player.Hitbox.Center.ToVector2() +
                                    new Vector2((int)player.CurrentFacingDirection, 0));
                            }
                        }
                    }
                    // if PAUSED:
                    else
                    {
                        pauseMenu.UpdatePauseMenu(kbState, previousKbState);

                        // On user selection (with ENTER)
                        if (SingleKeyPress(kbState, previousKbState, Keys.Enter))
                        {
                            AudioManager.soundEffects["menu confirm"].Play();

                            // Based on the highlighted button, take appropriate action
                            if (pauseMenu.PauseButtonSelected == PauseMenuState.resumeSelected)
                            {
                                isPaused = false;
                            }
                            else if (pauseMenu.PauseButtonSelected == PauseMenuState.optionsSelected)
                            {
                                currentGameState = GameState.PauseOptions;
                            }
                            else if (pauseMenu.PauseButtonSelected == PauseMenuState.quitSelected)
                            {
                                currentGameState = GameState.Title;

                                isPaused = false;

                                AudioManager.StopAllEffects();
                            }
                        }
                    }



                    // -- DEBUG MODE TOGGLE FUNCTIONALITY

                    // If B key pressed
                    if (SingleKeyPress(kbState, previousKbState, Keys.B))
                    {
                        debugModeEnabled = !debugModeEnabled;
                    }

                    // -- MUSIC TOGGLE FUNCTIONALITY

                    // If M key pressed
                    if (SingleKeyPress(kbState, previousKbState, Keys.M))
                    {
                        musicEnabled = !musicEnabled;
                    }

                    // -- PAUSE TOGGLE FUNCTIONALITY

                    // If ESC or P key pressed
                    if (SingleKeyPress(kbState, previousKbState, Keys.Escape) || SingleKeyPress(kbState, previousKbState, Keys.P))
                    {
                        // Toggle pause state
                        isPaused = !isPaused;

                        // Reset pause menu state
                        pauseMenu.PauseButtonSelected = PauseMenuState.resumeSelected;

                        // Stop all audio
                        AudioManager.PauseAllEffects();
                    }

                    break;

                #endregion

                case GameState.PauseOptions:

                    optionsMenu.UpdateOptionsMenu(kbState, previousKbState);

                    if (SingleKeyPress(kbState, previousKbState, Keys.Back))
                    {
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu confirm"]);
                        currentGameState = GameState.Game;
                    }

                    break;

                case GameState.Credits:

                    creditsMenu.UpdateCreditsMenu(kbState, previousKbState);

                    // Back to main menu
                    if (SingleKeyPress(kbState, previousKbState, Keys.Back))
                    {
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu confirm"]);
                        currentGameState = GameState.Title;
                    }
                    break;

                case GameState.GameOver:

                    // Go to main menu on ENTER press
                    if (SingleKeyPress(kbState, previousKbState, Keys.Enter))
                    {
                        currentGameState = GameState.Title;
                        AudioManager.StopAllEffects();
                    }
                    break;

                case GameState.Options:

                    optionsMenu.UpdateOptionsMenu(kbState, previousKbState);

                    // Check if background music is enabled
                    if (musicEnabled)
                    {
                        AudioManager.soundEffects["BGM\\xDeviruchi - Title Theme"].Play();
                    }
                    else
                    {
                        AudioManager.soundEffects["BGM\\xDeviruchi - Title Theme"].Pause();
                    }

                    if (SingleKeyPress(kbState, previousKbState, Keys.Back))
                    {
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu confirm"]);
                        currentGameState = GameState.Title;
                    }
                    break;

                case GameState.Victory:

                    //Credits Transition
                    if (SingleKeyPress(kbState, previousKbState, Keys.Enter))
                    {
                        currentGameState = GameState.Title;

                        // Stop all audio
                        AudioManager.StopAllEffects();
                    }

                    break;
            }

            // Set previous keyboard state
            previousKbState = kbState;
        }

        /// <summary>
        /// Reloads in the level from its file and sets the current level index
        /// </summary>
        /// <param name="levelIndex">The level index to load</param>
        private void StartLevel(int levelIndex)
        {
            // Unload existing level
            levels[currentLevelIndex].UnloadLevel();

            // Set the current level index
            currentLevelIndex = levelIndex;

            // Load the level from the file
            levels[currentLevelIndex].LoadLevel();

            // Set the current level's tileset
            levels[currentLevelIndex].CurrentTileset = tilesets[(int)dimensionState];

            // Set CollisionManager current level
            _collisionManager.CurrentLevel = levels[currentLevelIndex];


            // TODO: Fix hardcoded values below

            // This is the number of tiles from the edge of the level that the camera CANNOT see
            const float TileMargin = 2.2f;
            // Use this to set how far the camera can move left and right on this level
            camera.HorizontalBounds = (

                // Offset by a tile margin
                (int)(TileMargin * TileSize),

                // Subtract screenWidth to offset the width of the camera window, and then offset by the tile margin
                levels[currentLevelIndex].LevelWidth - Game1.ScreenWidth - (int)(TileMargin * TileSize)
            );

            // Move camera to starting position
            camera.Position = new Vector2(
                // Player position
                levels[currentLevelIndex].player.Position.X - (Game1.ScreenWidth * .5f) + levels[currentLevelIndex].player.Hitbox.Height / 2,
                levels[currentLevelIndex].player.Position.Y - (Game1.ScreenHeight * .55f) + levels[currentLevelIndex].player.Hitbox.Height / 2
            );

            // Fade out
            screenFadeFx.StartFadeOut();
        }

        /// <summary>
        /// Draw method, called each frame
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch sb)
        {
            switch (currentGameState)
            {
                // WARNING SCREEN - Draw warning screen texture
                case GameState.WarningScreen:

                    warningScreenFadeFx.Draw(sb);

                    break;

                // TITLE - Draw main menu
                case GameState.Title:

                    mainMenu.DrawMainMenu(sb);

                    break;

                // GAME - Draw level, entities, HUD
                case GameState.Game:

                    // -- Draw LEVEL --

                    // Draw BG, using the current dimensionState
                    backgrounds[(int)dimensionState].DrawBackgroundLayers(sb, camera.Position);

                    // Draw the current Level
                    levels[currentLevelIndex].Draw(gameTime, sb);

                    // -- Draw ingame GUI ELEMENTS --
                    foreach (GUIElement g in ingameGuiElementList)
                    {
                        g.Draw(sb);
                    }

                    

                    // Draw debugging strings
                    if (debugModeEnabled)
                    {
                        // Enemy index we are debugging
                        int enemyIndexToDebug = 0;

                        sb.DrawString(consolas12font, "player.AnimationName : " + levels[currentLevelIndex].player.AnimForDebug.CurrentAnimation, new Vector2(50, 120), Color.LimeGreen);
                        sb.DrawString(consolas12font, "player.PlayerState : " + levels[currentLevelIndex].player.CurrentPlayerState.ToString(), new Vector2(50, 140), Color.LimeGreen);
                        sb.DrawString(consolas12font, "player.IsFalling : " + levels[currentLevelIndex].player.IsFalling.ToString(), new Vector2(50, 160), Color.LimeGreen);
                        sb.DrawString(consolas12font, "player.AnimationFrame : " + levels[currentLevelIndex].player.AnimForDebug.CurrentFrame, new Vector2(50, 180), Color.LimeGreen);

                        sb.DrawString(consolas12font, $"player.health: {levels[currentLevelIndex].player.Health}", new Vector2(50, 220), Color.LimeGreen);
                        sb.DrawString(consolas12font, $"Enemy[{enemyIndexToDebug}] Health: {levels[currentLevelIndex].enemyList[0].Health}", new Vector2(50, 240), Color.LimeGreen);

                        sb.DrawString(consolas12font, $"Music enabled: {musicEnabled}", new Vector2(50, 280), Color.LimeGreen);
                        //sb.DrawString(arial30font, "camera position x : " + camera.Position.X, new Vector2(50, 120), Color.LimeGreen);
                        //sb.DrawString(arial30font, "background clamped offset x : " + backgrounds[(int)dimensionState].clampedOffsetX, new Vector2(50, 160), Color.LimeGreen);


                        #region ### Debug - Check obstruction hitboxes for enemy ###


                        // Create a new hitbox to sense for obstructions
                        Rectangle entityDetectionHitbox = new Rectangle(
                        // Same location as the entity hitbox
                        levels[currentLevelIndex].enemyList[enemyIndexToDebug].Hitbox.Location,

                        new Point(
                            // Width of half a tile
                            GameManager.TileSize / 2,
                            // Height is the same height as the entity hitbox
                            // Remove 1 pixel since we are embedded 1 pixel into the ground
                            levels[currentLevelIndex].enemyList[enemyIndexToDebug].Hitbox.Height - 1));

                        if (levels[currentLevelIndex].enemyList[enemyIndexToDebug].CurrentFacingDirection == CharacterEntity.FacingDirection.Left)
                        {
                            // If facing left, offset to the left of the hitbox
                            entityDetectionHitbox.Offset(-GameManager.TileSize / 2, 0);
                        }
                        else
                        {
                            // Otherwise (facing right), offset to the right of the hitbox
                            entityDetectionHitbox.Offset(levels[currentLevelIndex].enemyList[enemyIndexToDebug].Hitbox.Width, 0);
                        }

                        // Adjust by camera for drawing
                        entityDetectionHitbox.Offset((-camera.Position).ToPoint());

                        // DRAW DEBUG HITBOX for TILE OBSTRUCTION CHECK
                        sb.Draw(
                            TextureManager.textures["_debugging\\DebugSprite"],
                            entityDetectionHitbox,
                            Color.CadetBlue * .15f);

                        // Resize hitbox for the purpose of sensing if CharacterEntity is on a ledge
                        // Height is only half a tile
                        entityDetectionHitbox.Height = GameManager.TileSize / 2;
                        // Width should only be half a tile as well
                        entityDetectionHitbox.Width = GameManager.TileSize / 2;

                        // Move below the CharacterEntity's hitbox
                        entityDetectionHitbox.Y += levels[currentLevelIndex].enemyList[0].Hitbox.Height;

                        // DRAW DEBUG HITBOX for LEDGE CHECK
                        sb.Draw(
                            TextureManager.textures["_debugging\\DebugSprite"],
                            entityDetectionHitbox,
                            Color.SeaGreen * .15f);

                        #endregion
                    }

                    if (isPaused)
                    {
                        pauseMenu.DrawPauseMenu(sb);
                    }

                    break;

                // CREDITS
                case GameState.Credits:
                    creditsMenu.DrawCreditsMenu(sb);
                    break;

                // GAMEOVER
                case GameState.GameOver:
                    sb.Draw(gameOverScreen,
                            new Rectangle(0, 0, gameOverScreen.Width, gameOverScreen.Height),
                            Color.White);
                    break;
                case GameState.Options:
                    optionsMenu.DrawOptionsMenu(sb);
                    break;
                case GameState.PauseOptions:
                    optionsMenu.DrawOptionsMenu(sb);
                    break;
                case GameState.Victory:
                    //draws victory screen
                    sb.Draw(victoryScreen,
                            new Rectangle(0, 0, victoryScreen.Width, victoryScreen.Height),
                            Color.White);
                    break;
            }
        }

        /// <summary>
        /// Creates all levels in the game (does not load them)
        /// </summary>
        /// <param name="levelNames">Level names to create</param>
        private void CreateLevels(string[] levelNames)
        {
            foreach (string levelName in levelNames)
            {
                // Add the new Level
                levels.Add(new Level(levelName));
            }
        }

        /// <summary>
        /// Loads content relevant to game
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            // TEXTURES
            TextureManager.LoadContent(content);

            victoryScreen = TextureManager.textures["BG_menus\\victoryScreen"];
            gameOverScreen = TextureManager.textures["BG_menus\\gameOverScreen"];

            // MISC

            // Debug fonts
            arial30font = content.Load<SpriteFont>("_Font Assets\\Arial30");
            consolas12font = content.Load<SpriteFont>("_Font Assets\\Consolas12");

            // AUDIO

            #region ### Audio ###

            AudioManager.LoadContent(content);

            // Loop and set volume levels for the BGM
            AudioManager.soundEffects["BGM\\xDeviruchi - Title Theme"].IsLooped = true;
            AudioManager.soundEffects["BGM\\xDeviruchi - Title Theme"].Volume = 0.4f;
            AudioManager.soundEffects["BGM\\xDeviruchi - The Icy Cave "].IsLooped = true;
            AudioManager.soundEffects["BGM\\xDeviruchi - The Icy Cave "].Volume = 0.195f;
            AudioManager.soundEffects["BGM\\xDeviruchi - And The Journey Begins"].IsLooped = true;
            AudioManager.soundEffects["BGM\\xDeviruchi - And The Journey Begins"].Volume = 0.195f;

            #endregion


            #region ### Level, Backgrounds, Tilesets Setup ###

            // Level names to load, in the order they will be played
            string[] levelNames = {
                "level00",
                "level01",
                "stresstest",
                "test00"
            };

            // Load in the above level files
            CreateLevels(levelNames);

            // Tilesets to load
            tilesets = new Tileset[]
            {
                // Green valley tileset, used for the Overworld
                new Tileset(content.Load<Texture2D>("_Image Assets\\Tilesets\\GreenvalleyNiraya_3x"), TileSize),

                // Stringstar tileset, used for the Underworld
                new Tileset(content.Load<Texture2D>("_Image Assets\\Tilesets\\StringstarNiraya_3x"), TileSize)
            };

            // Background assets to load
            backgrounds = new ParallaxingBackground[]
            {
                // Overworld
                new ParallaxingBackground(
                    
                    // Width and height of the background layer
                    1536, 1200,
                    
                    // How far to vertically shift
                    -120,

                    // All layers in back-to-front order
                    new Texture2D[]{
                        content.Load<Texture2D>("_Image Assets\\BG_overworld\\Hills3xExtended_0005_layer01"),
                        content.Load<Texture2D>("_Image Assets\\BG_overworld\\Hills3xExtended_0004_layer02"),
                        content.Load<Texture2D>("_Image Assets\\BG_overworld\\Hills3xExtended_0003_layer03"),
                        content.Load<Texture2D>("_Image Assets\\BG_overworld\\Hills3xExtended_0002_layer04"),
                        content.Load<Texture2D>("_Image Assets\\BG_overworld\\Hills3xExtended_0001_layer05"),
                        content.Load<Texture2D>("_Image Assets\\BG_overworld\\Hills3xExtended_0000_layer06")
                    }
                ),

                // Underworld
                new ParallaxingBackground(
                    
                    // Width and height of the background layer
                    1586, 1856,

                    // How far to vertically shift
                    -500,

                    // All layers in back-to-front order
                    new Texture2D[]{
                        content.Load<Texture2D>("_Image Assets\\BG_underworld\\Layer_0010_1"),
                        content.Load<Texture2D>("_Image Assets\\BG_underworld\\Layer_0009_2"),
                        content.Load<Texture2D>("_Image Assets\\BG_underworld\\Layer_0008_3"),
                        content.Load<Texture2D>("_Image Assets\\BG_underworld\\Layer_0007_Lights"),
                        content.Load<Texture2D>("_Image Assets\\BG_underworld\\Layer_0006_4"),
                        content.Load<Texture2D>("_Image Assets\\BG_underworld\\Layer_0005_5"),
                        content.Load<Texture2D>("_Image Assets\\BG_underworld\\Layer_0004_Lights"),
                        content.Load<Texture2D>("_Image Assets\\BG_underworld\\Layer_0003_6"),
                        content.Load<Texture2D>("_Image Assets\\BG_underworld\\Layer_0002_7"),
                        content.Load<Texture2D>("_Image Assets\\BG_underworld\\Layer_0001_8"),
                        content.Load<Texture2D>("_Image Assets\\BG_underworld\\Layer_0000_9")
                    }
                )
            };

            #endregion


            #region ### Menu Screen Constructors ###

            // TODO: Sound assets should all point to the same instance if they are the same sound. Right now the same sound file is .Load() multiple times!

            // Textures for main menu screen
            mainMenu = new MenuScreen(
                new Texture2D[]
                {
                    content.Load<Texture2D>("_Image Assets\\BG_menus\\mainStartSelected"),
                    content.Load<Texture2D>("_Image Assets\\BG_menus\\mainOptionsSelected"),
                    content.Load<Texture2D>("_Image Assets\\BG_menus\\mainCreditsSelected"),
                });

            // Textures for options menu screen
            optionsMenu = new MenuScreen(
                new Texture2D[]
                {
                    content.Load<Texture2D>("_Image Assets\\BG_menus\\optionsMusicSelected"),
                    content.Load<Texture2D>("_Image Assets\\BG_menus\\optionsMSelectedMChecked"),
                    content.Load<Texture2D>("_Image Assets\\BG_menus\\optionsMSelectedDChecked"),
                    content.Load<Texture2D>("_Image Assets\\BG_menus\\optionsMSelectedBothChecked"),
                    content.Load<Texture2D>("_Image Assets\\BG_menus\\optionsDebugSelected"),
                    content.Load<Texture2D>("_Image Assets\\BG_menus\\optionsDSelectedMChecked"),
                    content.Load<Texture2D>("_Image Assets\\BG_menus\\optionsDSelectedDChecked"),
                    content.Load<Texture2D>("_Image Assets\\BG_menus\\optionsDSelectedBothChecked")
                });

            // Textures for credit menu screen
            creditsMenu = new MenuScreen(
                new Texture2D[] {
                    content.Load<Texture2D>("_Image Assets\\BG_menus\\creditsMenuP1"),
                    content.Load<Texture2D>("_Image Assets\\BG_menus\\creditsMenuP2"),
                    content.Load<Texture2D>("_Image Assets\\BG_menus\\creditsMenuP3")
                });

            // Textures for pause menu screen
            pauseMenu = new MenuScreen(
                new Texture2D[] {
                    content.Load<Texture2D>("_Image Assets\\BG_menus\\pauseScreenOverlay"),
                    content.Load<Texture2D>("_Image Assets\\BG_menus\\pauseResumeSelected"),
                    content.Load<Texture2D>("_Image Assets\\BG_menus\\pauseOptionsSelected"),
                    content.Load<Texture2D>("_Image Assets\\BG_menus\\pauseQuitSelected"),
                });

            #endregion


            #region ### GUI Constructors ###

            healthbar = new GUIMeter(
                // Position
                new Vector2(16, 16),

                // Gray bar allows tinting
                content.Load<Texture2D>("_Image Assets\\UI\\greybar_cropped_x4"),
                content.Load<Texture2D>("_Image Assets\\UI\\healthbar_cropped_x4"),

                // X offset between the textures
                32,

                Player.PlayerDefaultHealth);

            screenFadeFx = new GUIFadeFx(
                content.Load<Texture2D>("_Image Assets\\fx\\screenFade"),
                0.0072f,
                0.02f);

            deathSprite = new GUISprite(
                new Vector2(0, 0),
                content.Load<Texture2D>("_Image Assets\\UI\\deathOverlay"));

            // GUI Element List while game is running
            ingameGuiElementList = new List<GUIElement>
            {
                healthbar,
                deathSprite,
                screenFadeFx
            };

            // WARNING SCREEN FADE
            warningScreenFadeFx = new GUIFadeFx(
                content.Load<Texture2D>("_Image Assets\\UI\\WarningScreen"),
                0.008f,
                0.03f);

            #endregion

        }

        /// <summary>
        /// Returns true if the provided key is being pressed this frame but not last frame
        /// </summary>
        public static bool SingleKeyPress(KeyboardState kbState, KeyboardState previousKbState, Keys keyToCheck)
        {
            if (previousKbState == null)
            {
                return false;
            }

            // Check if it was pressed last frame
            if (kbState.IsKeyDown(keyToCheck) && previousKbState.IsKeyUp(keyToCheck))
            {
                return true;
            }

            return false;
        }
    }
}
