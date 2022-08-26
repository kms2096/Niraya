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
    /// An element that is positioned in screen space, existing outside the level
    /// </summary>
    abstract class GUIElement
    {
        protected Vector2 position;

        /// <summary>
        /// The position of the GUIElement on the screen.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
        }

        /// <summary>
        /// Creates an abstract GUIElement
        /// </summary>
        public GUIElement(Vector2 position)
        {
            this.position = position;
        }

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch sb);

    }
}
