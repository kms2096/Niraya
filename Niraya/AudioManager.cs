using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;


namespace Niraya
{
    /// <summary>
    /// Handles the playing of all audio within the Game
    /// </summary>
    public static class AudioManager
    {
        // TODO: This class should keep track of all playing sounds, and only call methods on the relevant sound effects (not ALL of them, like it does now)

        /// <summary>
        /// Indexes each sound effect by its filename
        /// </summary>
        public static Dictionary<string, SoundEffectInstance> soundEffects;

        /// <summary>
        /// File names of all sound effects
        /// </summary>
        private static string[] effectNames;

        /// <summary>
        /// Location of the audio files
        /// </summary>
        private const string RelativePath = "_Sound Assets\\";

        /// <summary>
        /// Static constructor for AudioManager
        /// </summary>
        static AudioManager()
        {
            effectNames = new string[]
            {
                // BGM
                "BGM\\xDeviruchi - And The Journey Begins", // overworld theme
                "BGM\\xDeviruchi - Title Theme",
                "BGM\\xDeviruchi - The Icy Cave ", //underworld theme

                // Menu / UI
                "menu confirm", // pressing enter on a button
                "menu select", // highlighting button

                // Effects
                "effects\\super crash", // death

                // Player
                "health pickup",
                "player footstep",
                "player sword attack 1",
                "player dash sword attack",
                "player damage 2",
                "player death 1",

                // Enemy
                "knight sword attack 01",
                "skeleton attack"
            };

            soundEffects = new Dictionary<string, SoundEffectInstance>();
        }

        /// <summary>
        /// Loads audio files
        /// </summary>
        public static void LoadContent(ContentManager content)
        {
            foreach (string effectName in effectNames)
            {
                // Add a new entry to the dictionary
                soundEffects.Add(
                    effectName,
                    // Load the effect and create an instance
                    content.Load<SoundEffect>(RelativePath + effectName).CreateInstance());
            }
        }

        /// <summary>
        /// Immediately pauses all sound effects
        /// </summary>
        public static void PauseAllEffects()
        {
            foreach (KeyValuePair<string, SoundEffectInstance> soundEffect in soundEffects)
            {
                // Pause the SoundEffectInstance
                soundEffect.Value.Pause();
            }
        }

        /// <summary>
        /// Immediately stops all sound effects
        /// </summary>
        public static void StopAllEffects()
        {
            foreach (KeyValuePair<string, SoundEffectInstance> soundEffect in soundEffects)
            {
                // Stop the SoundEffectInstance
                soundEffect.Value.Stop();
            }
        }

        /// <summary>
        /// Plays the provided sound effect, even if it's already in progress (stop + play)
        /// </summary>
        public static void ForcePlay(SoundEffectInstance soundEffect)
        {
            // ez pz why isnt this a thing already
            soundEffect.Stop();
            soundEffect.Play();
        }
    }
}
