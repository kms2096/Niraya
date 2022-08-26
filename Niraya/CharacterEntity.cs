using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Niraya
{
    /// <summary>
    /// An Entity that represents a character in the level
    /// </summary>
    public abstract class CharacterEntity : Entity
    {
        /// <summary>
        /// A direction that a sprite can be horizontally oriented in (either 1 or -1)
        /// </summary>
        public enum FacingDirection
        {
            Right = 1,
            Left = -1
        }

        // FIELDS

        /// <summary>
        /// Distance this CharacterEntity moves per frame
        /// </summary>
        protected Vector2 velocity;

        /// <summary>
        /// Change in velocity per frame
        /// </summary>
        protected Vector2 acceleration;

        /// <summary>
        /// The region of space in which this character deals damage, at a location offset from the Hitbox
        /// </summary>
        protected Rectangle relativeAttackHitbox;

        /// <summary>
        /// Number of health points this Entity possesses
        /// </summary>
        protected float health;

        /// <summary>
        /// The max health of an EnemySkeleton
        /// </summary>
        protected float maxHealth;

        /// <summary>
        /// If this CharacterEntity is immune to taking damage
        /// </summary>
        protected bool isInvulnerable;

        /// <summary>
        /// If this CharacterEntity is currently falling
        /// </summary>
        protected bool isFalling;

        /// <summary>
        /// If this CharacterEntity is dealing damage from its AttackHitbox
        /// </summary>
        protected bool isAttacking;

        /// <summary>
        /// Direction the CharacterEntity is facing
        /// </summary>
        protected FacingDirection facingDirection;

        /// <summary>
        /// Opacity of the Player sprite when drawn
        /// </summary>
        protected float spriteOpacity;

        /// <summary>
        /// The animations for this CharacterEntity
        /// </summary>
        protected AnimationHandler animationHandler;

        /// <summary>
        /// Stores animation indices as keys and an array of its respective animation frames as values
        /// </summary>
        protected Dictionary<string, int[]> animationDict;

        // PROPERTIES
        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        public FacingDirection CurrentFacingDirection
        {
            get { return facingDirection; }
        }

        public bool IsInvulnerable
        {
            get { return isInvulnerable; }
        }
        public float MaxHealth
        {
            get { return maxHealth; }
        }

        /// <summary>
        /// Hitbox this entity uses for attacking
        /// </summary>
        public Rectangle AttackHitbox
        {
            // Converts the relative attack hitbox to the ACTUAL world positioned attack hitbox
            get
            {
                // Flips the orientation of the attack hitbox
                if (facingDirection == FacingDirection.Right)
                {
                    return new Rectangle(
                        hitbox.Location + relativeAttackHitbox.Location,

                        new Point(relativeAttackHitbox.Width, relativeAttackHitbox.Height)
                    );
                }
                else
                {
                    return new Rectangle(
                        hitbox.Location + 
                        new Point(
                            -relativeAttackHitbox.Location.X - relativeAttackHitbox.Width + hitbox.Width,
                            relativeAttackHitbox.Location.Y),

                        new Point(relativeAttackHitbox.Width, relativeAttackHitbox.Height)
                    );
                }
            }
        }

        public float Health
        {
            get { return health; }
            //set { health = value; } // Possibly delete????
        }

        /// <summary>
        /// If this CharacterEntity is alive
        /// </summary>
        public bool IsAlive
        {
            get { return health > 0; }
        }

        /// <summary>
        /// If this CharacterEntity is dealing damage from its AttackHitbox
        /// </summary>
        public bool IsAttacking
        {
            get { return isAttacking; }
        }

        /// <summary>
        /// Whether or not the player is colliding with the level
        /// </summary>
        public bool IsFalling
        {
            get { return isFalling; }
            set { isFalling = value; }
        }

        // CONSTRUCTOR
        public CharacterEntity() : base()
        {
            isAttacking = false;

            // Start not moving
            velocity = Vector2.Zero;
            acceleration = Vector2.Zero;

            // Opaque
            spriteOpacity = 1;

            // Create the animations
            animationDict = new Dictionary<string, int[]>();
        }

        // METHODS

        public abstract void TakeDamage(GameTime gameTime, float damage, Vector2 sourceLocation);

        /// <summary>
        /// Sets up this CharacterEntity's animation dictionary and initializes their AnimationHandler
        /// </summary>
        protected abstract void InitializeAnimationHandler(
            Texture2D spritesheet,
            Point frameDimensions,
            Point frameOffset,
            int horizontalFrameOffset);


        // TODO: Basic physics should be implemented in an Update() override method HERE, not the individual character entities like Player and Enemy
        // That way, CharacterEntities are an actual physically-based simulation globally
    }
}
