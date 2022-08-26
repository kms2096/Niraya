using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Niraya
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private GameManager _gameManager;

        public const int ScreenWidth = 1920;
        public const int ScreenHeight = 1080;

        public static Random rng;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Hide mouse cursor
            IsMouseVisible = false;

            // Set to 1920x1080 Fullscreen
            _graphics.PreferredBackBufferWidth = ScreenWidth;
            _graphics.PreferredBackBufferHeight = ScreenHeight;
            _graphics.IsFullScreen = true;

            _graphics.HardwareModeSwitch = false;

            _graphics.ApplyChanges();

            // Check if window is actually native 1920x1080
            if (
                (_graphics.GraphicsDevice.DisplayMode.Width != ScreenWidth) ||
                (_graphics.GraphicsDevice.DisplayMode.Height != ScreenHeight))
            {
                // Set to 1920x1080 Windowed
                _graphics.PreferredBackBufferWidth = ScreenWidth;
                _graphics.PreferredBackBufferHeight = ScreenHeight;
                _graphics.IsFullScreen = false;
                _graphics.ApplyChanges();

            }

            // Initialize random instance
            rng = new Random();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create spritebatch
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create manager classes
            _gameManager = new GameManager();
            _gameManager.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            // Press 0 to quit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.D0))
                Exit();

            _gameManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Same color as warning screen
            GraphicsDevice.Clear(new Color(21, 21, 21));

            // Begin drawing to the sprite batch
            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,

                // Samples textures in a way that prevents weird artifacting (unsure of the details on this one)
                SamplerState.PointClamp,

                // uh some stuff that doesnt matter atm just set it to defaults
                null, null, null,

                //_gameManager.Camera.GetMatrix);
                // ^ we are not doing the above.
                null);

            // Level Manager draw method is called here
            _gameManager.Draw(gameTime, _spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
