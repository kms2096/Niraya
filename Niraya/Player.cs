using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;

namespace Niraya
{
    /// <summary>
    /// A user-controllable character
    /// </summary>
	public class Player : CharacterEntity
    {
        // -- PLAYER CONSTANTS -- //

        /// <summary>
        /// Hitbox size (width and height)
        /// </summary>
        private readonly Point PlayerSize = new Point(45, 96);

        /// <summary>
        /// The attack hitbox, positioned at an offset of the normal hitbox
        /// </summary>
        private readonly Rectangle PlayerRelativeAttackHitbox =
            new Rectangle(
                // OFFSET from the regular hitbox
                -12, -24,

                // Attack hitbox width and height
                120, 120);

        /// <summary>
        /// Dimensions of a single frame on the spritesheet
        /// </summary>
        private readonly Point PlayerSpritesheetFrameDimensions = new Point(207, 132);

        /// <summary>
        /// How much to offset the animation on each spritesheet frame to center it
        /// </summary>
        private readonly Point PlayerSpritesheetFrameOffset = new Point(0, -12);

        /// <summary>
        /// Horizontal animation shift in the facing direction
        /// </summary>
        private const int PlayerSpritesheetFrameHorizontalFacingShift = 24;

        /// <summary>
        /// Starting health of the Player
        /// </summary>
        public const int PlayerDefaultHealth = 100;

        /// <summary>
        /// A current state of a player
        /// </summary>
        public enum PlayerState
        {
            /// <summary>
            /// Grounded state -- player is not moving
            /// </summary>
            Idle,

            /// <summary>
            /// Grounded state -- player is running left or right
            /// </summary>
            Running,

            /// <summary>
            /// Grounded state -- player is sliding across the ground
            /// </summary>
            Sliding,

            /// <summary>
            /// Midair state -- player is falling against a wall
            /// </summary>
            Wallsliding,

            /// <summary>
            /// Uncancellable state -- player dashes quickly a set distance to left or right
            /// </summary>
            Dashing,

            /// <summary>
            /// Uncancellable state -- player is doing a dashs attack
            /// </summary>
            DashAttacking,

            /// <summary>
            /// Uncancellable state -- player is in midair, either cause they jumped or otherwise
            /// </summary>
            Falling,

            /// <summary>
            /// Uncancellable state -- player is doing a grounded attack
            /// </summary>
            Attacking,

            /// <summary>
            /// Uncancellable state -- player is playing their hurt animation
            /// </summary>
            TakingDamage,

            /// <summary>
            /// Uncancellable state -- player is playing their death animation
            /// </summary>
            Dying,

            /// <summary>
            /// Uncancellable state -- player has died
            /// </summary>
            Dead,

            /// <summary>
            /// Uncancellable state -- player is coming back to life
            /// </summary>
            Reviving
        }

        /// <summary>
        /// Current state of this Player
        /// </summary>
        private PlayerState playerState;

        /// <summary>
        /// Starting Y velocity of a jump
        /// </summary>
        private const float JumpVelocity = 26;

        /// <summary>
        /// Base X velocity of running
        /// </summary>
        private const float RunVelocity = 8.3f;

        /// <summary>
        /// Base multiplier for knockback from taking damage
        /// </summary>
        private const float BaseKnockbackVelocity = 8.5f;

        /// <summary>
        /// The duration that the player is invulnerable for, converted from Milliseconds
        /// </summary>
        private readonly TimeSpan InvulnerabilityDuration = TimeSpan.FromSeconds(1); // 1 second of invulnerability

        /// <summary>
        /// The time at which the player last took damage and became invulnerable
        /// </summary>
        private TimeSpan invulnerabilityTime;

        /// <summary>
        /// Y velocity threshold of animation switch from jumping upward to falling downward
        /// </summary>
        private const float UpToFallVelocityThreshold = -3f;

        /// <summary>
        /// Max milliseconds between a keypress where a "double tap" will register
        /// </summary>
        private const float timeBetweenPress = 100;

        /// <summary>
        /// Getting the previous keyboard state
        /// </summary>
        private KeyboardState prevKBState;

        /// <summary>
        /// The current state of this Player
        /// </summary>
        public PlayerState CurrentPlayerState
        {
            get { return playerState; }
        }

