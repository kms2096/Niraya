using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Niraya
{
    /// <summary>
    /// A current world location from which the game is drawn. Can be translated but not scaled.
    /// </summary>
    public class Camera
    {
        /// <summary>
        /// The top left corner position of this camera
        /// </summary>
        private Vector2 position;

        private Vector2 targetPosition;
        private Vector2 velocity;

        private Vector2 shakeVelocity;

        /// <summary>
        /// Left and right bounding X positions of this camera
        /// </summary>
        private (int, int) horizontalBounds;

        /// <summary>
        /// How much to divide the distance from the camera to the target by, when setting camera velocity
        /// </summary>
        private const float TargetVelocityDivisor = 40.0f;

        /// <summary>
        /// Distance target must be for camera to start following
        /// </summary>
        private const int MinimumTargetDistanceThreshold = 10;

        /// <summary>
        /// Current world position (top left corner of the camera)
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set
            {
                // Keep camera position in-bounds
                position.X = Math.Clamp(value.X, horizontalBounds.Item1, horizontalBounds.Item2);
                position.Y = value.Y;
            }
        }

        /// <summary>
        /// Current target location (top left corner of the camera), where it will smoothly glide to
        /// </summary>
        public Vector2 TargetPosition
        {
            get { return targetPosition; }
            set
            {
                // Keep camera target position in-bounds
                targetPosition.X = Math.Clamp(value.X, horizontalBounds.Item1, horizontalBounds.Item2);
                targetPosition.Y = value.Y;
            }
        }

        /// <summary>
        /// Sets the left and right bounding X positions of the camera
        /// </summary>
        public (int, int) HorizontalBounds
        {
            get { return horizontalBounds; }
            set { horizontalBounds = value; }
        }

        /// <summary>
        /// Creates and returns a transformation representing the virtual offset + scaling of the camera
        /// </summary>
        private Matrix Matrix // Set to private to disable it -- this method CAN be deleted if we decide it's unneeded
        {
            get
            {
                return Matrix.CreateTranslation((int)-position.X, (int)-position.Y, 0);
            }
        }

        /// <summary>
        /// Creates a new Camera object at (0, 0)
        /// </summary>
        public Camera()
        {
            // Zero out all vectors
            position = Vector2.Zero;
            targetPosition = Vector2.Zero;
            velocity = Vector2.Zero;
            shakeVelocity = Vector2.Zero;

            horizontalBounds = (0, 0);
        }

        /// <summary>
        /// Move camera to follow current position
        /// </summary>
        public void Update()
        {
            if (Vector2.Distance(position, targetPosition) > MinimumTargetDistanceThreshold)
            {
                velocity = (targetPosition - position) / TargetVelocityDivisor;

                // Physics
                position += velocity;

                // Set camera bounds
                position.X = Math.Clamp(position.X, horizontalBounds.Item1, horizontalBounds.Item2);
            }

            if (shakeVelocity.Length() > 1)
            {
                position += shakeVelocity;
                shakeVelocity *= -.9f;
            }
            else
            {
                shakeVelocity = Vector2.Zero;
            }
        }

        /// <summary>
        /// Starts a camera shake with the provided starting velocity
        /// </summary>
        public void StartShake(int startingVelocity)
        {
            // Pick a random direction in radians
            double angle = Game1.rng.NextDouble() * 2 * Math.PI;

            // Create a vector with that random direction and the provided magnitude
            shakeVelocity = startingVelocity * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }
    }
}
