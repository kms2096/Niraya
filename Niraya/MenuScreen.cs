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
    /// Possible states for the main menu
    /// </summary>
    public enum MainMenuState
    {
        startSelected,
        optionsSelected,
        creditsSelected
    }

    /// <summary>
    /// Possible states for the pause menu
    /// </summary>
    public enum PauseMenuState
    {
        resumeSelected,
        optionsSelected,
        quitSelected
    }

    /// <summary>
    /// Possible states for the options menu
    /// </summary>
    public enum OptionsMenuState
    {
        musicSelected,
        debugSelected
    }

    /// <summary>
    /// Possible states for the credits menu
    /// </summary>
    public enum CreditsMenuState
    {
        page1,
        page2,
        page3
    }

    /// <summary>
    /// A state-based menu screen
    /// </summary>
    class MenuScreen
    {
        // FIELDS
        protected Texture2D[] menuStates;

        protected MainMenuState mainButtonSelected;
        protected PauseMenuState pauseButtonSelected;
        protected OptionsMenuState optionsButtonSelected;
        protected CreditsMenuState creditsButtonSelected;

        // PROPERTIES
        public MainMenuState MainButtonSelected
        {
            get { return mainButtonSelected; }
        }

        public PauseMenuState PauseButtonSelected
        {
            get { return pauseButtonSelected; }
            set { pauseButtonSelected = value; }
        }

        public OptionsMenuState OptionsButtonSelected
        {
            get { return optionsButtonSelected; }
        }

        public CreditsMenuState CreditsButtonSelected
        {
            get { return creditsButtonSelected; }
        }

        // CONSTRUCTOR
        /// <summary>
        /// A parameterized constructor which creates a menu screen based on
        /// an array of Texture2Ds and a sound effect passed in as parameters.
        /// </summary>
        public MenuScreen(Texture2D[] menuStates)
        {
            this.menuStates = menuStates;

            mainButtonSelected = MainMenuState.startSelected;
            pauseButtonSelected = PauseMenuState.resumeSelected;
            optionsButtonSelected = OptionsMenuState.musicSelected;
            creditsButtonSelected = CreditsMenuState.page1;
        }

        // METHODS
        /// <summary>
        /// Draws the main menu based on its current state
        /// </summary>
        public void DrawMainMenu(SpriteBatch sb)
        {
            switch (mainButtonSelected)
            {
                case MainMenuState.startSelected:
                    sb.Draw(menuStates[0],
                            new Rectangle(0, 0, menuStates[0].Width, menuStates[0].Height),
                            Color.White);
                    break;

                case MainMenuState.optionsSelected:
                    sb.Draw(menuStates[1],
                            new Rectangle(0, 0, menuStates[1].Width, menuStates[1].Height),
                            Color.White);
                    break;

                case MainMenuState.creditsSelected:
                    sb.Draw(menuStates[2],
                            new Rectangle(0, 0, menuStates[2].Width, menuStates[2].Height),
                            Color.White);
                    break;
            }
        }

        /// <summary>
        /// Updates the main menu state. States are based on player input with the 
        /// keyboard.
        /// </summary>
        public void UpdateMainMenu(KeyboardState kbState, KeyboardState prevKbState)
        {
            switch (mainButtonSelected)
            {
                case MainMenuState.startSelected:
                    if (SingleKeyPress(kbState, prevKbState, Keys.S))
                    {
                        mainButtonSelected = MainMenuState.optionsSelected;
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu select"]);
                    }
                    else if (SingleKeyPress(kbState, prevKbState, Keys.W))
                    {
                        mainButtonSelected = MainMenuState.creditsSelected;
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu select"]);
                    }
                    break;

                case MainMenuState.optionsSelected:
                    if (SingleKeyPress(kbState, prevKbState, Keys.S))
                    {
                        mainButtonSelected = MainMenuState.creditsSelected;
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu select"]);
                    }
                    else if (SingleKeyPress(kbState, prevKbState, Keys.W))
                    {
                        mainButtonSelected = MainMenuState.startSelected;
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu select"]);
                    }
                    break;

                case MainMenuState.creditsSelected:
                    if (SingleKeyPress(kbState, prevKbState, Keys.S))
                    {
                        mainButtonSelected = MainMenuState.startSelected;
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu select"]);
                    }
                    else if (SingleKeyPress(kbState, prevKbState, Keys.W))
                    {
                        mainButtonSelected = MainMenuState.optionsSelected;
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu select"]);
                    }
                    break;
            }
        }

        /// <summary>
        /// Draws the options menu based on its current state.
        /// </summary>
        public void DrawOptionsMenu(SpriteBatch sb)
        {
            if (!GameManager.debugModeEnabled && !GameManager.musicEnabled)
            {
                if (optionsButtonSelected == OptionsMenuState.musicSelected)
                {
                    sb.Draw(menuStates[0],
                           new Rectangle(0, 0, menuStates[0].Width, menuStates[0].Height),
                           Color.White);
                }
                else
                {
                    sb.Draw(menuStates[4],
                            new Rectangle(0, 0, menuStates[4].Width, menuStates[4].Height),
                            Color.White);
                }
            }
            else if (GameManager.debugModeEnabled && !GameManager.musicEnabled)
            {
                if (optionsButtonSelected == OptionsMenuState.musicSelected)
                {
                    sb.Draw(menuStates[2],
                            new Rectangle(0, 0, menuStates[2].Width, menuStates[2].Height),
                            Color.White);
                }
                else
                {
                    sb.Draw(menuStates[6],
                            new Rectangle(0, 0, menuStates[6].Width, menuStates[6].Height),
                            Color.White);
                }
            }
            else if (!GameManager.debugModeEnabled && GameManager.musicEnabled)
            {
                if (optionsButtonSelected == OptionsMenuState.musicSelected)
                {
                    sb.Draw(menuStates[1],
                            new Rectangle(0, 0, menuStates[1].Width, menuStates[1].Height),
                            Color.White);
                }
                else
                {
                    sb.Draw(menuStates[5],
                            new Rectangle(0, 0, menuStates[5].Width, menuStates[5].Height),
                            Color.White);
                }
            }
            else
            {
                if (optionsButtonSelected == OptionsMenuState.musicSelected)
                {
                    sb.Draw(menuStates[3],
                            new Rectangle(0, 0, menuStates[3].Width, menuStates[3].Height),
                            Color.White);
                }
                else
                {
                    sb.Draw(menuStates[7],
                            new Rectangle(0, 0, menuStates[7].Width, menuStates[7].Height),
                            Color.White);
                }
            }
        }

        /// <summary>
        /// Updates the state of the options menu. State is based on player input with
        /// the keyboard.
        /// </summary>
        public void UpdateOptionsMenu(KeyboardState kbState, KeyboardState prevKbState)
        {
            switch (optionsButtonSelected)
            {
                case OptionsMenuState.musicSelected:
                    if (SingleKeyPress(kbState, prevKbState, Keys.D))
                    {
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu select"]);
                        optionsButtonSelected = OptionsMenuState.debugSelected;
                    }
                    else if (SingleKeyPress(kbState, prevKbState, Keys.Enter))
                    {
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu confirm"]);
                        GameManager.musicEnabled = !GameManager.musicEnabled;
                    }
                    break;

                case OptionsMenuState.debugSelected:
                    if (SingleKeyPress(kbState, prevKbState, Keys.A))
                    {
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu select"]);
                        optionsButtonSelected = OptionsMenuState.musicSelected;
                    }
                    else if (SingleKeyPress(kbState, prevKbState, Keys.Enter))
                    {
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu confirm"]);
                        GameManager.debugModeEnabled = !GameManager.debugModeEnabled;
                    }
                    break;

            }
        }

        /// <summary>
        /// Updates the state of the credits menu. State is based on player input.
        /// </summary>
        /// <param name="kbState"></param>
        /// <param name="prevKbState"></param>
        public void UpdateCreditsMenu(KeyboardState kbState, KeyboardState prevKbState)
        {
            switch (creditsButtonSelected)
            {
                case CreditsMenuState.page1:
                    if (SingleKeyPress(kbState, prevKbState, Keys.D))
                    {
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu select"]);
                        creditsButtonSelected = CreditsMenuState.page2;
                    }
                    break;

                case CreditsMenuState.page2:
                    if (SingleKeyPress(kbState, prevKbState, Keys.D))
                    {
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu select"]);
                        creditsButtonSelected = CreditsMenuState.page3;
                    }
                    else if (SingleKeyPress(kbState, prevKbState, Keys.A))
                    {
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu select"]);
                        creditsButtonSelected = CreditsMenuState.page1;
                    }
                    break;

                case CreditsMenuState.page3:
                    if (SingleKeyPress(kbState, prevKbState, Keys.A))
                    {
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu select"]);
                        creditsButtonSelected = CreditsMenuState.page2;
                    }
                    break;
            }
        }
        /// <summary>
        /// Draws the credits menu.
        /// </summary>
        public void DrawCreditsMenu(SpriteBatch sb)
        {
            switch (creditsButtonSelected)
            {
                case CreditsMenuState.page1:
                    sb.Draw(menuStates[0],
                            new Rectangle(0, 0, menuStates[0].Width, menuStates[0].Height),
                            Color.White);
                    break;

                case CreditsMenuState.page2:
                    sb.Draw(menuStates[1],
                            new Rectangle(0, 0, menuStates[0].Width, menuStates[0].Height),
                            Color.White);
                    break;

                case CreditsMenuState.page3:
                    sb.Draw(menuStates[2],
                            new Rectangle(0, 0, menuStates[0].Width, menuStates[0].Height),
                            Color.White);
                    break;

            }
        }

        /// <summary>
        /// Draws the pause menu based on its current state.
        /// </summary>
        public void DrawPauseMenu(SpriteBatch sb)
        {
            sb.Draw(menuStates[0],
                    new Rectangle(0, 0, menuStates[0].Width, menuStates[0].Height),
                    Color.White * 0.5f);

            switch (pauseButtonSelected)
            {
                case PauseMenuState.resumeSelected:
                    sb.Draw(menuStates[1],
                            new Rectangle(0, 0, menuStates[1].Width, menuStates[1].Height),
                            Color.White);
                    break;

                case PauseMenuState.optionsSelected:
                    sb.Draw(menuStates[2],
                           new Rectangle(0, 0, menuStates[2].Width, menuStates[2].Height),
                           Color.White);
                    break;

                case PauseMenuState.quitSelected:
                    sb.Draw(menuStates[3],
                           new Rectangle(0, 0, menuStates[3].Width, menuStates[3].Height),
                           Color.White);
                    break;
            }

        }

        /// <summary>
        /// Updates the state of the pause menu. State is based on player input with
        /// the keyboard.
        /// </summary>
        public void UpdatePauseMenu(KeyboardState kbState, KeyboardState prevKbState)
        {
            switch (pauseButtonSelected)
            {
                case PauseMenuState.resumeSelected:
                    if (SingleKeyPress(kbState, prevKbState, Keys.S))
                    {
                        pauseButtonSelected = PauseMenuState.optionsSelected;
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu select"]);
                    }
                    else if (SingleKeyPress(kbState, prevKbState, Keys.W))
                    {
                        pauseButtonSelected = PauseMenuState.quitSelected;
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu select"]);
                    }
                    break;

                case PauseMenuState.optionsSelected:
                    if (SingleKeyPress(kbState, prevKbState, Keys.S))
                    {
                        pauseButtonSelected = PauseMenuState.quitSelected;
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu select"]);
                    }
                    else if (SingleKeyPress(kbState, prevKbState, Keys.W))
                    {
                        pauseButtonSelected = PauseMenuState.resumeSelected;
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu select"]);
                    }
                    break;

                case PauseMenuState.quitSelected:
                    if (SingleKeyPress(kbState, prevKbState, Keys.S))
                    {
                        pauseButtonSelected = PauseMenuState.resumeSelected;
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu select"]);
                    }
                    else if (SingleKeyPress(kbState, prevKbState, Keys.W))
                    {
                        pauseButtonSelected = PauseMenuState.optionsSelected;
                        AudioManager.ForcePlay(AudioManager.soundEffects["menu select"]);
                    }
                    break;
            }
        }

        // TODO: Do not implement this twice
        /// <summary>
        /// Returns true if the provided key is being pressed this frame but not last frame
        /// </summary>
        public bool SingleKeyPress(KeyboardState kbState, KeyboardState previousKbState, Keys keyToCheck)
        {
            if (previousKbState == null)
            {
                return false;
            }

            // Check if it was pressed last frame
            if (kbState.IsKeyDown(keyToCheck) && previousKbState.IsKeyUp(keyToCheck))
            {
                return true;
            }

            return false;
        }
    }
}
