using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Niraya
{
    /// <summary>
    /// Represents a tile in a level
    /// </summary>
    public struct Tile
    {
        /// <summary>
        /// The numeric ID of this tile
        /// </summary>
        private int id;

        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        public Tile(int id)
        {
            this.id = id;
        }
    }
}
