using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Niraya
{
    /// <summary>
    /// A background that scrolls as multiple separate layers
    /// </summary>
    class ParallaxingBackground
    {
        /// <summary>
        /// A Texture2D array storing the layer textures, in back-to-front order
        /// </summary>
        private Texture2D[] layers;

        /// <summary>
        /// Ratio of how far each layer scrolls proportionally to the layer in front of it
        /// </summary>
        private float layerScrollRatio = .9f;

        public float clampedOffsetX;

        private int layerWidth;
        private int layerHeight;

        /// <summary>
        /// Vertical shift of this ParallaxingBackground
        /// </summary>
        private int offsetY;

        /// <summary>
        /// Creates a parallaxing background
        /// </summary>
        /// <param name="layers">A Texture2D array storing the layer textures, in back-to-front order</param>
        public ParallaxingBackground(int layerWidth, int layerHeight, int offsetY, Texture2D[] layers)
        {
            this.layers = layers;

            this.layerWidth = layerWidth;
            this.layerHeight = layerHeight;
            this.offsetY = offsetY;
        }

        /// <summary>
        /// Draws the background layers provided with the global offset
        /// </summary>
        public void DrawBackgroundLayers(SpriteBatch sb, Vector2 offset)
        {
            // clampedOffsetX will always be a value between 0 and the width of the screen
            //clampedOffsetX = -(offset.X % screenWidth);
            clampedOffsetX = -(offset.X);

            // horizontalLayersToTile is the number of layers needed to tile the entire width of the screen
            // This is found by finding the nearest whole number of layers that will fit inside the screen, and adding 7
            // The 7 additional layers go on each end to fill the gaps
            int horizontalLayersToTile = (Game1.ScreenWidth / layerWidth) + 7;

            // Draw every layer
            for (int layer = 0; layer < layers.Length; layer++)
            {
                Vector2 layerPosition;

                // To create a seamless tiling, draw each layer enough times to tile the screen horizontally
                for (int i = 0; i < horizontalLayersToTile; i++)
                {
                    layerPosition =
                        new Vector2(
                            // X position

                            clampedOffsetX * ((float)Math.Pow(layerScrollRatio, layers.Length - 1 - layer)) + (i * layerWidth),


                            // Y position

                            offsetY
                        );

                    sb.Draw(
                        layers[layer],
                        layerPosition,
                        Color.White);
                    
                }
            }
        }
    }
}
