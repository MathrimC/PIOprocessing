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

namespace PIOprocessing.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
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
                    else
                    {
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


        public void RefreshPath()
        {
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
            // add error "directory doesn't exist" to textfield
        }

        private void loadReport()
        {
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
        }

    }
}