        /// <summary>
        /// If the player is user-controllable (not dead/reviving)
        /// </summary>
        public bool IsControllable
        {
            get
            {
                return 
                    !(playerState == PlayerState.Dying ||
                    playerState == PlayerState.Dead ||
                    playerState == PlayerState.Reviving);
            }
        }

        // TODO: These properties expose the animation handler (unsafe), and are for debug purposes ONLY:
        /// <summary>
        /// ONLY USE FOR DEBUG PURPOSES
        /// </summary>
        public AnimationHandler AnimForDebug
        {
            get
            {
                return animationHandler;
            }
        }

        public Player(Point startingPosition) : base()
        {
            // Initialize hitbox
            hitbox = new Rectangle(startingPosition, PlayerSize);

            // Initialize attack hitbox
            relativeAttackHitbox = PlayerRelativeAttackHitbox;

            // Get texture
            sprite = TextureManager.textures["Player\\Warrior_Sheet-Effect_3x"];

            // Set health
            maxHealth = PlayerDefaultHealth;
            health = maxHealth;

            // Opaque
            spriteOpacity = 1;

            velocity = Vector2.Zero;

            // Face right
            facingDirection = FacingDirection.Right;

            // Start in Falling state
            playerState = PlayerState.Falling;

            isAttacking = false;

            //sets up run sound effect
            AudioManager.soundEffects["player footstep"].Volume = 0.22f;
            AudioManager.soundEffects["player footstep"].IsLooped = true;

            //sets up attack sound effects
            AudioManager.soundEffects["player sword attack 1"].Volume = 0.125f;
            AudioManager.soundEffects["player dash sword attack"].Volume = 0.25f;

            //sets up voiced sound effects (damage, dying)
            AudioManager.soundEffects["player damage 2"].Volume = 0.12f;
            AudioManager.soundEffects["player death 1"].Volume = 0.155f;

            prevKBState = Keyboard.GetState();

            // Create the animations
            InitializeAnimationHandler(sprite, PlayerSpritesheetFrameDimensions, PlayerSpritesheetFrameOffset, PlayerSpritesheetFrameHorizontalFacingShift);
        }

        // METHODS
        public void HandleAudio()
        {            
            // Run sound effect
            if (playerState == PlayerState.Running)
            {
                AudioManager.soundEffects["player footstep"].Play();
                AudioManager.soundEffects["player sword attack 1"].Stop();
                AudioManager.soundEffects["player damage 2"].Stop();
            }
            else if (playerState == PlayerState.Attacking)
            {
                AudioManager.soundEffects["player sword attack 1"].Play();
                AudioManager.soundEffects["player footstep"].Stop();
                AudioManager.soundEffects["player damage 2"].Stop();
            }
            else if(playerState == PlayerState.DashAttacking)
            {
                AudioManager.soundEffects["player dash sword attack"].Play();
                AudioManager.soundEffects["player footstep"].Stop();
                AudioManager.soundEffects["player damage 2"].Stop();
            }
            else if (playerState == PlayerState.Dying)
            {
                AudioManager.soundEffects["player death 1"].Play();
                AudioManager.soundEffects["player footstep"].Stop();
                AudioManager.soundEffects["player sword attack 1"].Stop();
                AudioManager.soundEffects["player damage 2"].Stop();

            }
            else if (playerState == PlayerState.TakingDamage)
            {
                AudioManager.soundEffects["player damage 2"].Play();
                AudioManager.soundEffects["player footstep"].Stop();
                AudioManager.soundEffects["player sword attack 1"].Stop();
            }
            else
            {
                AudioManager.soundEffects["player footstep"].Stop();
                AudioManager.soundEffects["player sword attack 1"].Stop();
                AudioManager.soundEffects["player damage 2"].Stop();
            }
        }

        /// <summary>
        /// Revives the player after their first death
        /// </summary>
        public void ReviveAfterDeath()
        {
            // Opaque
            spriteOpacity = 1;

            // Set max health
            health = maxHealth;

            // Start in reviving state
            playerState = PlayerState.Reviving;
        }

