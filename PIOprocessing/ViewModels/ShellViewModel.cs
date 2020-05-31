using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using Microsoft.WindowsAPICodePack.Dialogs;
using PIOprocessingInterface.Properties;
using System.Windows.Forms.PropertyGridInternal;
using System.Windows;
using PIOprocessing.Views;
using System.Windows.Controls;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Windows.Forms.VisualStyles;
using PIOprocessing.Models;

namespace PIOprocessing.ViewModels
{
    
    public class ShellViewModel : Conductor<object>
    {
        IWindowManager manager = new WindowManager();

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
        public OxyColor OxyBackgroundColour
        {
            get
            {
                return ThemeModel.OxyBackgroundColour;
            }
        }
        public OxyColor OxyForegroundColour
        {
            get
            {
                return ThemeModel.OxyForegroundColour;
            }
        }

        public string PlaceholderLink { get; set; }

        private string reportsPath;
        private string pathFeedback;
        private BindableCollection<string> actions = new BindableCollection<string>();
        private BindableCollection<string> aggPosList = new BindableCollection<string>();
        private BindableCollection<string> cllPosList = new BindableCollection<string>();
        private BindableCollection<string> boardTypeList = new BindableCollection<string>();
        private BindableCollection<string> boardSubtypeList = new BindableCollection<string>();
        private HashSet<HandType> typeList;


        private string selectedAction;
        private string selectedAggPos;
        private string selectedCllPos;
        private string selectedBoardType;
        private string selectedBoardSubtype;
        private HandCategory? selectedCategory;
        private HandType? selectedType;
        private string subtypesVisibility;

        private string graphVisibility;
        private string btnVisibility;
        private string pillVisibility;
        private string placeholderVisibility;

        private int graphRow;

        public int GraphRow
        {
            get { return graphRow; }
            set { graphRow = value; NotifyOfPropertyChange(() => GraphRow); }
        }

        private int graphColumn;

        public int GraphColumn
        {
            get { return graphColumn; }
            set { graphColumn = value; NotifyOfPropertyChange(() => GraphColumn); }
        }

        // public int GraphWidth;



        private ReportReader reader;
        private SpotModel spot;

        public string ReportsPath
        { 
            get 
            { 
                return reportsPath; 
            } 
            set
            {
                reportsPath = value;
                Properties.Settings.Default.ReportsPath = reportsPath;
                Properties.Settings.Default.Save();
                RefreshPath();
                NotifyOfPropertyChange(() => reportsPath);
            }
        }

        public string PathFeedback { get { return pathFeedback; } }

        public SpotModel Spot { get { return spot; } }

        public PlotModel PlotModel
        {
            get
            {
                if (spot != null)
                    return spot.PlotModel;
                else
                    return null;
            }
        }

        public ShellViewModel()
        {

            // this.eventAggregator = eventAggregator;
            reportsPath = Properties.Settings.Default.ReportsPath;
            if (reportsPath == "")
            {
                reportsPath = "Click 'Browse' to select a folder";
            } else {
                RefreshPath();
                NotifyOfPropertyChange(() => reportsPath);
            }
            subtypesVisibility = "Hidden";
            graphVisibility = "Hidden";
            determineButtonVisibility();
            placeholderVisibility = "Visible";
            graphRow = 0;
            graphColumn = 1;
            ThemeModel.RefreshTheme();
            updatePlaceholderLink();
        }


        public BindableCollection<string> Actions
        {
            get { return actions; }
            
        }

        public BindableCollection<string> AggPosList
        {
            get { return aggPosList; }
        }
        public BindableCollection<string> CllPosList
        {
            get { return cllPosList; }
        }
        public BindableCollection<string> BoardTypeList
        {
            get { return boardTypeList; }
        }
        public BindableCollection<string> BoardSubtypeList
        {
            get { return boardSubtypeList; }
        }

        public HashSet<HandType> TypeList { get { return typeList; } }
        public bool HasBoardSubtypes
        {
            get { return boardSubtypeList.Count != 0; }
        }

        public string SubtypesVisibility
        {
            get { return subtypesVisibility; }
        }
        public string GraphVisibility
        {
            get { return graphVisibility; }
        }
        public string BtnVisibility
        {
            get { return btnVisibility; }
        }
        public string PillVisibility
        {
            get { return pillVisibility; }
        }
        public string PlaceholderVisibility
        {
            get { return placeholderVisibility; }
        }

