using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace Niraya
{
    /// <summary>
    /// Onscreen GUIElement that visualizes a quantity between 0 and a maximum value
    /// </summary>
    class GUIMeter : GUIElement
    {
        // FIELDS
        private Texture2D innerTexture;
        private Texture2D outerBorderTexture;

        /// <summary>
        /// How wide the meter should be
        /// </summary>
        private float desiredMeterWidth;
        /// <summary>
        /// How wide the meter is presently
        /// </summary>
        private float currentMeterWidth;

        /// <summary>
        /// Maximum value of this meter
        /// </summary>
        private float maxValue;

        /// <summary>
        /// How far to shift the inner texture of the meter inside the outer border texture
        /// </summary>
        private int innerTextureOffset;

        // PROPERTIES

        /// <summary>
        /// Value this meter should visualize, should be between (0 - maxValue)
        /// </summary>
        public float CurrentValue
        {
            set
            {
                // Range check
                if (value < 0 || value > maxValue)
                {
                    throw new ArgumentOutOfRangeException($"Value {value} is out of this GUIMeter's range: (0 - {maxValue})");
                }
                desiredMeterWidth = innerTexture.Width * (value / maxValue);
            }
        }

        // CONSTRUCTOR

        /// <summary>
        /// Creates a GUIMeter
        /// </summary>
        /// <param name="innerTextureOffset">How far to offset the inner meter texture horizontally</param>
        /// <param name="maxValue">Maximum value of this meter</param>
        public GUIMeter(Vector2 position, Texture2D innerTexture, Texture2D outerBorderTexture, int innerTextureOffset, float maxValue)
            : base(position)
        {
            this.innerTexture = innerTexture;
            this.outerBorderTexture = outerBorderTexture;
            this.innerTextureOffset = innerTextureOffset;

            this.position = position;

            this.maxValue = maxValue;

            desiredMeterWidth = 0;
            currentMeterWidth = 0;
        }

        // METHODS

        /// <summary>
        /// Updates the GUIMeter
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Prevent meter width from going below 0
            if (desiredMeterWidth < 0)
            {
                desiredMeterWidth = 0;
            }

            // Calculate difference between the desired width of the meter and the current width of the meter
            float change = desiredMeterWidth - currentMeterWidth;

            // If this discrepancy is significant, change the meter width
            if (Math.Abs(change) > 0.1f)
            {
                // Only change by a fraction of the amount we should change, so it animates (real smooth-like :D )
                currentMeterWidth += change * .09f;
            }
        }

        /// <summary>
        /// Draws this GUIMeter
        /// </summary>
        public override void Draw(SpriteBatch sb)
        {
            // Devious little trick to fixing off-by-1 rounding errors
            int correctedCurrentMeterWidth = (int)Math.Ceiling(currentMeterWidth - 0.5f);

            // Meter border
            sb.Draw(outerBorderTexture,
                position,
                Color.White);

            // Meter animation
            sb.Draw(innerTexture,
                // Draw the inner texture at the provided position, offset by the innerTextureOffset
                position + new Vector2(innerTextureOffset, 0),

                // The length of the texture is the meterWidth
                new Rectangle(
                    0,
                    0,
                    correctedCurrentMeterWidth,
                    innerTexture.Height),

                // Green section
                Color.LimeGreen);


            // Discrepancy visualization
            sb.Draw(innerTexture,
                // Draw the inner texture at the provided position, offset by the innerTextureOffset
                position + new Vector2(
                    // Add 1 to fix rounding issues
                    innerTextureOffset + desiredMeterWidth + 1,
                    0),

                // The length of the texture is the amount between where the meter IS and where it should be
                new Rectangle(
                    (int)desiredMeterWidth,
                    0,
                    (int)(correctedCurrentMeterWidth - desiredMeterWidth),
                    innerTexture.Height),

                // Red section
                Color.Red);
        }
    }
}