        /// <summary>
        /// Damages the Player, knocking them back and gives them temporary invulnerability, if they aren't already invulnerable
        /// </summary>
        /// <param name="damage">Damage to take</param>
        /// <param name="sourceLocation">Location of the damage source</param>
        public override void TakeDamage(GameTime gameTime, float damage, Vector2 sourceLocation)
        {
            // Check that the Invulnerability Duration has expired, and that godmode is disabled
            // Otherwise, nothing happens in this method
            if (!isInvulnerable && !GameManager.isGodmodeEnabled)
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

                // Check if player died
                if (health <= 0)
                {
                    // Ensure health stays at 0
                    health = 0;

                    // BIG camera shake
                    GameManager.camera.StartShake(100);

                    playerState = PlayerState.Dying;
                }
                else
                {
                    // Set the time at which we are invulnerable to be NOW-- since we just took damage.
                    invulnerabilityTime = gameTime.TotalGameTime;

                    // Camera shake
                    GameManager.camera.StartShake(7);

                    // Set state (damage overrides all states)
                    playerState = PlayerState.TakingDamage;
                }
            }
        }

        /// <summary>
        /// Heals the player the provided amount
        /// </summary>
        public void Heal(int amount)
        {
            // Increase health
            health += amount;

            // Cap health at the max value
            if (health > maxHealth)
            {
                health = maxHealth;
            }
        }

        /// <summary>
        /// Initializes Player animation dictionary and creates the animation handler
        /// </summary>
        protected override void InitializeAnimationHandler(
            Texture2D spritesheet,
            Point frameDimensions,
            Point frameOffset,
            int horizontalFrameOffset)
        {
            // Add all entries to the animation dictionary
            animationDict.Add("Idle", new int[] { 0, 1, 2, 3, 4, 5 });
            animationDict.Add("Run", new int[] { 6, 7, 8, 9, 10, 11, 12, 13 });
            animationDict.Add("ComboAttack", new int[] { 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 });
            animationDict.Add("Death", new int[] { 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 });
            animationDict.Add("Revival", new int[] { 36, 35, 34, 33, 32, 31, 30, 29, 28, 27, 26 }); // reverse of death
            animationDict.Add("DeadIdle", new int[] { 36 });
            animationDict.Add("Hurt", new int[] { 37, 38, 39, 40 });
            animationDict.Add("JumpUp", new int[] { 41, 42, 43 });
            animationDict.Add("UpToFall", new int[] { 44, 45 });
            animationDict.Add("Fall", new int[] { 46, 47, 48 });
            animationDict.Add("WallSlide", new int[] { 60, 61, 62 });
            animationDict.Add("Dash", new int[] { 69, 70, 71, 72, 73, 74, 75 });
            animationDict.Add("DashAttack", new int[] { 76, 77, 78, 79, 80, 81, 82, 83, 84, 85 });

            animationHandler = new AnimationHandler(spritesheet, frameDimensions, frameOffset, horizontalFrameOffset, animationDict);

            // Set starting animation! If this is not set then we will crash since there is no animation to play.
            animationHandler.CurrentAnimation = "Idle";
        }

        /// <summary>
        /// Draws the Player
        /// </summary>
        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            // If debug mode, draw debug hitboxes
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

            // If godmode, draw hitbox in gold color
            if (GameManager.isGodmodeEnabled)
            {
                //if (GameManager.debugModeEnabled)
                //{
                //    // Draw 3-pixel margin gold hitbox
                //    sb.Draw(GameManager.debugSprite, new Rectangle((Position - GameManager.camera.Position).ToPoint() - new Point(3, 3), new Point(hitbox.Width, hitbox.Height) + new Point(3, 3)), Color.Gold * .3f);
                //}
                //else
                //{

                // Draw normal-sized gold hitbox
                sb.Draw(TextureManager.textures["_debugging\\DebugSprite"], new Rectangle((Position - GameManager.camera.Position).ToPoint(), new Point(hitbox.Width, hitbox.Height)), Color.Gold * .3f);

                //}
            }

