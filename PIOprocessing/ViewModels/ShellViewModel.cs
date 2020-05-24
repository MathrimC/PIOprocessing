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

namespace PIOprocessing.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        IWindowManager manager = new WindowManager();


        
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
            placeholderVisibility = "Visible";
            graphRow = 3;
            graphColumn = 4;
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
            placeholderVisibility = "Visible";
            // spot.LoadPlotModel((HandType)selectedType);
            
            // NotifyOfPropertyChange(() => PlotModel);
            NotifyOfPropertyChange(() => GraphVisibility);
            NotifyOfPropertyChange(() => PlaceholderVisibility);
            NotifyOfPropertyChange(() => SelectedType);
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
                    placeholderVisibility = "Hidden";
                    spot.LoadPlotModel((HandType)selectedType);
                }
                NotifyOfPropertyChange(() => PlotModel);
                NotifyOfPropertyChange(() => GraphVisibility);
                NotifyOfPropertyChange(() => PlaceholderVisibility);
            }
        }

        public void BrowseHands()
        {
            manager.ShowWindow(new HandBrowserViewModel(spot.HandGroup), null, null);
            // ActivateItem(new HandBrowserViewModel(spot.HandGroup));
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

        public void update_size(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 420)
            {
                GraphRow = 10;
                GraphColumn = 1;
            } else
            {
                GraphRow = 3;
                GraphColumn = 4;
            }

        }

    }
}
