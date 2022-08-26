using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Niraya
{
    /// <summary>
    /// A GUI sprite element which can be overlayed on top of the screen.
    /// </summary>
    class GUISprite : GUIElement
    {
        // FIELDS
        protected Texture2D sprite;
        protected bool isActive;


        // PROPERTIES
        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }

        // CONSTRUCTOR
        public GUISprite(Vector2 position, Texture2D sprite) : base(position)
        {
            this.sprite = sprite;
            isActive = false;
        }

        // METHODS
        public override void Draw(SpriteBatch sb)
        {
            if (isActive)
            {
                sb.Draw(sprite,
                        position,
                        Color.White);
            }
        }

        public override void Update(GameTime gameTime)
        {
          
        }
    }
}