            // Draw animation
            animationHandler.Draw(sb, hitbox.Center.ToVector2(), facingDirection, spriteOpacity);
        }

        /// <summary>
        /// Updates the Player's state
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Store current state of the keyboard
            KeyboardState kbState = Keyboard.GetState();


            // Player physics
            Position += velocity;
            // Gravity
            velocity.Y += GameManager.DefaultGravity;

            // Cap the falling speed of the player
            if (velocity.Y > GameManager.MaxFallSpeed)
            {
                velocity.Y = GameManager.MaxFallSpeed;
            }

            // Set invulnerability status
            isInvulnerable = gameTime.TotalGameTime <= invulnerabilityTime + InvulnerabilityDuration;

            // Handle Audio
            HandleAudio();


            // Player FSM
            switch (playerState)
            {
                case PlayerState.Idle:

                    CheckLateralMovement(kbState);
                    CheckGroundedInputs(kbState, prevKBState);

                    animationHandler.CurrentAnimation = "Idle";

                    // Check if falling
                    if (isFalling)
                    {
                        playerState = PlayerState.Falling;
                    }
                    break;

                case PlayerState.Running:

                    CheckLateralMovement(kbState);
                    CheckGroundedInputs(kbState, prevKBState);

                    animationHandler.CurrentAnimation = "Run";

                    // Check if falling
                    if (isFalling)
                    {
                        playerState = PlayerState.Falling;
                    }
                    break;

                case PlayerState.Falling:

                    // Check lateral movement to allow air strafing
                    // This WILL change us out of the Falling state!
                    CheckLateralMovement(kbState);

                    // Check if we are still falling, if we aren't, then the above will put us in a grounded state
                    if (isFalling)
                    {
                        // Since the lateral movement check might change the state,
                        // change it back to the Falling state
                        playerState = PlayerState.Falling;
                    }


                    // Handle animations:

                    // If the player is falling down
                    if (velocity.Y >= 0)
                    {
                        // Use falling animation
                        animationHandler.CurrentAnimation = "Fall";
                    }
                    // If the player is jumping up and still moving faster than the UpToFall threshold
                    else if (velocity.Y < UpToFallVelocityThreshold)
                    {
                        // Use the jumping up animation
                        animationHandler.CurrentAnimation = "JumpUp";
                    }
                    // Otherwise, the player is jumping up but has hit the threshold and is therefore about to start falling
                    else
                    {
                        // Use the UpToFall animation
                        animationHandler.CurrentAnimation = "UpToFall";

                        // To add floaty-ness, effectively reduce gravity by 75% at the jump apex
                        velocity.Y -= (GameManager.DefaultGravity * .75f);
                    }

                    break;

                case PlayerState.Sliding:

                    CheckGroundedInputs(kbState, prevKBState);
                    CheckLateralMovement(kbState);

                    break;

                case PlayerState.Dashing:

                    animationHandler.CurrentAnimation = "Dash";

                    // If this is the first part of the dash
                    if (animationHandler.CurrentFrame < 2)
                    {
                        // Dash at high speed
                        velocity.X = RunVelocity * 2.5f * (int)facingDirection;

                        // Effectively reduce gravity by 75% during the dash
                        velocity.Y -= (GameManager.DefaultGravity * .75f);

                    }
                    // Second half of the dash (cooldown)
                    else
                    {
                        velocity.X *= .9f;

                        if (kbState.IsKeyDown(Keys.Enter))
                        {
                            playerState = PlayerState.DashAttacking;
                        }
                        else if (!isFalling)
                        {
                            CheckGroundedInputs(kbState, prevKBState);
                        }
                    }

                    if (animationHandler.CurrentFrame < 5)
                    {
                        isInvulnerable = true;
                    }

                    if (animationHandler.HasLoopedOnce)
                    {
                        CheckLateralMovement(kbState);
                    }

                    break;

                case PlayerState.DashAttacking:

                    // Rapidly slow down
                    velocity.X *= .7f;

                    animationHandler.CurrentAnimation = "DashAttack";

                    relativeAttackHitbox.Width = 150;
                    isAttacking = !(animationHandler.CurrentFrame < 3 || animationHandler.CurrentFrame > 5);

                    if (animationHandler.HasLoopedOnce)
                    {
                        isAttacking = false;

                        CheckLateralMovement(kbState);
                    }
                    break;

                case PlayerState.Attacking:

                    velocity.X = 0;

                    animationHandler.CurrentAnimation = "ComboAttack";

                    // Only attack if the Active Frames of the animation are playing
                    // TODO: This is hardcoded!
                    relativeAttackHitbox.Width = 120;
                    isAttacking = !(animationHandler.CurrentFrame < 3 || animationHandler.CurrentFrame > 9);

                    // Check if falling
                    if (isFalling)
                    {
                        playerState = PlayerState.Falling;
                    }
                    // Otherwise, check if loop has played once
                    else if (animationHandler.HasLoopedOnce)

                    {
                        isAttacking = false;

                        // Takes us out of the attacking state
                        CheckLateralMovement(kbState);
                    }

                    break;

                case PlayerState.TakingDamage:

                    isAttacking = false;

                    // Slow down horizontally
                    velocity.X *= .95f;

                    animationHandler.CurrentAnimation = "Hurt";

                    // Check if the damage animation has ended
                    if (animationHandler.HasLoopedOnce)
                    {
                        // Assume we are falling (to prevent jumping)
                        // If we aren't falling, then the falling state will move us to the correct state
                        playerState = PlayerState.Falling;
                    }

                    break;

                case PlayerState.Dying:
                    
                    if (!isFalling)
                    {
                        // Slow down horizontally
                        velocity.X *= .9f;
                    }

                    isAttacking = false;

                    animationHandler.CurrentAnimation = "Death";

                    if (animationHandler.HasLoopedOnce)
                    {
                        // Switch to dead state
                        playerState = PlayerState.Dead;
                    }
                    break;

                case PlayerState.Dead:

                    if (!isFalling)
                    {
                        // Slow down horizontally
                        velocity.X *= .7f;
                    }

                    isAttacking = false;

                    animationHandler.CurrentAnimation = "DeadIdle";

                    break;

                case PlayerState.Reviving:

                    velocity.X = 0;
                    isAttacking = false;

                    // Set the time at which we are invulnerable to be NOW
                    invulnerabilityTime = gameTime.TotalGameTime;

                    animationHandler.CurrentAnimation = "Revival";

                    if (animationHandler.HasLoopedOnce)
                    {
                        playerState = PlayerState.Falling;
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

            // TODO: Implement this better instead of retro-fixing it at the end of Update()
            // If this animation is the attack animation, increase the FPS so it's faster
            if (animationHandler.CurrentAnimation == "ComboAttack")
            {
                animationHandler.CurrentFps = 18;
            }
            // Death/revival in slow motion
            else if (
                animationHandler.CurrentAnimation == "Death" ||
                animationHandler.CurrentAnimation == "Revival")
            {
                animationHandler.CurrentFps = 8;
            }
            // Otherwise 12 fps
            else
            {
                animationHandler.CurrentFps = 12;
            }

            // Update animation
            animationHandler.UpdateAnimation(gameTime);


            // Store previous keyboard state
            prevKBState = kbState;
        }

        /// <summary>
        /// Check for left/right movement, always setting to a grounded state
        /// </summary>
        private void CheckLateralMovement(KeyboardState kbState)
        {
            // If both or neither lateral keys are pressed, do not move
            if (kbState.IsKeyDown(Keys.A) == kbState.IsKeyDown(Keys.D))
            {
                // Do not move laterally
                velocity.X = 0;

                // Idle state
                playerState = PlayerState.Idle;

                // Skip the rest of this method
                return;
            }

            // If A pressed
            if (kbState.IsKeyDown(Keys.A))
            {
                // Move left
                velocity.X = -RunVelocity;

                // Face left
                facingDirection = FacingDirection.Left;

                // Running state
                playerState = PlayerState.Running;
            }

            // If D pressed
            if (kbState.IsKeyDown(Keys.D))
            {
                // Move right
                velocity.X = RunVelocity;

                // Face right
                facingDirection = FacingDirection.Right;

                // Running state
                playerState = PlayerState.Running;

            }
        }

        /// <summary>
        /// Checks for inputs allowed while the Player is grounded
        /// </summary>
        private void CheckGroundedInputs(KeyboardState kbState, KeyboardState prevKBState)
        {
            // If Left Shift pressed
            if (kbState.IsKeyDown(Keys.LeftShift))
            {
                // Dashing state
                playerState = PlayerState.Dashing;

                // Skip the rest of this method
                return;
            }

            // If SPACEBAR pressed or W pressed
            if (kbState.IsKeyDown(Keys.Space) ||
                kbState.IsKeyDown(Keys.W))
            {
                // Jump
                velocity.Y = -JumpVelocity;

                playerState = PlayerState.Falling;
            }

            // If ENTER pressed
            if (GameManager.SingleKeyPress(kbState, prevKBState, Keys.Enter))
            {
                playerState = PlayerState.Attacking;

                // Skip the rest of this method
                return;
            }

            // If S pressed
            if (kbState.IsKeyDown(Keys.S))
            {
                // Move camera target position downward
                GameManager.camera.TargetPosition += new Vector2(0, 250);
            }
        }
    }
}

