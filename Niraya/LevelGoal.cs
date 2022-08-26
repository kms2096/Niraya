using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Niraya
{
    public class LevelGoal : Entity
    {
        /// <summary>
        /// A type of LevelGoal
        /// </summary>
        public enum GoalType
        {
            /// <summary>
            /// This is a placeholder for what our normal level goal will be when it's not the dog
            /// </summary>
            NOT_IMPLEMENTED,

            /// <summary>
            /// The dog at the end of the game
            /// </summary>
            Dog
        }

        /// <summary>
        /// The GoalType that this LevelGoal is
        /// </summary>
        private GoalType goalType;

        public GoalType Type
        {
            get { return goalType; }
        }

        public LevelGoal(Point position, GoalType goalType)
            : base()
        {
            this.goalType = goalType;

            // Set up this collectible based on its type
            switch (goalType)
            {
                case GoalType.Dog:
                    // Texture
                    sprite = TextureManager.textures["Dog\\dog48x32_x2"];

                    // Position and size
                    hitbox = new Rectangle(
                        position,

                        // Hitbox size
                        new Point(96, 64));
                    break;

                case GoalType.NOT_IMPLEMENTED:
                    // Texture
                    sprite = TextureManager.textures["Dog\\dog48x32_x2"];

                    // Position and size
                    hitbox = new Rectangle(
                        position,

                        // Hitbox size
                        new Point(96, 64));
                    break;


                    // etc . . . add more goal types . . !
            }

            // Fix vertical offset
            hitbox.Y -= 12;
        }

        public override void Update(GameTime gameTime)
        {
            // Change animation frames -- CURRENTLY UNNEEDED

        }

        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            // Every quarter second, change frame
            if (((int)(gameTime.TotalGameTime.TotalSeconds * 8)) % 2 == 0)
            {
                sb.Draw(sprite,
                    Position - GameManager.camera.Position,
                    // TODO: Make this actually animate instead of hardcoding first frame
                    new Rectangle(0, 0, 96, 64),
                    Color.White);
            }
            else
            {
                sb.Draw(sprite,
                    Position - GameManager.camera.Position,
                    // TODO: Make this actually animate instead of hardcoding first frame
                    new Rectangle(96, 0, 96, 64),
                    Color.White);
            }
            

            if (GameManager.debugModeEnabled)
            {
                sb.Draw(TextureManager.textures["_debugging\\DebugSprite"], 
                    new Rectangle((Position - GameManager.camera.Position).ToPoint(), 
                    new Point(hitbox.Width, hitbox.Height)), 
                    Color.Red * .3f);
            }
        }
    }
}


