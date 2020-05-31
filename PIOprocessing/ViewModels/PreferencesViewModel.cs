using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PIOprocessing.Models;

namespace PIOprocessing.ViewModels
{
    class PreferencesViewModel : Screen
    {
        public string BackgroundColour
        {
            get
            {
                return ThemeModel.BackgroundColour;
            }
        }
        public string ForegroundColour
        {
            get
            {
                return ThemeModel.ForegroundColour;
            }
        }

        public string Font
        {
            get
            {
                return ThemeModel.Font;
            }
        }
        public string FontWeight
        {
            get
            {
                return ThemeModel.FontWeight;
            }
        }

        protected List<string> themes;
        public List<string> Themes { get { return themes; } }
        protected string selectedTheme;
        public string SelectedTheme
        {
            get
            {
                return selectedTheme;
            }
            set
            {
                selectedTheme = value;
                Properties.Settings.Default.Theme = selectedTheme;
                ThemeModel.RefreshTheme();
                NotifyOfPropertyChange(() => BackgroundColour);
                NotifyOfPropertyChange(() => ForegroundColour);
                NotifyOfPropertyChange(() => Font);
                NotifyOfPropertyChange(() => FontWeight);
            }
        }
        
        protected bool relativeBetsize;
        
        public bool RelativeBetsize
        {
            get
            {
                return relativeBetsize;
            }
            set
            {
                relativeBetsize = value;
                Properties.Settings.Default.RelativeBetsize = relativeBetsize;
            }
        }
        public bool AbsoluteBetsize
        {
            get
            {
                return !relativeBetsize;
            }
        }

        protected int startingStacks;
        public int StartingStacks {
            get
            {
                return startingStacks;
            }
            set {
                startingStacks = value;
                Properties.Settings.Default.StartingStacks = startingStacks;
            } }

        protected int smallBlind;
        public int SmallBlind
        {
            get
            {
                return smallBlind;
            }
            set
            {
                smallBlind = value;
                Properties.Settings.Default.SmallBlind = smallBlind;
            }
        }

        protected int bigBlind;
        public int BigBlind
        {
            get
            {
                return bigBlind;
            }
            set
            {
                bigBlind = value;
                Properties.Settings.Default.BigBlind = bigBlind;
            }
        }

        protected bool groupHands;
        public bool GroupHands
        {
            get
            {
                return groupHands;
            }
            set
            {
                groupHands = value;

                Properties.Settings.Default.GroupHands = groupHands;
            }
        }
        public bool DontGroupHands
        {
            get
            {
                return !groupHands;
            }
        }

        public PreferencesViewModel()
        {
            RelativeBetsize = Properties.Settings.Default.RelativeBetsize;
            startingStacks = Properties.Settings.Default.StartingStacks;
            SmallBlind = Properties.Settings.Default.SmallBlind;
            BigBlind = Properties.Settings.Default.BigBlind;
            SelectedTheme = Properties.Settings.Default.Theme;
            themes = ThemeModel.Themes.Keys.ToList<string>();
            GroupHands = Properties.Settings.Default.GroupHands;
        }

        public void SavePreferences()
        {
            Properties.Settings.Default.Save();
            this.TryClose();
        }

        public void CancelPreferences()
        {
            Properties.Settings.Default.Reload();
            this.TryClose();
        }
    }
}
