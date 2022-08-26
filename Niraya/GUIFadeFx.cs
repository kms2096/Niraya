using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace Niraya
{
    /// <summary>
    /// A screen fade with specified speeds
    /// </summary>
    class GUIFadeFx : GUIElement 
    {
        /// <summary>
        /// A state that a fade can be in
        /// </summary>
        public enum FadeState
        {
            FadingIn,
            FadingInToOut,
            FadingOut,

            /// <summary>
            /// Fade has fully faded in and is opaque
            /// </summary>
            FullOpaque,

            /// <summary>
            /// Fade is complete and is transparent now
            /// </summary>
            FullTransparent,

            /// <summary>
            /// Fade is reset; has yet to start
            /// </summary>
            Inactive
        }

        private FadeState fadeState;

        private Texture2D fadeTexture;

        private float opacity;

        private float fadeInSpeed;
        private float fadeOutSpeed;

        public FadeState CurrentFadeState
        {
            get { return fadeState; }
        }

        /// <summary>
        /// Creates a new fade
        /// </summary>
        /// <param name="fadeTexture">Screen-sized texture to fade</param>
        /// <param name="fadeInSpeed">Opacity delta per frame when fading in</param>
        /// <param name="fadeOutSpeed">Opacity delta per frame when fading out</param>
        public GUIFadeFx(Texture2D fadeTexture, float fadeInSpeed, float fadeOutSpeed)

            // Always positioned at 0, 0
            : base(Vector2.Zero)
        {
            this.fadeTexture = fadeTexture;

            this.fadeInSpeed = fadeInSpeed;
            this.fadeOutSpeed = fadeOutSpeed;

            Reset();
        }

        /// <summary>
        /// Begins a fade in
        /// </summary>
        public void StartFadeIn()
        {
            fadeState = FadeState.FadingIn;
            opacity = 0f;
        }

        /// <summary>
        /// Begins a fade in/out
        /// </summary>
        public void StartFadeInOut()
        {
            fadeState = FadeState.FadingInToOut;
            opacity = 0f;
        }

        /// <summary>
        /// Starts already faded in, and begins a fade out
        /// </summary>
        public void StartFadeOut()
        {
            fadeState = FadeState.FadingOut;
            opacity = 1f;
        }

        /// <summary>
        /// Resets fade
        /// </summary>
        public void Reset()
        {
            fadeState = FadeState.Inactive;
            opacity = 0f;
        }

        public override void Update(GameTime gameTime)
        {
            switch (fadeState)
            {
                case FadeState.FadingIn:
                    opacity += fadeInSpeed;

                    if (opacity >= 1f)
                    {
                        fadeState = FadeState.FullOpaque;
                    }

                    break;

                // Just like fading in, but we go straight to the fade-out
                case FadeState.FadingInToOut:
                    opacity += fadeInSpeed;

                    if (opacity >= 1f)
                    {
                        opacity = 1f;

                        // Go directly to fadeout
                        fadeState = FadeState.FadingOut;
                    }

                    break;

                case FadeState.FadingOut:
                    opacity -= fadeOutSpeed;

                    if (opacity <= 0f)
                    {
                        fadeState = FadeState.FullTransparent;
                    }

                    break;

                case FadeState.FullOpaque:
                    opacity = 1f;

                    break;
                case FadeState.FullTransparent:
                    opacity = 0f;

                    break;

                case FadeState.Inactive:
                    opacity = 0f;

                    break;

            }
        }

        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(fadeTexture, position, Color.White * opacity);
        }
    }
}
