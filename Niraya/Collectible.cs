using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Niraya
{
    /// <summary>
    /// An entity that can be collected by the player
    /// </summary>
    public class Collectible : Entity
    {
        /// <summary>
        /// A type of Collectible
        /// </summary>
        public enum CollectibleType
        {
            /// <summary>
            /// Can be eaten to restore health
            /// </summary>
            Apple
        }

        /// <summary>
        /// CollectibleType that this Collectible is
        /// </summary>
        private CollectibleType collectibleType;

        /// <summary>
        /// If this collectible is visible and can still be collected
        /// </summary>
        private bool isActive;

        public CollectibleType Type
        {
            get { return collectibleType; }
        }

        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }

        /// <summary>
        /// Creates a collectible of the given type at the provided location
        /// </summary>
        public Collectible(Point startingPosition, CollectibleType collectibleType) : base()
        {
            this.collectibleType = collectibleType;

            // Set up this collectible based on its type
            switch (collectibleType)
            {
                case CollectibleType.Apple:
                    // Texture
                    sprite = TextureManager.textures["Items\\apple_consumable3x"];

                    // Position and size
                    hitbox = new Rectangle(
                        startingPosition,
                        
                        // Size of an Apple
                        new Point(48, 48));
                    break;

                // etc . . . add more collectibles!!
            }

            // Start active
            isActive = true;
        }

        public override void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            // Only draw collectible if active
            if (isActive)
            {
                // To make the collectible float up and down, apply a slight y-offset
                float yOffset = 
                    
                    // This first clause takes the SINE of the current number of milliseconds in the game (divided by a large number to slow it way down)
                    // The function outputs a value that oscillates smoothly from 1 to -1
                    (float)(Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 500f)

                    // Multiply this value to scale it to how far up and down we want the sprite to move (amplitude)
                    * 11f);

                sb.Draw(
                    sprite,

                    // Draw at the correct position
                    (Position - GameManager.camera.Position) +

                    // Add a vector that offsets the Y position (for the visual floating up and down)
                    new Vector2(0, yOffset),

                    new Rectangle(0, 0, hitbox.Width, hitbox.Height),
                    Color.White,
                    0,
                    Vector2.Zero,
                    1,
                    SpriteEffects.FlipHorizontally,
                    0);
            }
        }

        /// <summary>
        /// Method to be called when this collectible is collected
        /// </summary>
        public void PickUp()
        {
            if (isActive)
            {
                AudioManager.ForcePlay(AudioManager.soundEffects["health pickup"]);
                isActive = false;
            }
        }
    }
}
