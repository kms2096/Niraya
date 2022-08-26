using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Niraya
{
    /// <summary>
    /// A knight enemy that paces back and forth, and attacks the Player in melee range
    /// </summary>
    public class EnemyKnight : Enemy
    {
        // -- ENEMY CONSTANTS -- //

        /// <summary>
        /// Hitbox size (width and height)
        /// </summary>
        private readonly Point EnemySize = new Point(60, 99);

        /// <summary>
        /// The attack hitbox, positioned at an offset of the normal hitbox
        /// </summary>
        private readonly Rectangle EnemyRelativeAttackHitbox =
            new Rectangle(
                // Offset of attack hitbox
                0, 0,

                // Width and height of attack hitbox
                60, 99);

        /// <summary>
        /// Dimensions of a single frame on the spritesheet
        /// </summary>
        private readonly Point EnemySpritesheetFrameDimensions = new Point(360, 240);

        /// <summary>
        /// How much to offset the animation on each spritesheet frame to center it
        /// </summary>
        private readonly Point EnemySpritesheetFrameOffset = new Point(0, -70);

        /// <summary>
        /// Horizontal animation shift in the facing direction
        /// </summary>
        private const int EnemySpritesheetFrameHorizontalFacingShift = 9;

        /// <summary>
        /// An exclusive state an EnemyKnight can be in
        /// </summary>
        public enum EnemyState
        {
            Idle,
            Walk,
            Rolling,
            Falling,
            Attacking,
            TakingDamage,
            Dying,
            Dead
        }

        /// <summary>
        /// The type of knight that this is
        /// </summary>
        public enum KnightType
        {
            Basic,
            Advanced
        }

        /// <summary>
        /// Base walking speed on ground
        /// </summary>
        private readonly float BaseWalkSpeed;

        /// <summary>
        /// Base multiplier for knockback from taking damage
        /// </summary>
        private const float BaseKnockbackVelocity = 8.5f;

        /// <summary>
        /// Y velocity threshold of animation switch from jumping upward to falling downward
        /// </summary>
        private const float UpToFallVelocityThreshold = -3f;

        /// <summary>
        /// Chance the knight will roll after changing direction
        /// </summary>
        private const float ChanceToRoll = .12f;

        /// <summary>
        /// The duration that the EnemyKnight is invulnerable for, converted from Milliseconds
        /// </summary>
        private readonly TimeSpan InvulnerabilityDuration = TimeSpan.FromSeconds(.95f);

        /// <summary>
        /// The time at which the enemy last took damage and became invulnerable
        /// </summary>
        private TimeSpan invulnerabilityTime;

        /// <summary>
        /// Minimum duration of time a knight will spend walking in a given direction
        /// </summary>
        private readonly TimeSpan MinDirectionalWalkDuration = TimeSpan.FromSeconds(.7f);

        /// <summary>
        /// The time at which the knight last changed direction
        /// </summary>
        private TimeSpan lastDirectionChangeTime;

        /// <summary>
        /// State of this EnemyKnight
        /// </summary>
        private EnemyState enemyState;

        /// <summary>
        /// Type of knight that this EnemyKnight is
        /// </summary>
        private KnightType knightType;

        public EnemyKnight(Point startingPosition, KnightType knightType) : base()
        {
            this.knightType = knightType;

            // Initialize hitbox
            hitbox = new Rectangle(startingPosition, EnemySize);

            // Initialize attack hitbox
            relativeAttackHitbox = EnemyRelativeAttackHitbox;

            spriteOpacity = 1;

            facingDirection = FacingDirection.Left;

            velocity = Vector2.Zero;

            enemyState = EnemyState.Falling;

            isAttacking = false;

            // Set up this knight based on its type
            switch (knightType)
            {
                // Beige knight
                case KnightType.Basic:
                    BaseWalkSpeed = 4.1f;
                    maxHealth = 50;

                    sprite = TextureManager.textures["Enemy\\knight_beige_3x"];

                    break;

                // Silver knight
                case KnightType.Advanced:
                    BaseWalkSpeed = 4.5f;
                    maxHealth = 100;

                    sprite = TextureManager.textures["Enemy\\knight_3x"];

                    break;
            }

            health = maxHealth;

            //sets up attack sound effect
            AudioManager.soundEffects["knight sword attack 01"].Volume = .14f;

            // Create the animations
            InitializeAnimationHandler(sprite, EnemySpritesheetFrameDimensions, EnemySpritesheetFrameOffset, EnemySpritesheetFrameHorizontalFacingShift);
        }

        /// <summary>
        /// Damages the EnemyKnight, knocking them back and gives them temporary invulnerability, if they aren't already invulnerable
        /// </summary>
        /// <param name="damage">Damage to take</param>
        /// <param name="sourceLocation">Location of the damage source</param>
        public override void TakeDamage(GameTime gameTime, float damage, Vector2 sourceLocation)
        {
            // Check that the Invulnerability Duration has expired
            // Otherwise, nothing happens in this method
            if (!isInvulnerable)
            {
                // Deal damage
                health -= damage;

                // Horizontal knockback
                velocity.X =
                    BaseKnockbackVelocity *
                    // Knockback velocity points away from the damage source location
                    Math.Sign((hitbox.Center.ToVector2() - sourceLocation).X) *
                    // Scale knockback with percent of max health that is removed
                    (1 + (damage / (float)maxHealth));

                // Vertical knockback always lauches upward
                velocity.Y = -BaseKnockbackVelocity;

                // Check if EnemyKnight died
                if (health <= 0)
                {
                    // Ensure health stays at 0
                    health = 0;

                    enemyState = EnemyState.Dying;
                }
                else
                {
                    // Set state (damage state overrides all states)
                    enemyState = EnemyState.TakingDamage;

                    // Set the time at which we are invulnerable to be NOW-- since we just took damage.
                    invulnerabilityTime = gameTime.TotalGameTime;
                }
            }
        }

        /// <summary>
        /// Causes this EnemyKnight to attack
        /// </summary>
        /// <returns>Damage dealt from this attack</returns>
        public override float Attack()
        {
            enemyState = EnemyState.Attacking;
            return 20;
        }

        protected override void InitializeAnimationHandler(
            Texture2D spritesheet,
            Point frameDimensions,
            Point frameOffset,
            int horizontalFrameOffset)
        {
            // Add all animations
            animationDict.Add("Walk", new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            animationDict.Add("Roll", new int[] { 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 });
            animationDict.Add("Attack", new int[] { 24, 25, 26, 27 });
            animationDict.Add("Death", new int[] { 36, 37, 38, 39, 40, 41, 42, 43, 44, 45 });
            animationDict.Add("Fall", new int[] { 48, 49, 50 });
            animationDict.Add("UpToFall", new int[] { 51, 52 });
            animationDict.Add("Jump", new int[] { 53, 54, 55 });
            animationDict.Add("DeadIdle", new int[] { 45 });
            animationDict.Add("Hurt", new int[] { 47, 47, 47, 47, 47, 47, 47, 47, 47 });
            animationDict.Add("FunnyCrouchWalk", new int[] { 108, 109, 110, 111, 112, 113, 114, 115 });
            animationDict.Add("CrouchTransition", new int[] { 116 });
            animationDict.Add("CrouchAttack", new int[] { 56, 57, 58, 59 });
            animationDict.Add("Idle", new int[] { 60, 61, 62, 63, 64, 65, 66, 67, 68, 69 });
            animationDict.Add("LeftToRightTurn", new int[] { 117, 118, 119 });

            animationHandler = new AnimationHandler(spritesheet, frameDimensions, frameOffset, horizontalFrameOffset, animationDict);

            // Set starting animation! If this is not set then we will crash since there is no animation to play.
            animationHandler.CurrentAnimation = "DeadIdle";
        }


        public override void Update(GameTime gameTime)
        {
            // Physics
            Position += velocity;
            // Gravity
            velocity.Y += GameManager.DefaultGravity;

            // Cap the falling speed of the EnemyKnight
            if (velocity.Y > GameManager.MaxFallSpeed)
            {
                velocity.Y = GameManager.MaxFallSpeed;
            }

            // Set invulnerability status
            isInvulnerable = gameTime.TotalGameTime <= invulnerabilityTime + InvulnerabilityDuration;

            // EnemyKnight FSM
            switch (enemyState)
            {
                case EnemyState.Walk:
                    isAttacking = true;

                    if (
                        // If there are tiles in the way, turn-- OR
                        isObstructed ||
                        // If we're on an edge, and it's been long enough since we last changed direction 
                        (isOnLedge &&
                        gameTime.TotalGameTime > lastDirectionChangeTime + MinDirectionalWalkDuration))
                    {
                        // Face the other direction
                        facingDirection = (FacingDirection)(-(int)facingDirection);

                        // Set the time at which we last changed direction
                        lastDirectionChangeTime = gameTime.TotalGameTime;

                        // Random chance to roll
                        if (Game1.rng.NextDouble() < ChanceToRoll)
                        {
                            enemyState = EnemyState.Rolling;
                        }
                    }

                    velocity.X = BaseWalkSpeed * (int)facingDirection;

                    if (isFalling)
                    {
                        enemyState = EnemyState.Falling;
                    }

                    // Set animation to Walk
                    if (GameManager.dimensionState == GameManager.DimensionState.Overworld)
                    {
                        animationHandler.CurrentAnimation = "Walk";
                    }
                    else
                    {
                        animationHandler.CurrentAnimation = "FunnyCrouchWalk";
                    }

                    break;

                case EnemyState.Rolling:
                    isAttacking = false;

                    // If this is the first part of the roll
                    if (animationHandler.CurrentFrame < 8)
                    {
                        // Speed up for the roll
                        velocity.X = BaseWalkSpeed * (int)facingDirection * 1.5f;

                        isInvulnerable = true;
                    }
                    else
                    {
                        velocity.X *= .9f;
                    }


                    animationHandler.CurrentAnimation = "Roll";

                    if (animationHandler.HasLoopedOnce)
                    {
                        if (isFalling)
                        {
                            enemyState = EnemyState.Falling;
                        }
                        else
                        {
                            enemyState = EnemyState.Walk;
                        }
                    }
                    break;

                case EnemyState.TakingDamage:
                    isAttacking = false;

                    // Slow down horizontally
                    velocity.X *= .95f;

                    animationHandler.CurrentAnimation = "Hurt";

                    // Check if the hurt animation has ended AND the EnemyKnight has hit the ground
                    if (animationHandler.HasLoopedOnce && !isFalling)
                    {
                        enemyState = EnemyState.Walk;
                    }

                    break;

                case EnemyState.Falling:
                    // Handle animations:

                    // If the player is falling toward the ground
                    if (velocity.Y >= 0)
                    {
                        // Use falling animation
                        animationHandler.CurrentAnimation = "Fall";
                    }
                    // If the player is jumping up and still moving faster than the UpToFall threshold
                    else if (velocity.Y < UpToFallVelocityThreshold)
                    {
                        // Use the jumping animation
                        animationHandler.CurrentAnimation = "Jump";
                    }
                    // Otherwise, the player is jumping up but has hit the threshold and is therefore about to start falling
                    else
                    {
                        // Use the UpToFall animation
                        animationHandler.CurrentAnimation = "UpToFall";
                    }

                    if (!isFalling)
                    {
                        enemyState = EnemyState.Walk;
                    }

                    break;

                case EnemyState.Dying:
                    isAttacking = false;

                    if (!isFalling)
                    {
                        // Slow down horizontally
                        velocity.X *= .9f;
                    }
                    else
                    {
                        // Slight slow down if in midair
                        velocity.X *= .95f;
                    }

                    animationHandler.CurrentAnimation = "Death";

                    if (animationHandler.HasLoopedOnce)
                    {
                        enemyState = EnemyState.Dead;
                    }

                    break;

                case EnemyState.Dead:
                    isAttacking = false;

                    if (!isFalling)
                    {
                        // Slow down horizontally
                        velocity.X *= .7f;
                    }
                    else
                    {
                        // Slight slow down if in midair
                        velocity.X *= .95f;
                    }

                    animationHandler.CurrentAnimation = "DeadIdle";

                    break;

                case EnemyState.Attacking:
                    AudioManager.soundEffects["knight sword attack 01"].Play();
                    isAttacking = true;

                    // Stops moving
                    velocity.X = 0;

                    // Play appropriate attack animation
                    if (GameManager.dimensionState == GameManager.DimensionState.Overworld)
                    {
                        animationHandler.CurrentAnimation = "Attack";
                    }
                    else
                    {
                        animationHandler.CurrentAnimation = "CrouchAttack";
                    }

                    if (animationHandler.HasLoopedOnce)
                    {
                        enemyState = EnemyState.Walk;
                    }

                    break;
            }

            // If invulnerable
            if (isInvulnerable)
            {
                // Flash when invulnerable by changing opacity every other frame 
                if (animationHandler.CurrentFrame % 2 == 0)
                {
                    spriteOpacity = .5f;
                }
                else
                {
                    spriteOpacity = 1;
                }
            }
            else
            {
                spriteOpacity = 1;
            }

            // Update the animation
            animationHandler.UpdateAnimation(gameTime);
        }

        /// <summary>
        /// Draws the EnemyKnights
        /// </summary>
        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {

            if (GameManager.debugModeEnabled)
            {
                // Draw hitbox
                sb.Draw(TextureManager.textures["_debugging\\DebugSprite"], new Rectangle((Position - GameManager.camera.Position).ToPoint(), new Point(hitbox.Width, hitbox.Height)), Color.Red * .3f);

                // Draw attack hitbox if we are attacking
                if (isAttacking)
                {
                    sb.Draw(TextureManager.textures["_debugging\\DebugSprite"], new Rectangle(AttackHitbox.Location - GameManager.camera.Position.ToPoint(), new Point(AttackHitbox.Width, AttackHitbox.Height)), Color.MediumPurple * .3f);
                }
            }

            // Draw animation
            animationHandler.Draw(sb, hitbox.Center.ToVector2(), facingDirection, spriteOpacity);
        }
    }
}
