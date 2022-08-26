using System;
using System.Collections.Generic;
using System.Text;

namespace Niraya
{
    /// <summary>
    /// Contains the indices needed to locate and iterate through a given region on a 2D tilemap
    /// </summary>
    struct TilemapRegion
    {
        public readonly int StartingCol;
        public readonly int EndingCol;

        public readonly int StartingRow;
        public readonly int EndingRow;

        public TilemapRegion(int startingCol, int endingCol, int startingRow, int endingRow)
        {
            this.StartingCol = startingCol;
            this.EndingCol = endingCol;

            this.StartingRow = startingRow;
            this.EndingRow = endingRow;
        }
    }
}
