using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Niraya
{
    /// <summary>
    /// A CharacterEntity with basic AI that serves as an obstacle for the Player
    /// </summary>
    public abstract class Enemy : CharacterEntity
    {
        /// <summary>
        /// If this Enemy is obstructed from moving in the direction they face
        /// </summary>
        protected bool isObstructed;

        /// <summary>
        /// If this Enemy is on a ledge (i.e. if they walk forward they will fall)
        /// </summary>
        protected bool isOnLedge;

        public bool IsObstructed
        {
            get { return isOnLedge; }
            set { isObstructed = value; }
        }

        public bool IsOnLedge
        {
            get { return isOnLedge; }
            set { isOnLedge = value; }
        }

        public Enemy() : base()
        {
        }

        public abstract float Attack();
    }
}
