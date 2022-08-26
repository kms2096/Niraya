using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Niraya
{
    /// <summary>
    /// An object in a level with a world-positioned hitbox and a texture
    /// </summary>
    public abstract class Entity
    {
        // FIELDS

        /// <summary>
        /// Texture used for all draw calls
        /// </summary>
        protected Texture2D sprite;

        /// <summary>
        /// The world-positioned bounding box of this entity
        /// </summary>
        protected Rectangle hitbox;

        // PROPERTIES

        /// <summary>
        /// The Vector2 position of this Entity's hitbox
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return new Vector2(hitbox.X, hitbox.Y);
            }
            set 
            {
                hitbox.X = (int)value.X;
                hitbox.Y = (int)value.Y;
            }
        }

        public Rectangle Hitbox
        {
            get { return hitbox; }
        }

        // CONSTRUCTOR
        public Entity ()
        {
            // TODO: Entities should probably require a position to be passed in, though it would need to be implemented well
        }

        // METHODS

        // Can be overridden
        public virtual void Draw(GameTime gameTime, SpriteBatch sb)
        {
            sb.Draw(sprite,
                    Position -= GameManager.camera.Position,
                    Color.White);
        }

        public abstract void Update(GameTime gameTime);
    }
}
