using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Niraya
{
    /// <summary>
    /// Contains all the information needed for a level in the game
    /// </summary>
    public class Level
    {
        /// <summary>
        /// Name of this Level, and the folder containing the files for this Level
        /// </summary>
        private string levelName;

        // --- Tiles ---

        /// <summary>
        /// 2D Array of all Tile objects in a level
        /// </summary>
        private Tile[,] tilemap;

        /// <summary>
        /// Dimension of a tile
        /// </summary>
        private int tileSize;

        /// <summary>
        /// Current tileset to use for drawing tiles
        /// </summary>
        private Tileset currentTileset;

        /// <summary>
        /// Number of extra rows to draw below the level
        /// </summary>
        private const int ExtraRows = 12;

        // --- Entities ---

        // TODO: Probably should not be public, this is unsafe. Find better solution for Player access in GameManager.
        /// <summary>
        /// The player
        /// </summary>
        public Player player;

        // TODO: Probably should not be public, this is unsafe. Find better solution for LevelGoal access in GameManager.
        /// <summary>
        /// The level goal at the end of the level
        /// </summary>
        public LevelGoal levelGoal;

        // TODO: This should not be set to public! This is only done for debugging purposes
        /// <summary>
        /// List of all enemies in this level
        /// </summary>
        public List<Enemy> enemyList;

        /// <summary>
        /// List of all collectibles in this level
        /// </summary>
        private List<Collectible> collectibleList;

        // ---

        // Used to get the path and filename for a given level name
        private const string DirectoryPath = "../../../Levels/";
        private const string TilemapFileExtension = "_tilemap.csv";
        private const string EntitiesFileExtension = "_entities.csv";

        public Level(string levelName)
        {
            // Set the level name
            this.levelName = levelName;

            // Set tile size
            tileSize = GameManager.TileSize;
        }

        /// <summary>
        /// Returns a world-positioned hitbox for the tile at a given column and row in the level
        /// </summary>
        public Rectangle GetTileHitbox(int col, int row)
        {
            return new Rectangle(
                col * tileSize,
                row * tileSize,
                tileSize,
                tileSize);
        }

        /// <summary>
        /// The width of the tilemap, in tiles
        /// </summary>
        public int TileMapWidth
        {
            get { return tilemap.GetLength(0); }
        }
        /// <summary>
        /// The height of the tilemap, in tiles
        /// </summary>
        public int TileMapHeight
        {
            get { return tilemap.GetLength(1); }
        }

        /// <summary>
        /// The width of the level, in pixels
        /// </summary>
        public int LevelWidth
        {
            get { return tilemap.GetLength(0) * tileSize; }
        }
        /// <summary>
        /// The height of the level, in pixels
        /// </summary>
        public int LevelHeight
        {
            get { return tilemap.GetLength(1) * tileSize; }
        }

        /// <summary>
        /// Returns the numeric Tileset ID for the tile at a given column and row in the level
        /// </summary>
        public int GetTileID(int col, int row)
        {
            return tilemap[col, row].ID;
        }

        /// <summary>
        /// The current tileset the Level will use for drawing tiles
        /// </summary>
        public Tileset CurrentTileset
        {
            set { currentTileset = value; }
        }

        /// <summary>
        /// Updates all entities in the level
        /// </summary>
        public void Update(GameTime gameTime)
        {

            // --- UPDATE ENTITIES --- //

            foreach (Enemy e in enemyList)
            {
                e.Update(gameTime);
            }


            // --- UPDATE PLAYER --- //

            // TODO: Move this into the above Entity.Update() foreach loop
            player.Update(gameTime);



            // ---  COLLISIONS AND ATTACKS ---//

            // Resolve player collisions
            GameManager._collisionManager.ResolveTileCollisions(player);

            // For every enemy
            foreach (Enemy e in enemyList)
            {
                // Resolve enemy collisions
                GameManager._collisionManager.ResolveTileCollisions(e);

                // Update the enemy with their obstruction status
                bool onLedge;
                e.IsObstructed = GameManager._collisionManager.CheckObstructingTiles(e, out onLedge);
                e.IsOnLedge = onLedge;

                // --- ATTACKS --- //

                // Check if player hit enemy
                if (GameManager._collisionManager.CheckAttackFromCharacter(player, e))
                {
                    if (GameManager.isGodmodeEnabled)
                    {
                        e.TakeDamage(gameTime, e.Health, player.Hitbox.Center.ToVector2());
                    }
                    else
                    {
                        // TODO: This is hardcoded!-- ideally the player should store the value of damage to deal, either in a field/property or a method Attack() that returns an int
                        e.TakeDamage(gameTime, 20, player.Hitbox.Center.ToVector2());
                    }
                }
                // Check if enemy hit player
                if (GameManager._collisionManager.CheckAttackFromCharacter(e, player))
                {
                    // TODO: This is hardcoded!-- ideally the enemy should store the value of damage to deal, either in a field/property or a method Attack() that returns an int
                    player.TakeDamage(gameTime, e.Attack(), e.Hitbox.Center.ToVector2());

                }
            }

            // -- COLLECTIBLES --

            foreach(Collectible c in collectibleList)
            {
                // If a controllable player collides with an active collectible
                if (player.IsControllable &&
                    c.IsActive &&
                    GameManager._collisionManager.CheckEntityCollision(c, player))
                {
                    // TODO: Remove this collectible from the list? (no way to re-activate collectibles so yes they should just be removed)

                    // TODO: I did this poorly, do this in a more robust way rather than just checking the type on pickup
                    // Check if this collectible is an apple
                    if (c.Type == Collectible.CollectibleType.Apple)
                    {
                        // Pick up this collectible
                        // TODO: Collectible pick up method should use events/delegates to call player methods such as Heal(), alteratively Player should have a Pickup(Collectible c) method
                        c.PickUp();

                        // Heal the player
                        player.Heal(10);
                    }
                }
            }
        }

        /// <summary>
        /// Transitions this Level to the underworld
        /// </summary>
        public void TransitionToUnderworld()
        {
            // Check every Enemy in enemyList
            for (int i = 0; i < enemyList.Count; i++)
            {
                // If this Enemy is dead
                if (
                    //enemyList[i] as EnemyKnight != null &&
                    !enemyList[i].IsAlive)
                {
                    // Turn it into an alive skeleton at the Enemy's position
                    enemyList[i] = new EnemySkeleton(enemyList[i].Position.ToPoint());
                }       
            }
        }

        /// <summary>
        /// Draws all tiles in this level
        /// </summary>
        private void DrawTiles(SpriteBatch sb)
        {
            // Iterate through every single tile in the level
            for (int row = 0; row < TileMapHeight + ExtraRows; row++)
            {
                for (int col = 0; col < TileMapWidth; col++)
                {
                    int tileID;

                    // As long as we are drawing REAL tiles (not extra rows)
                    if (row < TileMapHeight)
                    {
                        // Record the tile ID for this tile
                        tileID = GetTileID(col, row);
                    }
                    else
                    {
                        // Use the tile ID for the bottom row of tiles
                        tileID = GetTileID(col, TileMapHeight - 1);
                    }

                    // If the tile ID is -1, it doesn't exist, otherwise draw this tile
                    if (tileID != -1)
                    {
                        // Weird shift that makes most of the level tiles in the overworld draw at the correct height
                        // TODO: This is all hardcoded. Not sure much can be done really but yeah
                        int yShift = 0;
                        if (GameManager.dimensionState == GameManager.DimensionState.Overworld)
                        {
                            if (!((tileID >= 12 && tileID <= 25) ||
                            (tileID >= 38 && tileID <= 51) ||
                            (tileID >= 64 && tileID <= 77) ||
                            (tileID >= 90 && tileID <= 103)))
                            {
                                // At 3x scaling, this is 2 ingame pixels
                                yShift = -6;
                            }
                        }

                        sb.Draw(

                            // Draw from the current tileset's texture
                            currentTileset.Tilesheet,

                            // Draw to the correct coordinates on screen
                            new Rectangle(
                                col * tileSize - (int)GameManager.camera.Position.X,
                                // yShift is for the above shift for the weird detail tiles
                                row * tileSize + yShift - (int)GameManager.camera.Position.Y,
                                tileSize,
                                tileSize),

                            // Find the local rectangle on the texture to draw from, based on what kind of tile this is
                            currentTileset.GetTilesheetLocalRect(tileID),

                            // No tint
                            Color.White);
                    }
                }
            }
        }

        /// <summary>
        /// Draws all entities in this level
        /// </summary>
        private void DrawEntities(GameTime gameTime, SpriteBatch sb)
        {
            // -- Draw ENTITIES --

            foreach (Enemy e in enemyList)
            {
                e.Draw(gameTime, sb);
            }

            // Drawing collectibles
            foreach (Collectible collectible in collectibleList)
            {
                collectible.Draw(gameTime, sb);
            }

            // Draw the level goal
            levelGoal.Draw(gameTime, sb);

            // Drawing the player (should be on top of other entities)
            player.Draw(gameTime, sb);
        }

        /// <summary>
        /// Draws the level to the provided SpriteBatch
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch sb)
        {
            DrawTiles(sb);
            DrawEntities(gameTime, sb);
        }

        /// <summary>
        /// Parses relevant level CSV files and generates a level
        /// </summary>
        public void LoadLevel()
        {
            LoadTilemapFromFile(DirectoryPath + levelName + "\\" + levelName + TilemapFileExtension);
            LoadEntitiesFromFile(DirectoryPath + levelName + "\\" + levelName + EntitiesFileExtension);
        }

        /// <summary>
        /// Unloads this level's data from memory
        /// </summary>
        public void UnloadLevel()
        {
            tilemap = null;

            player = null;
            enemyList = null;
            collectibleList = null;
        }

        /// <summary>
        /// Parses a CSV file for entity information, then adds this data to the Level
        /// </summary>
        /// <param name="filename">Path to the entities CSV file</param>
        private void LoadEntitiesFromFile(string filename)
        {
            // Initialize empty Lists
            enemyList = new List<Enemy>();
            collectibleList = new List<Collectible>();


            // Create null StreamReader before attempting to open file
            StreamReader input = null;

            try
            {
                // Open the file
                input = new StreamReader(filename);

                // Parse the file
                for (int row = 0; row < TileMapHeight; row++)
                {
                    // Read in an entire line and split it into an array (this represents a row)
                    string[] data = input.ReadLine().Split(",");

                    // Read each element in the array (this represents an entity tile)
                    for (int col = 0; col < TileMapWidth; col++)
                    {
                        // ID of this entity tile
                        int entityTileID = int.Parse(data[col]);

                        // Check which entity the tile represents
                        switch (entityTileID)
                        {
                            // PLAYER SPAWN
                            case 234:
                                // Create player, setting spawn point to the tile position
                                player = new Player(GetTileHitbox(col, row).Location);
                                break;

                            // KNIGHT SPAWN
                            case 235:
                                enemyList.Add(
                                    // Create a Basic knight, setting spawn point to the tile position
                                    new EnemyKnight(
                                        GetTileHitbox(col, row).Location,
                                        EnemyKnight.KnightType.Basic));
                                break;

                            // APPLE
                            case 236:
                                collectibleList.Add(
                                    // Create an apple, setting location to the tile position
                                    new Collectible(
                                        GetTileHitbox(col, row).Location,
                                        Collectible.CollectibleType.Apple));
                                break;

                            // LEVEL END
                            case 237:
                                levelGoal = new LevelGoal(
                                    GetTileHitbox(col, row).Location,
                                    LevelGoal.GoalType.NOT_IMPLEMENTED);
                                break;

                            // DOG (Victory)
                            case 238:
                                levelGoal = new LevelGoal(
                                    GetTileHitbox(col, row).Location,
                                    LevelGoal.GoalType.Dog);
                                break;

                            // *EMPTY
                            case -1:
                                // Do nothing
                                break;

                            // *UNKNOWN TILE
                            default:
                                // Crash
                                throw new Exception($"Entities file \"{filename}\" contains unknown entity tile ID {entityTileID}");
                        }
                    }
                }

                // Success message
                Debug.WriteLine($"Loaded entities from file \"{filename}\"");

            }
            // If the file is not found, print an error
            catch (FileNotFoundException e)
            {
                Debug.WriteLine($"Could not find entities file \"{filename}\" : {e}");
            }

            // If the file is not found, print an error
            catch (DirectoryNotFoundException e)
            {
                Debug.WriteLine($"Could not find entities directory \"{filename}\" : {e}");
            }

            // If anything else throws any other exception, print an error
            catch (Exception e)
            {
                Debug.WriteLine($"Failed to load entities from file \"{filename}\" : {e}");
            }
        }

        /// <summary>
        /// Parses a CSV file and generates a tilemap
        /// </summary>
        /// <param name="filename">Path to the tilemap CSV file</param>
        private void LoadTilemapFromFile(string filename)
        {
            StreamReader input = null;

            try
            {
                // Open the file
                input = new StreamReader(filename);

                // Read in first line of data, use this for the dimensions of the level
                string[] data = input.ReadLine().Split(",");

                // Initialize tilemap 2D array
                tilemap = new Tile[int.Parse(data[0]), int.Parse(data[1])];

                // Parse the file
                for (int row = 0; row < TileMapHeight; row++)
                {
                    // Read in an entire line and split it into an array (this represents a row)
                    data = input.ReadLine().Split(",");

                    // Read each element in the array (this represents a tile)
                    for (int col = 0; col < TileMapWidth; col++)
                    {
                        // Convert the string representation of the tile ID into a number,
                        // then turn this into a Tile and store it in the tilemap
                        tilemap[col, row] = new Tile(int.Parse(data[col]));
                    }
                }

                // Success message
                Debug.WriteLine($"Loaded tilemap from file \"{filename}\"");

            }
            // If the file is not found, print an error
            catch (FileNotFoundException e)
            {
                Debug.WriteLine($"Could not find tilemap file \"{filename}\" : {e}");
            }

            // If the file is not found, print an error
            catch (DirectoryNotFoundException e)
            {
                Debug.WriteLine($"Could not find tilemap directory \"{filename}\" : {e}");
            }

            // If anything else throws any other exception, print an error
            catch (Exception e)
            {
                Debug.WriteLine($"Failed to load tilemap from file \"{filename}\" : {e}");
            }
        }
    }
}
