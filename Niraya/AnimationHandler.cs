using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Niraya
{
    /// <summary>
    /// Handles and draws animations from a spritesheet, constantly animating the currently set animation
    /// </summary>
    public class AnimationHandler
    {
        private Texture2D spritesheet;

        /// <summary>
        /// (Width, Height) of a single animation frame
        /// </summary>
        private Point frameDimensions;

        /// <summary>
        /// Horizontal shift applied to the animation based on its direction to center it on the frame 
        /// </summary>
        private int horizontalFrameOffset;

        /// <summary>
        /// (xOffset, yOffset) needed to correct an animation to center it on the frame 
        /// </summary>
        private Point frameOffset;

        private int numCols;
        private int numRows;

        /// <summary>
        /// The name of the current animation that is being played
        /// </summary>
        private string currentAnimation;

        /// <summary>
        /// A dictionary pairing each animation name to an array storing the local rectangles composing said animation
        /// </summary>
        private Dictionary<string, Rectangle[]> animationRectDict;

        /// <summary>
        /// The local rectangles that make up the currently playing animation
        /// </summary>
        private Rectangle[] currentLocalRectangles;

        /// <summary>
        /// Current frame of the current animation -- this is the index of currentLocalRectangles to use
        /// </summary>
        private int currentFrame;

        /// <summary>
        /// Frames per second of this animation handler
        /// </summary>
        private float fps;

        /// <summary>
        /// Time since the last frame was displayed
        /// </summary>
        private double animationTimer;

        /// <summary>
        /// True if the current animation has fully played through once
        /// </summary>
        private bool hasLoopedOnce;

        /// <summary>
        /// Whether the currently playing animation has looped through all frames
        /// </summary>
        public bool HasLoopedOnce
        {
            get { return hasLoopedOnce; }
        }

        /// <summary>
        /// Current frames per second
        /// </summary>
        public float CurrentFps
        {
            set { fps = value; }
        }

        /// <summary>
        /// The current relative frame of the currently playing animation (i.e. first frame = 0, etc.)
        /// </summary>
        public int CurrentFrame
        {
            get { return currentFrame; }
        }

        /// <summary>
        /// Sets the current animation to play, and resets the hasLoopedOnce flag if it is a new animation
        /// </summary>
        public string CurrentAnimation
        {
            get { return currentAnimation; }

            set
            {
                // Check if this is a new animation
                if (value != currentAnimation)
                {
                    // If it is, then record that it has not fully looped through yet
                    hasLoopedOnce = false;

                    // Set the current animation number to the provided value
                    currentAnimation = value;

                    // Set the current rectangles to use from the current animation number
                    currentLocalRectangles = animationRectDict[currentAnimation];

                    // Set the current frame of animation to be the first
                    currentFrame = 0;
                }
            }
        }

        public AnimationHandler(Texture2D spritesheet, Point frameDimensions, Point frameOffset, int horizontalFrameOffset, Dictionary<string, int[]> animationDict)
        {
            this.spritesheet = spritesheet;

            // Frame information
            this.frameDimensions = frameDimensions;
            this.frameOffset = frameOffset;
            this.horizontalFrameOffset = horizontalFrameOffset;

            // Ensure that spritesheet has dimensions that are a factor of frameDimensions
            if ((spritesheet.Width % frameDimensions.X != 0) ||
                (spritesheet.Height % frameDimensions.Y != 0))
            {
                throw new InvalidOperationException("spritesheet must have dimensions that are factors of frameDimensions");
            }

            numCols = spritesheet.Width / frameDimensions.X;
            numRows = spritesheet.Height / frameDimensions.Y;

            // Will convert a dictionary pairing animation numbers to the animation frames
            // into a dictionary pairing animation numbers to the rectangles on the spritesheet
            animationRectDict = CreateRectangleDictionary(animationDict);

            // Start on frame 0
            currentFrame = 0;

            // Reset animation timer
            animationTimer = 0;

            // Default fps
            fps = 12;

            currentAnimation = null;
        }

        /// <summary>
        /// Given a dictionary pairing each animation name to the frame indices that compose it,
        /// outputs a dictionary pairing each animation name to the local rectangles that compose it
        /// </summary>
        private Dictionary<string, Rectangle[]> CreateRectangleDictionary(Dictionary<string, int[]> animationDict)
        {
            // Create an empty dictionary to store each animation number's rectangle array
            Dictionary<string, Rectangle[]> animationRectDict = new Dictionary<string, Rectangle[]>();

            // Go through every animation number in the dictionary
            foreach (KeyValuePair<string, int[]> entry in animationDict)
            {
                // Create an array of rectangles with the same length as the array of frame indices
                Rectangle[] localRectangles = new Rectangle[entry.Value.Length];

                // Go through this rectangle array
                for (int i = 0; i < localRectangles.Length; i++)
                {
                    // Get the corresponding local rectangle on the spritesheet for every animation frame index
                    localRectangles[i] = GetSpritesheetLocalRect(entry.Value[i]);
                }

                // Finally, add the animation number and its composing local rectangles to the animationRectDict
                animationRectDict.Add(entry.Key, localRectangles);
            }

            return animationRectDict;
        }

        /// <summary>
        /// Provided with the index of a given frame, will return the Rectangle of that frame's position in the spritesheet
        /// </summary>
        private Rectangle GetSpritesheetLocalRect(int frameIndex)
        {
            // Ensure that index of is in range of possible indices
            if (frameIndex < 0 || frameIndex > (numCols * numRows - 1))
            {
                throw new IndexOutOfRangeException($"Index {frameIndex} out of range 0-{numCols * numRows - 1}");
            }

            // Find how far across and down this frame is
            int frameCol = frameIndex % numCols;
            int frameRow = frameIndex / numCols;

            // Return the rectangle with the correct top left coordinates,
            // and the frame dimensions
            return new Rectangle(
                frameDimensions.X * frameCol,
                frameDimensions.Y * frameRow,
                frameDimensions.X,
                frameDimensions.Y);
        }

        /// <summary>
        /// Updates the state of the playing animation
        /// </summary>
        public void UpdateAnimation(GameTime gameTime)
        {
            // Add to the time counter (need TOTALSECONDS here)
            animationTimer += gameTime.ElapsedGameTime.TotalSeconds;

            // Has enough time gone by to actually flip frames?
            if (animationTimer >= (1.0 / fps))
            {
                // Increment the frame
                currentFrame++;

                // TODO: This is implemented horrendously but it solves the issue more than it breaks it
                // Since the hasLoopedOnce was not being checked until after the next Draw() call, causing 1 frame of animation loop, it has been moved back by 1 frame
                // Now, the animation is marked as being "looped" on the SECOND-TO-LAST frame, rather than on the LAST frame.
                if (currentFrame == currentLocalRectangles.Length - 1)
                {
                    // Mark that we have looped
                    hasLoopedOnce = true;
                }

                // There are as many frames are there are local rectangles,
                // so if the current frame is out of that range, loop the animation
                if (currentFrame >= currentLocalRectangles.Length)
                {
                    // Go back to first frame (loop)
                    currentFrame = 0;

                    // Mark that we have looped
                    //hasLoopedOnce = true;
                }

                // Remove one "frame" worth of time
                animationTimer -= (1.0 / fps);
            }
        }

        /// <summary>
        /// Draws the current frame of animation centered at the specified hitbox and with the specified direction
        /// </summary>
        public void Draw(SpriteBatch sb, Vector2 centerPosition, CharacterEntity.FacingDirection facingDirection, float opacity)
        {
            sb.Draw(
                // Sprite sheet is the texture
                spritesheet,

                // Draw from the center of the hitbox, 
                centerPosition

                // but then offset by HALF the dimensions of the animation frame to center it
                - new Vector2(frameDimensions.X / 2, frameDimensions.Y / 2)

                // and then shift the frame by the correct offset
                + new Vector2(frameOffset.X, frameOffset.Y)

                // and then correct it by the horizontal frame offset
                + new Vector2(horizontalFrameOffset * (int)facingDirection, 0)


                // Apply camera position
                - GameManager.camera.Position,


                currentLocalRectangles[currentFrame],
                Color.White * opacity,
                0,
                Vector2.Zero,
                1,
                (facingDirection == CharacterEntity.FacingDirection.Right) ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                0);
        }
    }
}


//sb.Draw(
//                // Sprite sheet is the texture
//                spritesheet,

//                // Draw from the center of the hitbox, 
//                centerPosition
//                // but then offset by HALF the dimensions of the animation frame to center it
//                - new Vector2(frameDimensions.Item1 / 2, frameDimensions.Item2 / 2)
//                // and then correct it by the offset
//                + new Vector2(frameOffset.Item1 * -(int)facingDirection, frameOffset.Item2)

//                // Apply camera position
//                - GameManager.camera.Position,


//                currentLocalRectangles[currentFrame],
//                Color.White * opacity,
//                0,
//                Vector2.Zero,
//                1,
//                (facingDirection == CharacterEntity.FacingDirection.Right) ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
//                0);