using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Niraya
{
    /// <summary>
    /// Handles all collisions between entities, and with the level tilemap
    /// </summary>
    public class CollisionManager
    {

        /// <summary>
        /// Tile IDs on which to check collision
        /// </summary>
        private readonly int[] HitboxTileIDs =
        {
            0, 1, 2, 26, 28, 52, 53, 54
        };

        /// <summary>
        /// Current level to be used for collisions
        /// </summary>
        private Level currentLevel;

        public Level CurrentLevel
        {
            set { currentLevel = value; }
        }

        /// <summary>
        /// Creates a CollisionManager
        /// </summary>
        public CollisionManager()
        {
        }

        /// <summary>
        /// Returns the starting/ending column indices and the starting/ending row indices of the tilemap region that intersects the provided hitbox
        /// </summary>
        private TilemapRegion GetIterationRegion(Rectangle hitbox)
        {
            // Find the correct ranges of values
            int startingCol = (int)(hitbox.X / GameManager.TileSize);
            int endingCol = (int)((hitbox.X + hitbox.Width) / GameManager.TileSize);
            // TODO: This is a hacky fix!! -- we subtract 1 from startingRow so there is an additional layer of tiles that is checked vertically (without this check there are glitches)
            int startingRow = (int)(hitbox.Y / GameManager.TileSize) - 1; 
            int endingRow = (int)((hitbox.Y + hitbox.Height) / GameManager.TileSize);

            // Clamp these to valid indices in the current level
            startingCol = Math.Clamp(startingCol, 0, currentLevel.TileMapWidth);
            endingCol = Math.Clamp(endingCol, 0, currentLevel.TileMapWidth);

            startingRow = Math.Clamp(startingRow, 0, currentLevel.TileMapHeight);
            endingRow = Math.Clamp(endingRow, 0, currentLevel.TileMapHeight);

            return new TilemapRegion(startingCol, endingCol, startingRow, endingRow);
        }

        /// <summary>
        /// Checks for collisions between a CharacterEntity and a tilemap, and iteratively resolves these for each found collision
        /// </summary>
        public void ResolveTileCollisions(CharacterEntity entity)
        {
            // Assume falling
            entity.IsFalling = true;

            TilemapRegion region =
                // Get the tilemap region in which the entity actually exists
                GetIterationRegion(
                    // Create a copy of the entity hitbox
                    new Rectangle(

                        // Offset this copied hitbox's location by the entity's velocity,
                        // so we are checking the TilemapRegion where the entity WILL be on the NEXT frame
                        entity.Hitbox.Location + entity.Velocity.ToPoint(),

                        entity.Hitbox.Size)
                    );

            // Iterate through each tile in the tilemap array
            for (int row = region.StartingRow; row <= region.EndingRow; row++)
            {
                for (int col = region.StartingCol; col <= region.EndingCol; col++)
                {
                    // Get this tile's ID
                    int tileID = currentLevel.GetTileID(col, row);

                    // If this tile has no hitbox, skip it
                    if (!HitboxTileIDs.Contains(tileID))
                    {
                        continue;
                    }

                    // Get the hitbox for the tile
                    Rectangle tileHitbox = currentLevel.GetTileHitbox(col, row);

                    // Check if the hitboxes would collide horizontally on the next frame (using the X velocity of the entity)
                    if (entity.Hitbox.X + entity.Velocity.X < tileHitbox.X + tileHitbox.Width &&
                        entity.Hitbox.X + entity.Hitbox.Width + entity.Velocity.X > tileHitbox.X &&
                        entity.Hitbox.Y < tileHitbox.Y + tileHitbox.Height &&
                        entity.Hitbox.Y + entity.Hitbox.Height > tileHitbox.Y)
                    {
                        // If the entity is moving right
                        if (entity.Velocity.X > 0)
                        {
                            entity.Position = new Vector2(
                                // Move entity to be exactly adjacent to the right of the tile hitbox
                                tileHitbox.X - entity.Hitbox.Width,

                                // No change in Y
                                entity.Position.Y);
                        }
                        // If the entity is moving left
                        else
                        {
                            entity.Position = new Vector2(
                                // Move entity to be exactly adjacent to the left of the tile hitbox
                                tileHitbox.X + tileHitbox.Width,

                                // No change in Y
                                entity.Position.Y);
                        }

                        // Set entity's X velocity to 0
                        entity.Velocity = new Vector2(0, entity.Velocity.Y);
                    }

                    // Check if the hitboxes would collide vertically on the next frame (using the Y velocity of the entity)
                    else if (
                        entity.Hitbox.X < tileHitbox.X + tileHitbox.Width &&
                        entity.Hitbox.X + entity.Hitbox.Width > tileHitbox.X &&
                        entity.Hitbox.Y + entity.Velocity.Y < tileHitbox.Y + tileHitbox.Height &&
                        entity.Hitbox.Y + entity.Hitbox.Height + entity.Velocity.Y > tileHitbox.Y)
                    {
                        // If the entity is moving down
                        if (entity.Velocity.Y > 0)
                        {
                            entity.Position = new Vector2(
                                // No change in X
                                entity.Position.X,

                                // Move entity to be exactly adjacent to the top of the tile hitbox
                                tileHitbox.Y - entity.Hitbox.Height);

                            entity.IsFalling = false;
                        }
                        // If the entity is moving up
                        else
                        {
                            entity.Position = new Vector2(
                                // No change in X
                                entity.Position.X,

                                // Move entity to be exactly adjacent to the bottom of the tile hitbox
                                tileHitbox.Y + tileHitbox.Height);
                        }

                        // Set entity's Y velocity to 0
                        entity.Velocity = new Vector2(entity.Velocity.X, 0);
                    }
                }
            }
        }

        /// <summary>
        /// Returns true for a collision between an attacking CharacterEntity's attack hitbox and the target's hitbox, if both are alive.
        /// </summary>
        public bool CheckAttackFromCharacter(CharacterEntity attacker, CharacterEntity target)
        {
            return
                attacker.AttackHitbox.Intersects(target.Hitbox) &&
                attacker.IsAttacking &&
                attacker.IsAlive &&
                target.IsAlive &&
                !target.IsInvulnerable;
        }

        /// <summary>
        /// Checks for any tiles that would obstruct lateral motion in front of the CharacterEntity, or a lack of tiles underneath (i.e. check if CharacterEntity is on a ledge)
        /// </summary>
        /// <param name="onLedge">True if this CharacterEntity is about to walk off an edge</param>
        /// <returns>True if there are tiles obstructing the movement of this CharacterEntity</returns>
        public bool CheckObstructingTiles(CharacterEntity entity, out bool onLedge)
        {
            // Assume the player is on a ledge
            onLedge = true;

            // -- OBSTRUCTION DETECTION --

            // Create a new hitbox to sense for obstructions
            Rectangle obstructionCheckHitbox = new Rectangle(
                // Same location as the entity hitbox
                entity.Hitbox.Location,

                new Point(
                    // Width of half a tile
                    GameManager.TileSize / 2,
                    // Height is the same height as the entity hitbox
                    // Remove 1 pixel since we are embedded 1 pixel into the ground
                    entity.Hitbox.Height - 1));

            if (entity.CurrentFacingDirection == CharacterEntity.FacingDirection.Left)
            {
                // If facing left, offset to the left of the hitbox
                obstructionCheckHitbox.Offset(-GameManager.TileSize / 2, 0);
            }
            else
            {
                // Otherwise (facing right), offset to the right of the hitbox
                obstructionCheckHitbox.Offset(entity.Hitbox.Width, 0);
            }

            // -- LEDGE DETECTION --

            // Copy and resize hitbox for the purpose of sensing if CharacterEntity is on a ledge:
            Rectangle ledgeCheckHitbox = new Rectangle(
                obstructionCheckHitbox.X,
                // Move below the CharacterEntity's hitbox
                obstructionCheckHitbox.Y + entity.Hitbox.Height,

                // Hitbox is only half a tile in size
                GameManager.TileSize / 2,
                GameManager.TileSize / 2);


            // -- GET REGION --

            TilemapRegion region =
                // Get the tilemap region in which the entity actually exists
                GetIterationRegion(Rectangle.Union(obstructionCheckHitbox, ledgeCheckHitbox));

            // Iterate through each tile in the tilemap array
            for (int row = region.StartingRow; row <= region.EndingRow; row++)
            {
                for (int col = region.StartingCol; col <= region.EndingCol; col++)
                {
                    // Get this tile's tileset ID
                    int tileID = currentLevel.GetTileID(col, row);

                    // If this tile has no hitbox, skip it
                    if (!HitboxTileIDs.Contains(tileID))
                    {
                        continue;
                    }

                    // Get the hitbox for the tile
                    Rectangle tileHitbox = currentLevel.GetTileHitbox(col, row);

                    
                    // Check if there is a tile inside of this hitbox (this tile would be obstructing motion)
                    if (obstructionCheckHitbox.Intersects(tileHitbox))
                    {
                        // Obstruction found
                        return true;
                    }

                    // Check if there is a tile inside of this hitbox, if there is, then we aren't on a ledge
                    if (ledgeCheckHitbox.Intersects(tileHitbox))
                    {
                        onLedge = false;
                    }
                }
            }

            // No obstruction
            return false;
        }

        /// <summary>
        /// Provided with an Entity and a reference to the Player, returns true if they are colliding
        /// </summary>
        public bool CheckEntityCollision(Entity e, Player player)
        {
            return e.Hitbox.Intersects(player.Hitbox);
        }
    }
}