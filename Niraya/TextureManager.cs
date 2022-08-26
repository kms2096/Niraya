using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Niraya
{
    /// <summary>
    /// Handles loading and referencing all textures within the Game
    /// </summary>
    public static class TextureManager
    {
        /// <summary>
        /// Indexes each texture by its filename
        /// </summary>
        public static Dictionary<string, Texture2D> textures;

        /// <summary>
        /// File names of all textures
        /// </summary>
        private static string[] textureNames;

        /// <summary>
        /// Location of the texture files
        /// </summary>
        private const string RelativePath = "_Image Assets\\";

        /// <summary>
        /// Static constructor for TextureManager
        /// </summary>
        static TextureManager()
        {
            textureNames = new string[]
            {
                "_debugging\\DebugSprite",
                "Player\\Warrior_Sheet-Effect_3x",
                "Enemy\\skeleton_3x",
                "Enemy\\knight_beige_3x",
                "Enemy\\knight_3x",
                "BG_menus\\victoryScreen",
                "BG_menus\\gameOverScreen",
                "Items\\apple_consumable3x",
                "Dog\\dog48x32_x2"
            };

            textures = new Dictionary<string, Texture2D>();
        }

        /// <summary>
        /// Loads texture files
        /// </summary>
        public static void LoadContent(ContentManager content)
        {
            foreach (string textureName in textureNames)
            {
                // Add a new entry to the dictionary
                textures.Add(
                    textureName,
                    // Load the effect and create an instance
                    content.Load<Texture2D>(RelativePath + textureName));
            }
        }
    }
}