        public void BtnBrowse()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            // dialog.InitialDirectory = txtFolder.Text;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (dialog.FileName != reportsPath)
                {
                    ReportsPath = dialog.FileName;
                }
            }
        }

        public void BtnClearGraph()
        {
            selectedType = null;
            graphVisibility = "Hidden";
            determineButtonVisibility();
            placeholderVisibility = "Visible";
            // spot.LoadPlotModel((HandType)selectedType);
            
            // NotifyOfPropertyChange(() => PlotModel);
            NotifyOfPropertyChange(() => GraphVisibility);
            NotifyOfPropertyChange(() => PlaceholderVisibility);
            NotifyOfPropertyChange(() => SelectedType);
        }
        public void PillClearGraph()
        {
            BtnClearGraph();
        }

        public string SelectedAction
        {
            get { return selectedAction; }
            set
            {
                selectedAction = value;
                aggPosList.Clear();
                if (selectedAction != null) { 
                    foreach (string aggPos in reader.GetAggPosList(selectedAction))
                    {
                        aggPosList.Add(aggPos);
                    }
                    if(aggPosList.Count == 1)
                    {
                        SelectedAggPos = aggPosList[0];
                        NotifyOfPropertyChange(() => SelectedAggPos);
                    }
                    // selectedAggPos = aggPosList[0];
                }
                
                // NotifyOfPropertyChange(() => SelectedAggPos);
            }
        }

        public string SelectedAggPos
        {
            get { return selectedAggPos; }
            set
            {
                selectedAggPos = value;
                cllPosList.Clear();
                if (selectedAggPos != null)
                {
                    foreach (string cllPos in reader.GetCllPosList(selectedAction,selectedAggPos))
                    {
                        cllPosList.Add(cllPos);
                    }
                    if (cllPosList.Count == 1)
                    {
                        SelectedCllPos = cllPosList[0];
                        NotifyOfPropertyChange(() => SelectedCllPos);
                    }
                    // selectedCllPos = cllPosList[0];
                }
                // SelectedCllPos = null;

                // NotifyOfPropertyChange(() => SelectedAggPos);
            }
        }

        public string SelectedCllPos
        {
            get { return selectedCllPos; }
            set
            {
                selectedCllPos = value;
                boardTypeList.Clear();
                if (selectedCllPos != null)
                {
                    foreach (string boardType in reader.GetBoardTypeList(selectedAction, selectedAggPos, selectedCllPos))
                    {
                        boardTypeList.Add(boardType);
                    }
                    if (boardTypeList.Count == 1)
                    {
                        SelectedBoardType = boardTypeList[0];
                        NotifyOfPropertyChange(() => SelectedBoardType);
                    }
                    // selectedCllPos = cllPosList[0];
                }
                // SelectedBoardType = null;
                // NotifyOfPropertyChange(() => SelectedAggPos);
            }
        }

        public string SelectedBoardType
        {
            get { return selectedBoardType; }
            set
            {
                selectedBoardType = value;
                boardSubtypeList.Clear();

                spot = null;
                // TypeList.Clear();
                if (selectedBoardType != null)
                {
                    foreach (string boardSubtype in reader.GetSubtypeList(selectedAction, selectedAggPos, selectedCllPos, selectedBoardType))
                    {
                        boardSubtypeList.Add(boardSubtype);
                    }
                    // selectedCllPos = cllPosList[0];

                    if (boardSubtypeList.Count == 0)
                    {
                        subtypesVisibility = "Hidden";
                        loadReport();
                        NotifyOfPropertyChange(() => Spot);
                    }
                    else {
                        if (boardSubtypeList.Count == 1)
                        {
                        SelectedBoardSubtype = boardSubtypeList[0];
                        NotifyOfPropertyChange(() => SelectedBoardSubtype);
                        }
                        subtypesVisibility = "Visible";
                    }
                }
                NotifyOfPropertyChange(() => HasBoardSubtypes);
                NotifyOfPropertyChange(() => SubtypesVisibility);
                NotifyOfPropertyChange(() => Spot);
            }
        }

        public string SelectedBoardSubtype
        {
            get { return selectedBoardSubtype; }
            set
            {
                selectedBoardSubtype = value;
                if (selectedBoardSubtype != null)
                {
                    loadReport();
                }
            }
        }


        public HandCategory? SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                selectedCategory = value;
                if (typeList != null)
                {
                    typeList = null;
                }
                if (selectedCategory != null)
                {
                    typeList = spot.StrengthTree[(HandCategory)selectedCategory];
             
                }
                NotifyOfPropertyChange(() => TypeList);
            }
        }

        public HandType? SelectedType
        {
            get { return selectedType; }
            set
            {
                selectedType = value;
                if(selectedType != null)
                {
                    graphVisibility = "Visible";
                    determineButtonVisibility();
                    placeholderVisibility = "Hidden";
                    spot.LoadPlotModel((HandType)selectedType);
                    NotifyOfPropertyChange(() => PlotModel);
                    NotifyOfPropertyChange(() => GraphVisibility);
                    NotifyOfPropertyChange(() => PlaceholderVisibility);
                }
            }
        }

        private void determineButtonVisibility()
        {
            if(graphVisibility == "Visible")
            {
                switch (Properties.Settings.Default.Theme)
                {
                    case ("Matrix"):
                        pillVisibility = "Visible";
                        btnVisibility = "Hidden";
                        break;
                    default:
                        pillVisibility = "Hidden";
                        btnVisibility = "Visible";
                        break;

                }
            } else
            {
                pillVisibility = "Hidden";
                btnVisibility = "Hidden";
            }
            NotifyOfPropertyChange(() => btnVisibility);
            NotifyOfPropertyChange(() => pillVisibility);
        }

        public void BrowseHands()
        {
            string windowTitle = "Source: " + spot.Report.FilePath;
            manager.ShowWindow(new HandBrowserViewModel(spot.HandGroup,windowTitle), null, null);
            // ActivateItem(new HandBrowserViewModel(spot.HandGroup));
        }

        public void PillBrowse()
        {
            BrowseHands();
        }

        public void OpenPreferences()
        {
            manager.ShowDialog(new PreferencesViewModel());

            if (selectedType != null && typeList.Contains((HandType)selectedType))
            {
                spot.LoadPlotModel((HandType)selectedType);
                NotifyOfPropertyChange(() => PlotModel);
            } else
            {
                BtnClearGraph();
            }

            updatePlaceholderLink();
            determineButtonVisibility();

            NotifyOfPropertyChange(() => BackgroundColour);
            NotifyOfPropertyChange(() => ForegroundColour);
            NotifyOfPropertyChange(() => Font);
            NotifyOfPropertyChange(() => FontWeight);
        }

        private void updatePlaceholderLink()
        {
            switch(Properties.Settings.Default.Theme)
            {
                case ("Ariana Grande"):
                    PlaceholderLink = "/Images/GrandeTato.gif";
                    break;
                case ("Matrix"):
                    PlaceholderLink = "/Images/MatriTato.gif";
                    break;
                default:
                    PlaceholderLink = "/Images/Placeholder.gif";
                    break;
            }

            NotifyOfPropertyChange(() => PlaceholderLink);
        }

        public void RefreshPath()
        {
            staticTimer.start("Total");
            if (System.IO.Directory.Exists(reportsPath))
            {
                reader = new ReportReader(reportsPath);
                if(reader.UnprocessedReports.Count != 0)
                {
                    pathFeedback = $"Some report folders were names incorrectly ({reader.UnprocessedReports[0].GetReportDirectory()})";
                } else
                {
                    pathFeedback = "";
                }
                NotifyOfPropertyChange(() => pathFeedback);
                actions.Clear();
                foreach (string action in reader.GetActionList())
                {
                    actions.Add(action);
                }
                NotifyOfPropertyChange(() => actions);
                NotifyOfPropertyChange(() => reportsPath);
            }
            staticTimer.stop("Total");
            staticTimer.log("Total");
            
        }

        private void loadReport()
        {
            staticTimer.reset();
            staticTimer.start("Total");
            Report report = reader.GetReport(selectedAction, selectedAggPos, selectedCllPos, selectedBoardType, selectedBoardSubtype);
            spot = new SpotModel(report);


            if (selectedCategory != null && spot.Categories.Contains((HandCategory)selectedCategory))
            {
                typeList = spot.StrengthTree[(HandCategory)selectedCategory];

            } else
            {
                typeList = null;
                selectedType = null;
            }

            // refresh typelist
            if (selectedType != null && typeList.Contains((HandType)selectedType))
            {
                spot.LoadPlotModel((HandType)selectedType);
                NotifyOfPropertyChange(() => PlotModel);
            }
            else
            {
                selectedType = null;
            }
            NotifyOfPropertyChange(() => Spot);
            staticTimer.stop("Total");
            staticTimer.log("Total");
        }

        

        public void UpdateSize(ShellView window)
        {
            
            if (window.ActualWidth < 800)
            {
                GraphRow = 1;
                GraphColumn = 0;
                // GraphWidth = (int)e.NewSize.Width - 40;
            }
            else
            {
                GraphRow = 0;
                GraphColumn = 1;
                // GraphWidth = (int)e.NewSize.Width - 340;
            }
            NotifyOfPropertyChange(() => GraphRow);
            
        }
    }
}
