using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Niraya
{
    /// <summary>
    /// Represents the tiles on a tile sheet
    /// </summary>
    public class Tileset
    {
        private Texture2D tilesheet;
        private int tileSize;
        private int numCols;
        private int numRows;

        public Texture2D Tilesheet
        {
            get { return tilesheet; }
        }


        /// <summary>
        /// Creates a Tileset with the provided tilesheet texture and the size of a single tile, in pixels
        /// </summary>
        /// <param name="tilesheet">Must have dimensions are that are factors of tileSize</param>
        /// <param name="tileSize">Width/height of a given tile, in pixels</param>
        public Tileset(Texture2D tilesheet, int tileSize)
        {
            this.tilesheet = tilesheet;
            this.tileSize = tileSize;

            // Ensure that tilesheet has dimensions that are a factor of tileSize
            if ((tilesheet.Width % tileSize != 0) ||
                (tilesheet.Height % tileSize != 0))
            {
                throw new InvalidOperationException("tilesheet must have dimensions that are a factor of tileSize");
            }

            numCols = tilesheet.Width / tileSize;
            numRows = tilesheet.Height / tileSize;
        }

        /// <summary>
        /// Provided with the index of a given tile, will return the Rectangle of that tile's position in the tilesheet
        /// </summary>
        public Rectangle GetTilesheetLocalRect(int index)
        {
            // Ensure that index of is in range of possible indices
            if (index < 0 || index > (numCols * numRows - 1))
            {
                throw new IndexOutOfRangeException($"Index {index} out of range 0-{numCols * numRows - 1}");
            }

            // Find how far across and down this tile is
            int tileCol = index % numCols;
            int tileRow = index / numCols;

            //Debug.WriteLine($"Tile with index {index} -- ({tileCol}, {tileRow})");

            // Return the rectangle with the correct top left coordinates,
            // and the correct size
            return new Rectangle(
                tileSize * tileCol,
                tileSize * tileRow,
                tileSize,
                tileSize);

            //return new Rectangle(0,0,tileSize, tileSize);
        }

    }
}
